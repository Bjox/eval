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
using System.Text;
using Eval.Core.Config;
using Eval.Core.Models;
using Eval.Core.Util.EARandom;
using Eval.Core.Util.Roulette;

namespace Eval.Core.Selection.Parent
{
    
    public class RankParentSelection : IParentSelection
    {
        public const double Min = 0.5;
        public const double Max = 1.5;

        private readonly double _min;
        private readonly double _max;

        public RankParentSelection(IEAConfiguration configuration)
        {
            _min = configuration.RankSelectionMinProbability;
            _max = configuration.RankSelectionMaxProbability;
        }

        /// <summary>
        /// Uses the default min and max probablities.
        /// </summary>
        public RankParentSelection()
        {
            _min = Min;
            _max = Max;
        }

        public IEnumerable<(IPhenotype, IPhenotype)> SelectParents(Population population, int n, EAMode mode, IRandomNumberGenerator random)
        {
            if (!population.IsSorted)
            {
                throw new ArgumentException("Population is not sorted");
            }
            var roulette = new AliasRoulette<IPhenotype>(random, population, (p, i) => GetProb(i, population.Size));
            for (int i = 0; i < n; i++)
            {
                yield return (roulette.Spin(), roulette.Spin());
            }
        }

        private double GetProb(int i, int popSize)
        {
            return _max - (_max - _min) * (i / (popSize - 1.0)); // gives highest prob for index 0
            //return (_max - _min) * (i / (popSize - 1.0)) + _min; // gives lowest prob for index 0
        }
    }
}
