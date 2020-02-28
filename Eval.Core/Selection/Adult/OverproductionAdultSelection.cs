#region LICENSE
/* 
 * Distributed under the MIT License.
 * Copyright (c) 2019-2020 Viktor Zoric, Bjørnar Walle Alvestad, Endre Larsen
 * 
 * Read full license terms in the accompanying LICENSE file or at https://opensource.org/licenses/MIT
 */
#endregion

using Eval.Core.Config;
using Eval.Core.Models;
using Eval.Core.Util.EARandom;
using Eval.Core.Util.Roulette;
using System;

namespace Eval.Core.Selection.Adult
{
    [Serializable]
    public class OverproductionAdultSelection : IAdultSelection
    {
        private readonly IRandomNumberGenerator _rng;

        public OverproductionAdultSelection(IRandomNumberGenerator rng)
        {
            _rng = rng;
        }

        public void SelectAdults(Population offspring, Population population, int n, EAMode mode)
        {
            var roulette = new Roulette<IPhenotype>(_rng, offspring.Size);

            if (mode == EAMode.MaximizeFitness)
            {
                roulette.AddAll(offspring, e => e.Fitness);

                population.Clear();
                for (int i = 0; i < n; i++)
                {
                    population.Add(roulette.SpinAndRemove());
                }
            }
            else if (mode == EAMode.MinimizeFitness)
            {
                double p_max = Double.MinValue;
                double p_min = Double.MaxValue;

                foreach (var e in offspring)
                {
                    p_max = Math.Max(e.Fitness, p_max);
                    p_min = Math.Min(e.Fitness, p_min);
                }

                foreach (var e in offspring)
                    roulette.Add(e, (p_max + p_min) - e.Fitness);

                population.Clear();
                for (int i = 0; i < n; i++)
                {
                    population.Add(roulette.SpinAndRemove());
                }
            }
            else
            {
                throw new NotImplementedException(mode.ToString());
            }
        }
    }
}
