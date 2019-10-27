using Eval.Core.Models;
using Eval.Core.Util.EARandom;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eval.Test.Unit.Models
{
    /// <summary>
    /// Extend from this abstract class in order to test a custom genotype inheriting <c>AbstractListGenotype</c>.
    /// </summary>
    /// <typeparam name="GType">The genotype Type being tested</typeparam>
    /// <typeparam name="AType">The array type being used in GType</typeparam>
    /// <typeparam name="EType">The element type of AType</typeparam>
    [TestClass]
    public abstract class AbstractListGenotypeTestBase<GType, AType, EType>
        where GType : AbstractListGenotype<AType, EType>
        where AType : IList<EType>
    {
        private const int genoLength = 10; // Must be even

        private Mock<IRandomNumberGenerator> randomMock;
        private IRandomNumberGenerator random;
        private GType g1;
        private GType g2;
        private EType g1Values;
        private EType g2Values;

        [TestInitialize]
        public void TestInitialize()
        {
            genoLength.Should().Match(l => l % 2 == 0);
            genoLength.Should().BeGreaterThan(1);

            randomMock = new Mock<IRandomNumberGenerator>();
            random = new DefaultRandomNumberGenerator();
            
            g1 = GetFirstGenotype(CreateElements);
            g2 = GetSecondGenotype(CreateElements);

            g1.Should().HaveCount(genoLength);
            g2.Should().HaveCount(genoLength);
            g1[0].Should().NotBeSameAs(g1[1], "elements of a genotype should not reference the same object");
            g2[0].Should().NotBeSameAs(g2[1], "elements of a genotype should not reference the same object");

            g1Values = g1[0];
            g2Values = g2[0];
            g1Values.Equals(g2Values).Should().BeFalse("elements of g1 and g2 should not be equal for the sake of testing");
        }

        private IEnumerable<EType> CreateElements(Func<EType> elementFactory)
        {
            for (int i = 0; i < genoLength; i++)
            {
                yield return elementFactory();
            }
        }

        protected abstract GType GetFirstGenotype(Func<Func<EType> ,IEnumerable<EType>> elementFactory);
        protected abstract GType GetSecondGenotype(Func<Func<EType>, IEnumerable<EType>> elementFactory);

        [TestMethod]
        public void CrossoverShouldCreateGenotypeOfSameLength()
        {
            ((GType)g1.CrossoverWith(g2, CrossoverType.OnePoint, randomMock.Object))
                .Should().HaveCount(genoLength);
            ((GType)g2.CrossoverWith(g1, CrossoverType.OnePoint, randomMock.Object))
                .Should().HaveCount(genoLength);
        }

        [TestMethod]
        public void OnePointCrossover_LeftmostSplitTest()
        {
            randomMock.Setup(rng => rng.Next(It.IsAny<int>()))
                .Returns(0);

            ((GType)g1.CrossoverWith(g2, CrossoverType.OnePoint, randomMock.Object))
                .Should().AllBeEquivalentTo(g2Values);

            ((GType)g2.CrossoverWith(g1, CrossoverType.OnePoint, randomMock.Object))
                .Should().AllBeEquivalentTo(g1Values);
        }

        [TestMethod]
        public void OnePointCrossover_RightmostSplitTest()
        {
            randomMock.Setup(rng => rng.Next(It.IsAny<int>()))
                .Returns(genoLength);

            ((GType)g1.CrossoverWith(g2, CrossoverType.OnePoint, randomMock.Object))
                .Should().AllBeEquivalentTo(g1Values);

            ((GType)g2.CrossoverWith(g1, CrossoverType.OnePoint, randomMock.Object))
                .Should().AllBeEquivalentTo(g2Values);
        }

        [TestMethod]
        public void OnePointCrossover_MiddleSplitTest()
        {
            randomMock.Setup(rng => rng.Next(It.IsAny<int>()))
                .Returns(genoLength / 2);

            var g1Crossoverg2 = ((GType)g1.CrossoverWith(g2, CrossoverType.OnePoint, randomMock.Object));
            g1Crossoverg2.Should().StartWith(Enumerable.Repeat(g1Values, genoLength / 2));
            g1Crossoverg2.Should().EndWith(Enumerable.Repeat(g2Values, genoLength / 2));

            var g2Crossoverg1 = ((GType)g2.CrossoverWith(g1, CrossoverType.OnePoint, randomMock.Object));
            g2Crossoverg1.Should().StartWith(Enumerable.Repeat(g2Values, genoLength / 2));
            g2Crossoverg1.Should().EndWith(Enumerable.Repeat(g1Values, genoLength / 2));
        }

        [TestMethod]
        public void UniformCrossoverAllFromFirst()
        {
            randomMock.Setup(rng => rng.NextBool()).Returns(true);

            ((GType)g1.CrossoverWith(g2, CrossoverType.Uniform, randomMock.Object))
                .Should().AllBeEquivalentTo(g1Values);

            ((GType)g2.CrossoverWith(g1, CrossoverType.Uniform, randomMock.Object))
                .Should().AllBeEquivalentTo(g2Values);
        }

        [TestMethod]
        public void UniformCrossoverAllFromSecond()
        {
            randomMock.Setup(rng => rng.NextBool()).Returns(false);

            ((GType)g1.CrossoverWith(g2, CrossoverType.Uniform, randomMock.Object))
                .Should().AllBeEquivalentTo(g2Values);

            ((GType)g2.CrossoverWith(g1, CrossoverType.Uniform, randomMock.Object))
                .Should().AllBeEquivalentTo(g1Values);
        }

        [TestMethod]
        public void UniformCrossoverMixedFromBoth()
        {
            var setupNextBool = randomMock.SetupSequence(rng => rng.NextBool());
            for (int i = 0; i < genoLength; i++)
            {
                setupNextBool = setupNextBool.Returns(true).Returns(false);
            }

            var g1Crossoverg2 = ((GType)g1.CrossoverWith(g2, CrossoverType.Uniform, randomMock.Object));
            var g2Crossoverg1 = ((GType)g2.CrossoverWith(g1, CrossoverType.Uniform, randomMock.Object));

            for (int i = 0; i < genoLength; i += 2)
            {
                var odd = i + 1;
                var even = i;

                g1Crossoverg2[even].Should().Be(g1Values);
                g1Crossoverg2[odd].Should().Be(g2Values);

                g2Crossoverg1[even].Should().Be(g2Values);
                g2Crossoverg1[odd].Should().Be(g1Values);
            }
        }

        [TestMethod]
        public void ClonedGenotypeShouldEqualOriginal()
        {
            var clone = g1.Clone();
            clone.Should().Be(g1);
        }

        [TestMethod]
        public void MutatedGenotypeShouldNotEqualOriginal()
        {
            var clone = g1.Clone();
            clone.Mutate(1.0, random);
            clone.Should().NotBe(g1);
        }

        [TestMethod]
        public void MutateWithZeroProbabilityShouldNotMutateGenotype()
        {
            var original = g1.Clone();
            g1.Mutate(0.0, random);
            ((IGenotype)g1).Should().Be(original);
        }
    }
}
