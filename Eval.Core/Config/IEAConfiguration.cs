using Eval.Core.Models;
using Eval.Core.Selection.Adult;
using Eval.Core.Selection.Parent;

namespace Eval.Core.Config
{
    public interface IEAConfiguration
    {
        // TODO: doc
        int PopulationSize { get; }
        double OverproductionFactor { get; }
        int MaximumGenerations { get; }
        CrossoverType CrossoverType { get; }
        AdultSelectionType AdultSelectionType { get; }
        ParentSelectionType ParentSelectionType { get; }
        double CrossoverRate { get; }
        double MutationRate { get; }
        int TournamentSize { get; }
        double TournamentProbability { get; }
        double TargetFitness { get; }
        EAMode Mode { get; }
        int Elites { get; }
        bool ReevaluateElites { get; }
    }
}
