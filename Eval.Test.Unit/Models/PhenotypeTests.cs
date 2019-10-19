using System;
using Eval.Core.Models;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Eval.Test.Unit.Models
{
    [TestClass]
    public class PhenotypeTests
    {
        private class TestPhenotype : Phenotype
        {
            public Mock<IGenotype> GenotypeMock { get; }
            public double TestFitnessValue { get; }

            public TestPhenotype(Mock<IGenotype> genotypeMock, double fitnessValue)
                : base(genotypeMock.Object)
            {
                TestFitnessValue = fitnessValue;
                GenotypeMock = genotypeMock;
            }

            protected override double CalculateFitness()
            {
                return TestFitnessValue;
            }
        }

        private TestPhenotype phenotype;

        [TestInitialize]
        public void TestInitialize()
        {
            phenotype = new TestPhenotype(new Mock<IGenotype>(), 0.4);
        }

        [TestMethod]
        public void IsEvaluatedShouldReturnFalseIfNotEvaluated()
        {
            phenotype.IsEvaluated.Should().BeFalse();
            phenotype.Evaluate();
            phenotype.IsEvaluated.Should().BeTrue();
        }

        [TestMethod]
        public void GetFitnessShouldThrowIfNotEvaluated()
        {
            phenotype.Invoking(p => p.Fitness)
                .Should().Throw<Exception>();
        }

        [TestMethod]
        public void GetGenotypeShouldReturnCorrectValue()
        {
            phenotype.Genotype.Should().BeSameAs(phenotype.GenotypeMock.Object);
        }

        [TestMethod]
        public void EvaluateShouldSetFitnessValueAndIsEvaluatedFlag()
        {
            phenotype.Evaluate();
            phenotype.IsEvaluated.Should().BeTrue();
            phenotype.Fitness.Should().Be(0.4);
        }

    }
}
