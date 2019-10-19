using Eval.Core.Util.EARandom;
using System;
using System.Collections.Generic;
using System.Text;

namespace Eval.Core.Models
{
    public interface IGenotype
    {
        IGenotype Clone();
        IGenotype CrossoverWith(IGenotype other, CrossoverType crossover, IRandomNumberGenerator random);
        void Mutate(double probability, IRandomNumberGenerator random);
    }
}
