#region LICENSE
/* 
 * Distributed under the MIT License.
 * Copyright (c) 2019-2020 Viktor Zoric, Bjørnar Walle Alvestad, Endre Larsen
 * 
 * Read full license terms in the accompanying LICENSE file or at https://opensource.org/licenses/MIT
 */
#endregion

using System;
using Eval.Core.Config;
using Eval.Core.Models;
using Eval.Core.Selection.Parent;
using Eval.Core.Util.EARandom;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Eval.Test.Unit.Selection.Parent
{
    [TestClass]
    public class RankParentSelectionTests
    {
        [TestMethod]
        public void RankParentSelection_VerifySelectionProbability()
        {
            var random = new DefaultRandomNumberGenerator("seed".GetHashCode());
            var population = new Population(11);
            var index = 0;
            population.Fill(() => new TestPhenotype(index++, index * index * index * 0.1));
            population.Sort(EAMode.MaximizeFitness);

            var selection = new RankParentSelection(new EAConfiguration { RankSelectionMinProbability = 0, RankSelectionMaxProbability = 1 });
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
