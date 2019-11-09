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
    [Serializable]
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
