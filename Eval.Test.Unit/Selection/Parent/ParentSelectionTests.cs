using System;
using System.Linq;
using Eval.Core.Config;
using Eval.Core.Models;
using Eval.Core.Selection.Parent;
using Eval.Core.Util.EARandom;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Eval.Core.Util;
using System.Diagnostics;

namespace Eval.Test.Unit.Selection.Parent
{
    class TestPhenotype : Phenotype
    {
        private readonly double _mockedFitness;
        public int Index { get; }
        public int Count { get; set; }
        public int Count2 { get; set; }

        public TestPhenotype(int index, double mockedFitness)
            : base(new Mock<IGenotype>().Object)
        {
            Index = index;
            _mockedFitness = mockedFitness;
            Evaluate();
        }

        protected override double CalculateFitness()
        {
            return _mockedFitness;
        }
    }

    [TestClass]
    public class ParentSelectionTests
    {
        private IRandomNumberGenerator random;

        [TestInitialize]
        public void TestInitialize()
        {
            random = new DefaultRandomNumberGenerator("seed".GetHashCode());
        }

        [TestMethod]
        public void SelectParentsShouldSelectCorrectNumber()
        {
            var population = new Population(20);
            for (int i = 0; i < population.Size; i++)
            {
                var pmock = new Mock<IPhenotype>();
                pmock.SetupGet(p => p.Fitness).Returns(i);
                pmock.SetupGet(p => p.IsEvaluated).Returns(true);
                population.Add(pmock.Object);
            }

            var selection = new ProportionateParentSelection();
            var selected = selection.SelectParents(population, 20, EAMode.MaximizeFitness, random);
            selected.Count().Should().Be(20);
        }

        [TestMethod]
        public void RankParentSelection_VerifySelectionProbability()
        {
            var population = new Population(11);
            var index = 0;
            population.Fill(() => new TestPhenotype(index++, index * index * index * 0.1));
            population.Sort(EAMode.MaximizeFitness);

            var selection = new RankParentSelection(0, 1);
            var selected = selection.SelectParents(population, 275000, EAMode.MaximizeFitness, random);

            var bucket = new int[population.Size];
            foreach (var (a, b) in selected)
            {
                bucket[((TestPhenotype)a).Index]++;
                bucket[((TestPhenotype)b).Index]++;
            }

            for (int i = 0; i < bucket.Length; i++)
            {
                bucket[i].Should().BeCloseTo(i * 10000, 1000);
            }
        }

        [TestMethod]
        public void ProportionateParentSelection_VerifySelectionProbability()
        {
            var population = new Population(11);
            var index = 0;
            population.Fill(() => new TestPhenotype(index, index++));
            population.Sort(EAMode.MaximizeFitness);

            var selection = new ProportionateParentSelection();
            var selected = selection.SelectParents(population, 275000, EAMode.MaximizeFitness, random);

            var bucket = new int[population.Size];
            foreach (var (a, b) in selected)
            {
                bucket[((TestPhenotype)a).Index]++;
                bucket[((TestPhenotype)b).Index]++;
            }

            for (int i = 0; i < bucket.Length; i++)
            {
                bucket[i].Should().BeCloseTo(i * 10000, 1000);
            }
        }

        [TestMethod]
        public void SigmaScalingParentSelection_VerifySelectionProbability()
        {
            var population = new Population(11);
            var index = 0;
            population.Fill(() => new TestPhenotype(index++, index * index));
            population.Sort(EAMode.MaximizeFitness);

            var selection = new SigmaScalingParentSelection();
            var selected = selection.SelectParents(population, 275000, EAMode.MaximizeFitness, random);

            foreach (var (a, b) in selected)
            {
                ((TestPhenotype)a).Count++;
                ((TestPhenotype)b).Count++;
            }

            var std = population.Select(p => p.Fitness).StandardDeviation();
            var avg = population.Select(p => p.Fitness).Average();
            var psum = population.Sum(p => 1 + (p.Fitness - avg) / (2 * std));

            foreach (TestPhenotype p in population)
            {
                var prob = (1 + (p.Fitness - avg) / (2 * std)) / psum;
                var expectedCount = prob * 275000 * 2;
                p.Count.Should().BeCloseTo((int)expectedCount, 1000);
            }
        }

        [TestMethod]
        public void TournamentParentSelection_VerifySelectionProbability()
        {
            // Test tournament selection by comparing it to the old "established" tournament selection implementation.

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

                var oldselection = new TournamentParentSelectionOld(config);
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
            IParentSelection selection = new TournamentParentSelectionOld(config);
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
