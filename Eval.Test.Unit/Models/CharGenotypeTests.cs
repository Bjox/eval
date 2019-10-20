using System;
using System.Linq;
using Eval.Core.Models;
using Eval.Core.Util.EARandom;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Eval.Test.Unit.Models
{
    [TestClass]
    public class CharGenotypeTests
    {
        private readonly int genoLength = 10;
        private CharGenotype g1;
        private CharGenotype g2;
        private Mock<IRandomNumberGenerator> randomMock;

        [TestInitialize]
        public void TestInitialize()
        {
            genoLength.Should().Match(l => l % 2 == 0);
            g1 = new CharGenotype(Enumerable.Repeat('A', genoLength).ToArray()); // All A's
            g2 = new CharGenotype(Enumerable.Repeat('Z', genoLength).ToArray()); // All Z's'
            randomMock = new Mock<IRandomNumberGenerator>();
        }

        [TestMethod]
        public void OnePointCrossover_LeftmostSplitTest()
        {
            randomMock.Setup(rng => rng.Next(It.IsAny<int>()))
                .Returns(0);

            ((CharGenotype)g1.CrossoverWith(g2, Crossover.OnePoint, randomMock.Object))
                .Should().AllBeEquivalentTo('Z');

            ((CharGenotype)g2.CrossoverWith(g1, Crossover.OnePoint, randomMock.Object))
                .Should().AllBeEquivalentTo('A');
        }

        [TestMethod]
        public void OnePointCrossover_RightmostSplitTest()
        {
            randomMock.Setup(rng => rng.Next(It.IsAny<int>()))
                .Returns(genoLength);

            ((CharGenotype)g1.CrossoverWith(g2, Crossover.OnePoint, randomMock.Object))
                .Should().AllBeEquivalentTo('A');

            ((CharGenotype)g2.CrossoverWith(g1, Crossover.OnePoint, randomMock.Object))
                .Should().AllBeEquivalentTo('Z');
        }

        [TestMethod]
        public void OnePointCrossover_MiddleSplitTest()
        {
            randomMock.Setup(rng => rng.Next(It.IsAny<int>()))
                .Returns(genoLength / 2);

            ((CharGenotype)g1.CrossoverWith(g2, Crossover.OnePoint, randomMock.Object))
                .ToCharString().Should().Be("AAAAAZZZZZ");

            ((CharGenotype)g2.CrossoverWith(g1, Crossover.OnePoint, randomMock.Object))
                .ToCharString().Should().Be("ZZZZZAAAAA");
        }

        [TestMethod]
        public void MutateShouldMutateChars()
        {
            randomMock.SetupSequence(rng => rng.Next(It.IsAny<int>(), It.IsAny<int>()))
                .Returns('X')
                .Returns('Y')
                .Returns('Z');

            var g = new CharGenotype("AAA");
            g.Mutate(1, randomMock.Object);

            g.ToCharString().Should().Be("XYZ");
        }

        [TestMethod]
        public void UniformCrossoverAllFromFirst()
        {
            randomMock.Setup(rng => rng.NextBool()).Returns(true);

            ((CharGenotype)g1.CrossoverWith(g2, Crossover.Uniform, randomMock.Object))
                .Should().AllBeEquivalentTo('A');

            ((CharGenotype)g2.CrossoverWith(g1, Crossover.Uniform, randomMock.Object))
                .Should().AllBeEquivalentTo('Z');
        }

        [TestMethod]
        public void UniformCrossoverAllFromSecond()
        {
            randomMock.Setup(rng => rng.NextBool()).Returns(false);

            ((CharGenotype)g1.CrossoverWith(g2, Crossover.Uniform, randomMock.Object))
                .Should().AllBeEquivalentTo('Z');

            ((CharGenotype)g2.CrossoverWith(g1, Crossover.Uniform, randomMock.Object))
                .Should().AllBeEquivalentTo('A');
        }

        [TestMethod]
        public void UniformCrossoverMixedFromBoth()
        {
            var s = randomMock.SetupSequence(rng => rng.NextBool())
                .Returns(true)
                .Returns(false)
                .Returns(true)
                .Returns(false)
                .Returns(true)
                .Returns(false)
                .Returns(true)
                .Returns(false)
                .Returns(true)
                .Returns(false)
                .Returns(true)
                .Returns(false)
                .Returns(true)
                .Returns(false)
                .Returns(true)
                .Returns(false)
                .Returns(true)
                .Returns(false)
                .Returns(true)
                .Returns(false);

            ((CharGenotype)g1.CrossoverWith(g2, Crossover.Uniform, randomMock.Object))
                .ToCharString().Should().Be("AZAZAZAZAZ");

            ((CharGenotype)g2.CrossoverWith(g1, Crossover.Uniform, randomMock.Object))
                .ToCharString().Should().Be("ZAZAZAZAZA");
        }
    }
}
