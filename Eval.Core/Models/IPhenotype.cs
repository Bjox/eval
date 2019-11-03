﻿using System;

namespace Eval.Core.Models
{
    public interface IPhenotype : IComparable<IPhenotype>
    {
        bool IsEvaluated { get; set; }
        IGenotype Genotype { get; }
        double Fitness { get; }
        double Evaluate();
    }
}
