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

namespace Eval.Test.Unit.Selection.Parent
{
    class TestPhenotype : Phenotype
    {
        private readonly double _mockedFitness;
        public int Index { get; }
        public int Count { get; set; }

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

        /*
        [TestMethod]
        public void TournamentParentSelection_VerifySelectionProbability()
        {
            var population = new Population(11);
            var index = 0;
            population.Fill(() => new TestPhenotype(index++, index * index));
            population.Sort(EAMode.MaximizeFitness);

            var selection = new TournamentParentSelection();
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
        }*/
    }
}
