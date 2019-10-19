using System;
using System.Collections;
using Eval.Core.Models;
using Eval.Core.Util.EARandom;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Eval.Test.Unit.Models
{
    [TestClass]
    public class BinaryGenotypeTests
    {
        private readonly int genoLength = 10;
        private BinaryGenotype g1;
        private BinaryGenotype g2;

        [TestInitialize]
        public void TestInitialize()
        {
            genoLength.Should().Match(l => l % 2 == 0);
            g1 = new BinaryGenotype(new BitArray(genoLength, false)); // All 0's
            g2 = new BinaryGenotype(new BitArray(genoLength, true));  // All 1's
        }

        [TestMethod]
        public void OnePointCrossover_LeftmostSplitTest()
        {
            var randomMock = new Mock<IRandomNumberGenerator>();
            randomMock.Setup(rng => rng.Next(It.IsAny<int>()))
                .Returns(0);

            ((BinaryGenotype)g1.CrossoverWith(g2, Crossover.OnePoint, randomMock.Object))
                .Should().AllBeEquivalentTo(1);

            ((BinaryGenotype)g2.CrossoverWith(g1, Crossover.OnePoint, randomMock.Object))
                .Should().AllBeEquivalentTo(0);
        }

        [TestMethod]
        public void OnePointCrossover_RightmostSplitTest()
        {
            var randomMock = new Mock<IRandomNumberGenerator>();
            randomMock.Setup(rng => rng.Next(It.IsAny<int>()))
                .Returns(genoLength);

            ((BinaryGenotype)g1.CrossoverWith(g2, Crossover.OnePoint, randomMock.Object))
                .Should().AllBeEquivalentTo(0);

            ((BinaryGenotype)g2.CrossoverWith(g1, Crossover.OnePoint, randomMock.Object))
                .Should().AllBeEquivalentTo(1);
        }

        [TestMethod]
        public void OnePointCrossover_MiddleSplitTest()
        {
            var randomMock = new Mock<IRandomNumberGenerator>();
            randomMock.Setup(rng => rng.Next(It.IsAny<int>()))
                .Returns(genoLength / 2);

            ((BinaryGenotype)g1.CrossoverWith(g2, Crossover.OnePoint, randomMock.Object))
                .ToBitString().Should().Be("0000011111");

            ((BinaryGenotype)g2.CrossoverWith(g1, Crossover.OnePoint, randomMock.Object))
                .ToBitString().Should().Be("1111100000");
        }

        [TestMethod]
        public void MutateWith1ProbabilityShouldFlipAllBits()
        {
            var g = new BinaryGenotype(new BitArray(10000, false));

            g.Mutate(1.0, new DefaultRandomNumberGenerator("this is sed".GetHashCode()));

            g.Should().AllBeEquivalentTo(true);
        }

        [TestMethod]
        public void MutateWith0ProbabilityShouldFlipNoBits()
        {
            var g = new BinaryGenotype(new BitArray(10000, false));

            g.Mutate(0.0, new DefaultRandomNumberGenerator("this is sed".GetHashCode()));

            g.Should().AllBeEquivalentTo(false);
        }
    }
}
