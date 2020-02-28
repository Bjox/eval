#region LICENSE
/* 
 * Distributed under the MIT License.
 * Copyright (c) 2019-2020 Viktor Zoric, Bjørnar Walle Alvestad, Endre Larsen
 * 
 * Read full license terms in the accompanying LICENSE file or at https://opensource.org/licenses/MIT
 */
#endregion

using Eval.Core.Models;
using Eval.Core.Selection.Adult;
using Eval.Core.Selection.Parent;
using System;

namespace Eval.Core.Config
{
    public interface IEAConfiguration
    {
        /// <summary>
        /// The number of individuals in the population.<br></br>
        /// Must be greater than or equal to 1.
        /// </summary>
        int PopulationSize { get; }
        /// <summary>
        /// Used to create a higher number of offsprings relative to to population size. <br></br>
        /// Used in conjunction with <c>OverproductionAdultSelection</c> and <c>GenerationalMixingAdultSelection</c>.<br></br>
        /// Must be greater than or equal to 1.0<br></br>
        /// Default value = 1.0
        /// </summary>
        double OverproductionFactor { get; }
        /// <summary>
        /// The maximum number of generations to run evolution.<br></br>
        /// Must be greater than or equal to 1.
        /// </summary>
        int MaximumGenerations { get; }
        /// <summary>
        /// The genotype crossover method to use.
        /// </summary>
        CrossoverType CrossoverType { get; }
        /// <summary>
        /// The adult selection strategy to use. This selection decides<br></br>
        /// which offspring to put into the population at each generation.<br></br>
        /// Default value = <c>GenerationalReplacement</c>
        /// </summary>
        AdultSelectionType AdultSelectionType { get; }
        /// <summary>
        /// The parent selection strategy to use. This selection decides<br></br>
        /// which individuals get to reproduce and produce offspring at each generation.<br></br>
        /// Default value = <c>FitnessProportionate</c>
        /// </summary>
        ParentSelectionType ParentSelectionType { get; }
        /// <summary>
        /// The rate at which genotype crossover is performed during reproduction.<br></br>
        /// This is the probability that crossover will be performed during a single reproduction.<br></br>
        /// Must be in the range <c>[0.0, 1.0]</c><br></br>
        /// Default value = 1.0
        /// </summary>
        double CrossoverRate { get; }
        /// <summary>
        /// The rate at which genotype mutation is performed during reproduction.<br></br>
        /// This is the probability of a single genome element being mutated.<br></br>
        /// Must be in the range <c>[0.0, 1.0]</c><br></br>
        /// Default value = 0.05
        /// </summary>
        double MutationRate { get; }
        /// <summary>
        /// The tournament size to use in tournament selection.<br></br>
        /// Used in conjunction with <c>TournamentParentSelection</c><br></br>
        /// Must be greather than or equal to 2<br></br>
        /// Default value = 10
        /// </summary>
        int TournamentSize { get; }
        /// <summary>
        /// The probability of selecting the best (most fit) individual as the winner of a tournament.<br></br>
        /// The complement is to select a random individual from the tournament as winner.<br></br>
        /// Used in conjunction with <c>TournamentParentSelection</c><br></br>
        /// Must be in the range <c>[0.0, 1.0]</c><br></br>
        /// </summary>
        double TournamentProbability { get; }
        /// <summary>
        /// The upper or lower fitness limit (based on the mode) of the evolution.<br></br>
        /// If the best individual reach or exceed this limit, the evolution is stopped.
        /// </summary>
        double TargetFitness { get; }
        /// <summary>
        /// The EA objective.<br></br>
        /// <c>MaximizeFitness</c>: Optimize towards high fitness values.<br></br>
        /// <c>MinimizeFitness</c>: Optimize towards low fitness values.
        /// </summary>
        EAMode Mode { get; }
        /// <summary>
        /// The number of elites to use during evolution.<br></br>
        /// This parameter specified the top <c>n</c> individuals to automatically transfer to the next generation.<br></br>
        /// Must be greater than or equal to 0.<br></br>
        /// Setting this to 0 means elitism is disabled.<br></br>
        /// Default value = 0
        /// </summary>
        int Elites { get; }
        /// <summary>
        /// Setting this flag will cause elites to be reevaluated (recalculates fitness) when they are transferred from one generation to the next.<br></br>
        /// Default value = false
        /// </summary>
        bool ReevaluateElites { get; }
        /// <summary>
        /// The wheight to be assigned to the least fit individual when using rank parent selection.<br></br>
        /// Used in conjunction with <c>RankParentSelection</c>.<br></br>
        /// Must be greater than or equal to 0.<br></br>
        /// Default value = 0.5
        /// </summary>
        double RankSelectionMinProbability { get; }
        /// <summary>
        /// The wheight to be assigned to the most fit individual when using rank parent selection.<br></br>
        /// Used in conjunction with <c>RankParentSelection</c>.<br></br>
        /// Must be greater than or equal to <c>RankSelectionMinProbability</c>.<br></br>
        /// Default value = 1.5
        /// </summary>
        double RankSelectionMaxProbability { get; }
        /// <summary>
        /// Flag to specify if statistics should be calcualted at each generation.<br></br>
        /// Default value = true
        /// </summary>
        bool CalculateStatistics { get; }
        /// <summary>
        /// Use ThreadPool for multithreading
        /// </summary>
        bool MultiThreaded { get; }
        /// <summary>
        /// Number of generations between each snapshot
        /// 0 or less will deactivate snapshots
        /// Default value = 0
        /// </summary>
        int SnapshotGenerationInterval { get; }
        /// <summary>
        /// Snapshot filename/full path to use.
        /// If this file exists upon evolution start, it will try to resume last state.
        /// Throws exception on serialization or deserialization failure
        /// </summary>
        string SnapshotFilename { get; }
        /// <summary>
        /// Maximum duration of the evolution.
        /// The evolution will stop at the end of the ongoing generation
        /// when total runtime is greater than this timespan
        /// </summary>
        TimeSpan? MaxDuration { get; }
    }
}
