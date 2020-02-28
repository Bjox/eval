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
using Eval.Core.Util.EARandom;
using Eval.Core.Util.Roulette;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Eval.Test.Unit.Util
{
    class Entry
    {
        public double Probability { get; set; }
        public int Count { get; set; }

        public Entry(double p)
        {
            Probability = p;
            Count = 0;
        }
    }

    [TestClass]
    public class AliasRouletteTests
    {
        private DefaultRandomNumberGenerator random;

        [TestInitialize]
        public void TestInitialize()
        {
            random = new DefaultRandomNumberGenerator("seed".GetHashCode());
        }

        [TestMethod]
        public void SimpleTest_NormalizedProbabilities()
        {
            var elements = new Entry[]
            {
                new Entry(0.1),
                new Entry(0.1),
                new Entry(0.5),
                new Entry(0.3)
            };

            AssertCount(elements);
        }

        [TestMethod]
        public void SimpleTest_NonNormalizedProbabilities()
        {
            var elements = new Entry[]
            {
                new Entry(1),
                new Entry(1),
                new Entry(5),
                new Entry(3)
            };

            AssertCount(elements);
        }

        [TestMethod]
        public void SimpleTest_BigProbabilityVariation()
        {
            var elements = new Entry[]
            {
                new Entry(1000),
                new Entry(100),
                new Entry(10),
                new Entry(1)
            };

            AssertCount(elements);
        }

        [TestMethod]
        public void SimpleTest_BiggerProbabilityVariation()
        {
            var elements = new Entry[]
            {
                new Entry(10000),
                new Entry(1),
                new Entry(1),
                new Entry(1)
            };

            AssertCount(elements);
        }

        [TestMethod]
        public void SimpleTest_SingleEntry()
        {
            var elements = new Entry[]
            {
                new Entry(100),
            };

            AssertCount(elements, 100);
        }

        [TestMethod]
        public void ComplexTest_HighNumberOfEntries()
        {
            var elements = new Entry[10000];
            for (int i = 0; i < elements.Length; i++)
            {
                elements[i] = new Entry(random.NextDouble() * 100 + 100);
            }

            AssertCount(elements);
        }

        [TestMethod]
        public void ComplexTest_MediumNumberOfEntriesMediumProbabilityVariation()
        {
            var elements = new Entry[500];
            for (int i = 0; i < elements.Length; i++)
            {
                elements[i] = new Entry(random.NextDouble() * 1000 + 1);
            }

            AssertCount(elements);
        }

        [TestMethod]
        public void InitializingEmptyRouletteShouldThrow()
        {
            Action action = () => new AliasRoulette<Entry>(random, new Entry[0], e => e.Probability);
            action.Should().Throw<ArgumentException>();
        }

        private void AssertCount(IReadOnlyList<Entry> elements, int spins = 0)
        {
            var probabilitySum = elements.Select(e => e.Probability).Sum();
            if (spins == 0)
            {
                var p_min = elements.Min(e => e.Probability);
                spins = (int)(1000 / (p_min / probabilitySum));
            }
            Console.WriteLine($"spins: {spins}");

            var roulette = new AliasRoulette<Entry>(random, elements, e => e.Probability);

            for (int i = 0; i < spins; i++)
            {
                roulette.Spin().Count++;
            }

            foreach (var element in elements)
            {
                var expectedCount = (element.Probability / probabilitySum) * spins;
                var actualCount = (double)element.Count;
                actualCount.Should().BeApproximately(expectedCount, expectedCount * 0.1 + 10);

                //var expectedProbability = element.Probability / probabilitySum;
                //var actualProbability = element.Count / (double)spins;
                //actualProbability.Should().BeApproximately(expectedProbability, expectedProbability * 0.05);
            }
        }
    }
}
