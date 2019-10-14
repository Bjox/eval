using System;
using System.Collections.Generic;
using System.Text;

namespace Eval.Core.Models
{
    public abstract class Genotype
    {
        public abstract Genotype Clone();
        public abstract Genotype CrossoverWith(Genotype other, CrossoverType crossover);
        public abstract void Mutate(double factor);
    }
}
