using System;
using Eval.Core.Config;
using Eval.Core.Models;
using Eval.Core.Selection.Parent;
using Eval.Core.Util.EARandom;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Eval.Test.Unit.Selection.Parent
{
    class TestPhenotype : Phenotype
    {
        private readonly double _mockedFitness;
        public int Index { get; }

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
    public class RankParentSelectionTests
    {
        private IRandomNumberGenerator random;

        [TestInitialize]
        public void TestInitialize()
        {
            random = new DefaultRandomNumberGenerator("seed".GetHashCode());
        }

        [TestMethod]
        public void VerifySelectionProbability()
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
    }
}
