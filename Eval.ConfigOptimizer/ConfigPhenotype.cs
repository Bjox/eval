using Eval.Core;
using Eval.Core.Config;
using Eval.Core.Models;
using System;
using System.Diagnostics;
using System.Text;

namespace Eval.ConfigOptimizer
{
    public class ConfigPhenotype : Phenotype
    {
        public new ConfigGenotype Genotype => (ConfigGenotype)base.Genotype;
        public int FitnessRuns { get; }
        public double? TargetEAFitnessAverage { get; private set; } = null;
        public TimeSpan? TargetEARuntimeAverage { get; private set; } = null;

        private readonly EA targetEA;

        public ConfigPhenotype(ConfigGenotype genotype, int fitnessRuns, EA targetEA) : base(genotype)
        {
            FitnessRuns = fitnessRuns;
            this.targetEA = targetEA;
        }

        protected override double CalculateFitness()
        {
            if (!Genotype.IsValidConfiguration())
            {
                return 0;
            }

            var fitnessAvg = 0.0;
            var runtimeAvg = TimeSpan.Zero;

            for (int i = 0; i < FitnessRuns; i++)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                fitnessAvg += targetEA.Evolve().Winner.Fitness;
                stopwatch.Stop();
                runtimeAvg += stopwatch.Elapsed;
            }

            fitnessAvg /= FitnessRuns;
            runtimeAvg = TimeSpan.FromTicks(runtimeAvg.Ticks / FitnessRuns);

            TargetEARuntimeAverage = runtimeAvg;
            TargetEAFitnessAverage = fitnessAvg;

            return targetEA.EAConfiguration.Mode == EAMode.MaximizeFitness
                ? fitnessAvg / runtimeAvg.TotalSeconds
                : 1 / ((fitnessAvg + 1) * runtimeAvg.TotalSeconds);
        }

        public override string ToString()
        {
            var str = new StringBuilder();
            str.AppendLine("ConfigPhenotype");
            str.AppendLine($"Target EA average fitness: {TargetEAFitnessAverage}");
            str.AppendLine($"Target EA average runtime: {TargetEARuntimeAverage?.TotalSeconds} sec.");
            str.AppendLine(Genotype.ToString());
            return str.ToString();
        }
    }
}
