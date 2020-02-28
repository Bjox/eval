#region LICENSE
/* 
 * Distributed under the MIT License.
 * Copyright (c) 2019-2020 Viktor Zoric, Bjørnar Walle Alvestad, Endre Larsen
 * 
 * Read full license terms in the accompanying LICENSE file or at https://opensource.org/licenses/MIT
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Eval.Core.Models;
using Eval.Core.Util.EARandom;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Eval.Test.Unit.Models
{
    [TestClass]
    public class CharGenotypeTests : AbstractListGenotypeTestBase<CharGenotype, char[], char>
    {
        protected override CharGenotype GetFirstGenotype(Func<Func<char>, IEnumerable<char>> elementFactory)
        {
            return new CharGenotype(elementFactory(() => 'A').ToArray());
        }

        protected override CharGenotype GetSecondGenotype(Func<Func<char>, IEnumerable<char>> elementFactory)
        {
            return new CharGenotype(elementFactory(() => 'Z').ToArray());
        }

        // Custom CharGenotype tests below this line ----------------------------------------------------

        private readonly int genoLength = 10;
        private CharGenotype g1;
        private CharGenotype g2;
        private Mock<IRandomNumberGenerator> randomMock;

        [TestInitialize]
        public void CharGenotypeTests_TestInitialize()
        {
            genoLength.Should().Match(l => l % 2 == 0);
            g1 = new CharGenotype(Enumerable.Repeat('A', genoLength).ToArray()); // All A's
            g2 = new CharGenotype(Enumerable.Repeat('Z', genoLength).ToArray()); // All Z's'
            randomMock = new Mock<IRandomNumberGenerator>();
        }

        [TestMethod]
        public void CharGenotypeTests_OnePointCrossover_LeftmostSplitTest()
        {
            randomMock.Setup(rng => rng.Next(It.IsAny<int>()))
                .Returns(0);

            ((CharGenotype)g1.CrossoverWith(g2, CrossoverType.OnePoint, randomMock.Object))
                .Should().AllBeEquivalentTo('Z');

            ((CharGenotype)g2.CrossoverWith(g1, CrossoverType.OnePoint, randomMock.Object))
                .Should().AllBeEquivalentTo('A');
        }

        [TestMethod]
        public void CharGenotypeTests_OnePointCrossover_RightmostSplitTest()
        {
            randomMock.Setup(rng => rng.Next(It.IsAny<int>()))
                .Returns(genoLength);

            ((CharGenotype)g1.CrossoverWith(g2, CrossoverType.OnePoint, randomMock.Object))
                .Should().AllBeEquivalentTo('A');

            ((CharGenotype)g2.CrossoverWith(g1, CrossoverType.OnePoint, randomMock.Object))
                .Should().AllBeEquivalentTo('Z');
        }

        [TestMethod]
        public void CharGenotypeTests_OnePointCrossover_MiddleSplitTest()
        {
            randomMock.Setup(rng => rng.Next(It.IsAny<int>()))
                .Returns(genoLength / 2);

            ((CharGenotype)g1.CrossoverWith(g2, CrossoverType.OnePoint, randomMock.Object))
                .ToCharString().Should().Be("AAAAAZZZZZ");

            ((CharGenotype)g2.CrossoverWith(g1, CrossoverType.OnePoint, randomMock.Object))
                .ToCharString().Should().Be("ZZZZZAAAAA");
        }

        [TestMethod]
        public void CharGenotypeTests_MutateShouldMutateChars()
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
        public void CharGenotypeTests_UniformCrossoverAllFromFirst()
        {
            randomMock.Setup(rng => rng.NextBool()).Returns(true);

            ((CharGenotype)g1.CrossoverWith(g2, CrossoverType.Uniform, randomMock.Object))
                .Should().AllBeEquivalentTo('A');

            ((CharGenotype)g2.CrossoverWith(g1, CrossoverType.Uniform, randomMock.Object))
                .Should().AllBeEquivalentTo('Z');
        }

        [TestMethod]
        public void CharGenotypeTests_UniformCrossoverAllFromSecond()
        {
            randomMock.Setup(rng => rng.NextBool()).Returns(false);

            ((CharGenotype)g1.CrossoverWith(g2, CrossoverType.Uniform, randomMock.Object))
                .Should().AllBeEquivalentTo('Z');

            ((CharGenotype)g2.CrossoverWith(g1, CrossoverType.Uniform, randomMock.Object))
                .Should().AllBeEquivalentTo('A');
        }

        [TestMethod]
        public void CharGenotypeTests_UniformCrossoverMixedFromBoth()
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

            ((CharGenotype)g1.CrossoverWith(g2, CrossoverType.Uniform, randomMock.Object))
                .ToCharString().Should().Be("AZAZAZAZAZ");

            ((CharGenotype)g2.CrossoverWith(g1, CrossoverType.Uniform, randomMock.Object))
                .ToCharString().Should().Be("ZAZAZAZAZA");
        }
    }

    [TestClass]
    public class CharGenotypeBaseTests : GenotypeTestBase<CharGenotype>
    {
        protected override CharGenotype CreateGenotype => new CharGenotype(new string('A', 10));
    }
}
