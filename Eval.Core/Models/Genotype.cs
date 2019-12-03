using System;
using Eval.Core.Util.EARandom;

namespace Eval.Core.Models
{
    [Serializable]
    public abstract class Genotype : IGenotype
    {
        public abstract IGenotype Clone();

        public abstract IGenotype CrossoverWith(IGenotype other, CrossoverType crossover, IRandomNumberGenerator random);

        public abstract void Mutate(double probability, IRandomNumberGenerator random);
    }
}
