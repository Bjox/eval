using Eval.Core.Config;
using Eval.Core.Models;
using Eval.Core.Selection.Adult;
using Eval.Core.Selection.Parent;
using System;

namespace Eval.ConfigOptimizer
{
    public class ConfigGenotype : PropertyGenotype<ConfigGenotype>, IEAConfiguration
    {
        public int PopulationSize { get; set; }
        public double OverproductionFactor { get; set; }
        public CrossoverType CrossoverType { get; set; }
        public AdultSelectionType AdultSelectionType { get; set; }
        public ParentSelectionType ParentSelectionType { get; set; }
        public double CrossoverRate { get; set; }
        public double MutationRate { get; set; }
        public int TournamentSize { get; set; }
        public double TournamentProbability { get; set; }
        public int Elites { get; set; }
        public double RankSelectionMinProbability { get; set; }
        public double RankSelectionMaxProbability { get; set; }

        public bool ReevaluateElites { get; set; }
        public int MaximumGenerations { get; set; }
        public double TargetFitness { get; set; }
        public EAMode Mode { get; set; }

        public bool CalculateStatistics => false;
        public bool MultiThreaded => false;

        public int SnapshotGenerationInterval => 0;
        public string SnapshotFilename => throw new NotImplementedException();

        public TimeSpan? MaxDuration => null;

        public ConfigGenotype()
            : base(builder => builder
                .DefineParameter(configGenotype => configGenotype.PopulationSize, 10, 200)
                .DefineParameter(configGenotype => configGenotype.OverproductionFactor, 1, 2)
                .DefineParameter(configGenotype => configGenotype.CrossoverType)
                .DefineParameter(configGenotype => configGenotype.AdultSelectionType)
                .DefineParameter(configGenotype => configGenotype.ParentSelectionType)
                .DefineParameter(configGenotype => configGenotype.CrossoverRate, 0, 1)
                .DefineParameter(configGenotype => configGenotype.MutationRate, 0, 1)
                .DefineParameter(configGenotype => configGenotype.TournamentSize, 2, 10)
                .DefineParameter(configGenotype => configGenotype.TournamentProbability, 0.5, 1)
                .DefineParameter(configGenotype => configGenotype.Elites, 0, 1)
                .DefineParameter(configGenotype => configGenotype.RankSelectionMinProbability, 0, 1)
                .DefineParameter(configGenotype => configGenotype.RankSelectionMaxProbability, 0, 10)
            )
        {
        }

        public ConfigGenotype(Func<Builder, Builder> builder)
            : base(builder)
        {
        }

        public ConfigGenotype(PropertyGenotypeElement[] elements)
            : base(elements)
        {
        }

        public virtual bool IsValidConfiguration()
        {
            if (TournamentSize > PopulationSize) return false;
            if (Elites > PopulationSize) return false;
            if (RankSelectionMinProbability > RankSelectionMaxProbability) return false;

            return true;
        }
    }
}
