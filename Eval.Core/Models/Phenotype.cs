﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Eval.Core.Models
{
    public abstract class Phenotype : IPhenotype
    {
        private double _fitness;

        public bool IsEvaluated { get; private set; }
        public Genotype Genotype { get; }
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

        public Phenotype(Genotype genotype)
        {
            Genotype = genotype;
        }

        public double Evaluate()
        {
            Fitness = CalculateFitness();
            IsEvaluated = true;
            return Fitness;
        }

        protected abstract double CalculateFitness();
    }
}
