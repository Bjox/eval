#region LICENSE
/* 
 * Distributed under the MIT License.
 * Copyright (c) 2019-2020 Viktor Zoric, Bjørnar Walle Alvestad, Endre Larsen
 * 
 * Read full license terms in the accompanying LICENSE file or at https://opensource.org/licenses/MIT
 */
#endregion

using System;

namespace Eval.Core.Models
{
    [Serializable]
    public abstract class Phenotype : IPhenotype
    {
        private double _fitness;

        public bool IsEvaluated { get; set; }
        public IGenotype Genotype { get; }
        public double Fitness
        { 
            get
            {
                if (!IsEvaluated)
                {
                    throw new Exception("Cannot get fitness of unevaluated phenotype");
                }
                return _fitness;
            }
            private set { _fitness = value; }
        }

        public Phenotype(IGenotype genotype)
        {
            Genotype = genotype;
        }

        public double Evaluate()
        {
            Fitness = CalculateFitness();
            IsEvaluated = true;
            return Fitness;
        }

        public int CompareTo(IPhenotype other)
        {
            return Fitness.CompareTo(other.Fitness);
        }

        protected abstract double CalculateFitness();

    }
}
