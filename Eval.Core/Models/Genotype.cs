using System;
using System.Collections.Generic;
using System.Text;
using Eval.Core.Util.EARandom;

namespace Eval.Core.Models
{
    [Serializable]
    public abstract class Genotype : IGenotype
    {
        public abstract IGenotype Clone();

        public abstract IGenotype CrossoverWith(IGenotype other, CrossoverType crossover, IRandomNumberGenerator random);

        public abstract void Mutate(double probability, IRandomNumberGenerator random);

        //public abstract override bool Equals(object obj);

        //public abstract override int GetHashCode();
    }
}
