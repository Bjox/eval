using System;
using System.Collections.Generic;
using System.Text;
using Eval.Core.Models;
using Eval.Core.Util.EARandom;
using Eval.Core.Util.Roulette;

namespace Eval.Core.Selection.Adult
{
    public class GenerationalMixingAdultSelection : IAdultSelection
    {
        private readonly IRandomNumberGenerator _rng;

        public GenerationalMixingAdultSelection(IRandomNumberGenerator rng)
        {
            _rng = rng;
        }

        public void SelectAdults(Population offspring, Population population, int n, bool maximizeFitness)
        {
            var roulette = new Roulette<Phenotype>(_rng, offspring.Size + population.Size);

            if (maximizeFitness)
            {

            }
            else
            {

            }
            
        }
    }
}
