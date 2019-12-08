using System;
using System.Collections.Generic;
using System.Text;

namespace Eval.Core.Models
{
    [Serializable]
    public class PopulationStatistics
    {
        public double MaxFitness { get; set; }
        public double MinFitness { get; set; }
        public double AverageFitness { get; set; }
        public double VarianceFitness { get; set; }
        public double StandardDeviationFitness { get; set; }

        public override string ToString()
        {
            return $"Max: {MaxFitness}   Min: {MinFitness}   Avg: {AverageFitness:0.0}   Var: {VarianceFitness:0.0}   Std: {StandardDeviationFitness:0.0}";
        }
    }
}
