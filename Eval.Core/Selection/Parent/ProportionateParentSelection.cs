using System;
using System.Collections.Generic;
using Eval.Core.Config;
using Eval.Core.Models;
using Eval.Core.Util.EARandom;
using Eval.Core.Util.Roulette;

namespace Eval.Core.Selection.Parent
{
    [Serializable]
    public class ProportionateParentSelection : IParentSelection
    {
        public IEnumerable<(IPhenotype, IPhenotype)> SelectParents(Population population, int n, EAMode mode, IRandomNumberGenerator random)
        {
            var roulette = new AliasRoulette<IPhenotype>(random, population, population.GetProbabilitySelector(mode));
            for (int i = 0; i < n; i++)
            {
                yield return (roulette.Spin(), roulette.Spin());
            }
        }
    }
}
