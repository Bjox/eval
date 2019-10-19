using System;
using System.Collections.Generic;
using Eval.Core;
using Eval.Core.Config;
using Eval.Core.Models;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Eval.Test.Unit.Models
{
    [TestClass]
    public class PopulationTests
    {
        private readonly int preferredPopSize = 120;
        private Population population;

        [TestInitialize]
        public void TestInitialize()
        {
            population = new Population(preferredPopSize);
        }

        [TestMethod]
        public void ShouldCreatePopulationOfCorrectSize()
        {
            population.Size.Should().Be(preferredPopSize);
        }

        [TestMethod]
        public void IsFilledFlagShouldNotInitiallyBeSet()
        {
            population.IsFilled.Should().BeFalse();
        }

        [TestMethod]
        public void FillShouldCallFactoryCorrectNumberOfTimes()
        {
            var phenotypeFactoryMock = new Mock<Func<IPhenotype>>();
            population.Fill(phenotypeFactoryMock.Object);
            phenotypeFactoryMock.Verify(factory => factory(), Times.Exactly(preferredPopSize));
        }

        [TestMethod]
        public void FillShouldPopulatePopulationWithCorrectElements()
        {
            var phenotypeMock = new Mock<IPhenotype>();
            population.Fill(() => phenotypeMock.Object);

            foreach (var individual in population)
            {
                individual.Should().NotBeNull();
                individual.Should().BeSameAs(phenotypeMock.Object);
            }
        }

        [TestMethod]
        public void FillShouldSetIsFilledFlag()
        {
            population.Fill(new Mock<Func<IPhenotype>>().Object);
            population.IsFilled.Should().BeTrue();
        }

        [TestMethod]
        public void EvaluateShouldThrowIfPopulationIsEmpty()
        {
            population.Invoking(p => p.Evaluate(false, e => { }))
                .Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void EvaluateShouldEvaluateAllAndCallEvaluatedEvent()
        {
            var (mocks, _) = FillWithMocks(population);

            var evaluatedEventMock = new Mock<Action<IPhenotype>>();
            population.Evaluate(false, evaluatedEventMock.Object);

            foreach (var mock in mocks)
            {
                mock.Verify(p => p.Evaluate(), Times.Once);
                evaluatedEventMock.Verify(e => e(mock.Object), Times.Once);
            }
        }

        [TestMethod]
        public void EvaluateShouldSkipAlreadyEvaluatedIndividuals()
        {
            var (mocks, _) = FillWithMocks(population,
                mock => mock.SetupGet(p => p.IsEvaluated).Returns(true));

            var evaluatedEventMock = new Mock<Action<IPhenotype>>();
            population.Evaluate(false, evaluatedEventMock.Object);

            foreach (var mock in mocks)
            {
                mock.Verify(p => p.Evaluate(), Times.Never);
                evaluatedEventMock.Verify(e => e(It.IsAny<IPhenotype>()), Times.Never);
            }
        }

        [TestMethod]
        public void EvaluateShouldReevaluateIfConfigured()
        {
            var (mocks, _) = FillWithMocks(population,
                mock => mock.SetupGet(p => p.IsEvaluated).Returns(true));

            var evaluatedEventMock = new Mock<Action<IPhenotype>>();
            population.Evaluate(true, evaluatedEventMock.Object);

            foreach (var mock in mocks)
            {
                mock.Verify(p => p.Evaluate(), Times.Once);
                evaluatedEventMock.Verify(e => e(mock.Object), Times.Once);
            }
        }

        [TestMethod]
        public void SortShouldThrowIfPopulationIsEmpty()
        {
            population.Invoking(p => p.Sort(EAMode.MaximizeFitness))
                .Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void WhenEAModeIsMaximize_SortShouldSortDescending()
        {
            var rng = new Random("this is seed".GetHashCode());
            var (mocks, _) = FillWithMocks(population,
                mock => mock.SetupGet(p => p.Fitness).Returns(rng.NextDouble()));

            population.Sort(EAMode.MaximizeFitness);

            population.Should().BeInDescendingOrder(i => i.Fitness);
        }

        [TestMethod]
        public void WhenEAModeIsMinimize_SortShouldSortAscending()
        {
            var rng = new Random("this is seed".GetHashCode());
            var (mocks, _) = FillWithMocks(population,
                mock => mock.SetupGet(p => p.Fitness).Returns(rng.NextDouble()));

            population.Sort(EAMode.MinimizeFitness);

            population.Should().BeInAscendingOrder(i => i.Fitness);
        }

        private static (IList<Mock<IPhenotype>>, Func<IPhenotype>) FillWithMocks(
            Population population,
            Action<Mock<IPhenotype>> setups = null)
        {
            var mocks = new List<Mock<IPhenotype>>();
            Func<IPhenotype> phenotypeMockFactory = () =>
            {
                var mock = new Mock<IPhenotype>();
                setups?.Invoke(mock);
                mocks.Add(mock);
                return mock.Object;
            };
            population.Fill(phenotypeMockFactory);
            return (mocks, phenotypeMockFactory);
        }
    }
}
