using System;
using System.Diagnostics;
using System.Linq;
using Eval.Core.Config;
using Eval.Core.Models;
using Eval.Core.Selection.Parent;
using Eval.Core.Util.EARandom;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Eval.Test.Unit.Selection.Parent
{
    [TestClass]
    public class TournamentParentSelectionTests
    {
        [TestMethod]
        public void TournamentParentSelection_VerifySelectionProbability()
        {
            // Test tournament selection by comparing it to the old "established" tournament selection implementation.
            var random = new DefaultRandomNumberGenerator("seed".GetHashCode());
            for (int tournamentSize = 2; tournamentSize <= 10; tournamentSize++)
            {
                var population = new Population(11);
                var index = 0;
                population.Fill(() => new TestPhenotype(index++, index * index));
                population.Sort(EAMode.MaximizeFitness);

                var config = new EAConfiguration
                {
                    TournamentSize = 3,
                    TournamentProbability = 0.5
                };

#pragma warning disable CS0618 // Type or member is obsolete
                var oldselection = new TournamentParentSelectionOld(config);
#pragma warning restore CS0618 // Type or member is obsolete
                var oldselected = oldselection.SelectParents(population, 275000, EAMode.MaximizeFitness, random);

                foreach (var (a, b) in oldselected)
                {
                    ((TestPhenotype)a).Count2++;
                    ((TestPhenotype)b).Count2++;
                }

                var selection = new TournamentParentSelection(config);
                var selected = selection.SelectParents(population, 275000, EAMode.MaximizeFitness, random);

                foreach (var (a, b) in selected)
                {
                    ((TestPhenotype)a).Count++;
                    ((TestPhenotype)b).Count++;
                }

                var counts = population.Cast<TestPhenotype>().Select(p => p.Count).Zip(population.Cast<TestPhenotype>().Select(p => p.Count2), (c1, c2) => (c1, c2));

                foreach (var count in counts)
                {
                    (count.c1 - count.c2).Should().BeLessThan(1000, $"tournament size = {tournamentSize}");
                }
            }
        }

        [TestMethod]
        public void TournamentSelection_RandomBoundsBug()
        {
            var selection = new TournamentParentSelection(new EAConfiguration
            {
                TournamentSize = 10,
                TournamentProbability = 2
            });
            var pop = new Population(10);
            pop.Fill(() => CreatePhenotypeMock(1).Object);
            pop.Sort(EAMode.MaximizeFitness);

            // NextDouble() returns 0
            var randomMock = new Mock<IRandomNumberGenerator>();
            randomMock.Setup(rng => rng.NextDouble()).Returns(0);
            selection.Invoking(x => x.SelectParents(pop, 1, EAMode.MaximizeFitness, randomMock.Object).ToList()).Should().NotThrow();

            // NextDouble() returns the highest IEEE 754 float (binary64) less than 1
            randomMock = new Mock<IRandomNumberGenerator>();
            randomMock.Setup(rng => rng.NextDouble()).Returns(BitConverter.Int64BitsToDouble(0x3FE_FFFFFFFFFFFFF));
            selection.Invoking(x => x.SelectParents(pop, 1, EAMode.MaximizeFitness, randomMock.Object).ToList()).Should().NotThrow();
        }

        [TestMethod, Ignore]
        public void TournamentSelection_PerfTest()
        {
            var random = new DefaultRandomNumberGenerator();

            var config = new EAConfiguration
            {
                TournamentSize = 20,
                TournamentProbability = 0.5
            };

            var popsize = 10000;
            var pop = new Population(popsize);
            pop.Fill(() => CreatePhenotypeMock(random.NextDouble() * 10).Object);
            pop.Sort(EAMode.MaximizeFitness);

            Console.WriteLine($"popsize = {popsize}, tournament size = {config.TournamentSize}");

            var watch = new Stopwatch();
            watch.Start();
#pragma warning disable CS0618 // Type or member is obsolete
            IParentSelection selection = new TournamentParentSelectionOld(config);
#pragma warning restore CS0618 // Type or member is obsolete
            foreach (var selected in selection.SelectParents(pop, popsize, EAMode.MaximizeFitness, random))
            {
            }
            watch.Stop();
            Console.WriteLine($"old: {watch.ElapsedMilliseconds} ms");

            watch.Restart();
            selection = new TournamentParentSelection(config);
            foreach (var selected in selection.SelectParents(pop, popsize, EAMode.MaximizeFitness, random))
            {
            }
            watch.Stop();
            Console.WriteLine($"new: {watch.ElapsedMilliseconds} ms");
        }

        private static Mock<IPhenotype> CreatePhenotypeMock(double fitnessSetup)
        {
            var pmock = new Mock<IPhenotype>();
            pmock.SetupGet(p => p.Fitness).Returns(fitnessSetup);
            pmock.SetupGet(p => p.IsEvaluated).Returns(true);
            return pmock;
        }
    }
}
