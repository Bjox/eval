using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Eval.Core.Models;
using Eval.Core.Util;
using Eval.Core.Util.EARandom;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Eval.Test.Unit.Models
{
    [TestClass]
    public class BinaryGenotypeTests : AbstractListGenotypeTestBase<BinaryGenotype, BitArrayList, bool>
    {
        protected override BinaryGenotype GetFirstGenotype(Func<Func<bool>, IEnumerable<bool>> elementFactory)
        {
            return new BinaryGenotype(new BitArrayList(elementFactory(() => false).ToArray()));
        }

        protected override BinaryGenotype GetSecondGenotype(Func<Func<bool>, IEnumerable<bool>> elementFactory)
        {
            return new BinaryGenotype(new BitArrayList(elementFactory(() => true).ToArray()));
        }

        // Custom BinaryGenotype tests below this line ----------------------------------------------------

        private readonly int genoLength = 10;
        private BinaryGenotype g1;
        private BinaryGenotype g2;
        private Mock<IRandomNumberGenerator> randomMock;

        [TestInitialize]
        public void BinaryGenotypeTests_TestInitialize()
        {
            genoLength.Should().Match(l => l % 2 == 0);
            g1 = new BinaryGenotype(new BitArrayList(genoLength, false)); // All 0's
            g2 = new BinaryGenotype(new BitArrayList(genoLength, true));  // All 1's
            randomMock = new Mock<IRandomNumberGenerator>();
        }

        [TestMethod]
        public void BinaryGenotypeTests_OnePointCrossover_LeftmostSplitTest()
        {
            randomMock.Setup(rng => rng.Next(It.IsAny<int>()))
                .Returns(0);

            ((BinaryGenotype)g1.CrossoverWith(g2, CrossoverType.OnePoint, randomMock.Object))
                .Should().AllBeEquivalentTo(1);

            ((BinaryGenotype)g2.CrossoverWith(g1, CrossoverType.OnePoint, randomMock.Object))
                .Should().AllBeEquivalentTo(0);
        }

        [TestMethod]
        public void BinaryGenotypeTests_OnePointCrossover_RightmostSplitTest()
        {
            randomMock.Setup(rng => rng.Next(It.IsAny<int>()))
                .Returns(genoLength);

            ((BinaryGenotype)g1.CrossoverWith(g2, CrossoverType.OnePoint, randomMock.Object))
                .Should().AllBeEquivalentTo(0);

            ((BinaryGenotype)g2.CrossoverWith(g1, CrossoverType.OnePoint, randomMock.Object))
                .Should().AllBeEquivalentTo(1);
        }

        [TestMethod]
        public void BinaryGenotypeTests_OnePointCrossover_MiddleSplitTest()
        {
            randomMock.Setup(rng => rng.Next(It.IsAny<int>()))
                .Returns(genoLength / 2);

            ((BinaryGenotype)g1.CrossoverWith(g2, CrossoverType.OnePoint, randomMock.Object))
                .ToBitString().Should().Be("0000011111");

            ((BinaryGenotype)g2.CrossoverWith(g1, CrossoverType.OnePoint, randomMock.Object))
                .ToBitString().Should().Be("1111100000");
        }

        [TestMethod]
        public void BinaryGenotypeTests_MutateWith1ProbabilityShouldFlipAllBits()
        {
            var g = new BinaryGenotype(new BitArrayList(10000, false));

            g.Mutate(1.0, new DefaultRandomNumberGenerator("this is sed".GetHashCode()));

            g.Should().AllBeEquivalentTo(true);
        }

        [TestMethod]
        public void BinaryGenotypeTests_MutateWith0ProbabilityShouldFlipNoBits()
        {
            var g = new BinaryGenotype(new BitArrayList(10000, false));

            g.Mutate(0.0, new DefaultRandomNumberGenerator("this is sed".GetHashCode()));

            g.Should().AllBeEquivalentTo(false);
        }

        [TestMethod]
        public void BinaryGenotypeTests_MutateShouldFlipBitsProportionalToProbability()
        {
            var probabilities = new[] { 0.01, 0.1, 0.3, 0.5, 0.7, 0.99 };
            var n = 100000;

            foreach (var prob in probabilities)
            {
                var g = new BinaryGenotype(new BitArrayList(n, false));
                g.Mutate(prob, new DefaultRandomNumberGenerator("this is sed".GetHashCode()));
                (g.Count(b => b == true) / (double)n).Should().BeApproximately(prob, prob * 0.05); // 5% allowed delta
            }
        }

        [TestMethod]
        public void BinaryGenotypeTests_UniformCrossoverAllFromFirst()
        {
            randomMock.Setup(rng => rng.NextBool()).Returns(true);

            ((BinaryGenotype)g1.CrossoverWith(g2, CrossoverType.Uniform, randomMock.Object))
                .Should().AllBeEquivalentTo(false);

            ((BinaryGenotype)g2.CrossoverWith(g1, CrossoverType.Uniform, randomMock.Object))
                .Should().AllBeEquivalentTo(true);
        }

        [TestMethod]
        public void BinaryGenotypeTests_UniformCrossoverAllFromSecond()
        {
            randomMock.Setup(rng => rng.NextBool()).Returns(false);

            ((BinaryGenotype)g1.CrossoverWith(g2, CrossoverType.Uniform, randomMock.Object))
                .Should().AllBeEquivalentTo(true);

            ((BinaryGenotype)g2.CrossoverWith(g1, CrossoverType.Uniform, randomMock.Object))
                .Should().AllBeEquivalentTo(false);
        }

        [TestMethod]
        public void BinaryGenotypeTests_UniformCrossoverMixedFromBoth()
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

            ((BinaryGenotype)g1.CrossoverWith(g2, CrossoverType.Uniform, randomMock.Object))
                .ToBitString().Should().Be("0101010101");

            ((BinaryGenotype)g2.CrossoverWith(g1, CrossoverType.Uniform, randomMock.Object))
                .ToBitString().Should().Be("1010101010");
        }
    }

    [TestClass]
    public class BinaryGenotypeBaseTests : GenotypeTestBase<BinaryGenotype>
    {
        protected override BinaryGenotype CreateGenotype => new BinaryGenotype(10);
    }
}
