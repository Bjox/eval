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
using System.Text;
using Eval.Core.Config;
using Eval.Core.Models;
using Eval.Core.Util.EARandom;
using Eval.Core.Util;
using Eval.Core.Util.Roulette;

namespace Eval.Core.Selection.Parent
{
    
    public class SigmaScalingParentSelection : IParentSelection
    {
        private const double S = 1; // I think this is configurable, but keep it as is until we are sure.

        public IEnumerable<(IPhenotype, IPhenotype)> SelectParents(Population population, int n, EAMode mode, IRandomNumberGenerator random)
        {
            var std = population.Select(p => p.Fitness).StandardDeviation();
            var avg = population.Select(p => p.Fitness).Average();

            var roulette = new AliasRoulette<IPhenotype>(random, population, p => S + (p.Fitness - avg) / (2 * std));

            for (int i = 0; i < n; i++)
            {
                yield return (roulette.Spin(), roulette.Spin());
            }
        }
    }
}
