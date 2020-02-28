#region LICENSE
/* 
 * Distributed under the MIT License.
 * Copyright (c) 2019-2020 Viktor Zoric, Bjørnar Walle Alvestad, Endre Larsen
 * 
 * Read full license terms in the accompanying LICENSE file or at https://opensource.org/licenses/MIT
 */
#endregion

using Eval.ConfigOptimizer;
using Eval.Core;
using Eval.Core.Config;
using Eval.Core.Util.EARandom;
using System.Threading;

namespace Eval.Examples.Hamming
{
    public class HammingConfigGenotype : ConfigGenotype
    {
        public HammingConfigGenotype()
            : base(builder => builder
                .DefineParameter(configGenotype => configGenotype.PopulationSize, 2, 100)
                .DefineParameter(configGenotype => configGenotype.OverproductionFactor, 1, 1)
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
    }

    public class HammingOptimizer : ConfigOptimizerEA
    {
        public override bool TargetEA_ReevaluateElites => false;
        public override int TargetEA_MaximumGenerations => 1000;
        public override double TargetEA_TargetFitness => 0.0;
        public override EAMode TargetEA_Mode => EAMode.MinimizeFitness;
        public override int TargetEA_FitnessRuns => 3;

        public HammingOptimizer(IEAConfiguration optimizerConfig, IRandomNumberGenerator random)
            : base(optimizerConfig, random)
        {
        }

        protected override EA CreateTargetEA(IEAConfiguration targetConfig, IRandomNumberGenerator random)
        {
            return new HammingEA(targetConfig, random);
        }

        protected override ConfigGenotype CreateRandomizedConfigGenotype()
        {
            return new HammingConfigGenotype();
        }

        protected override IRandomNumberGenerator CreateRandomNumberGenerator()
        {
            return new DefaultRandomNumberGenerator();
        }

        public static void Run()
        {
            var optimizerConfig = DefaultOptimizerConfig;
            optimizerConfig.PopulationSize = 100;
            optimizerConfig.MultiThreaded = true;

            var optimizer = new HammingOptimizer(optimizerConfig, new DefaultRandomNumberGenerator());
            optimizer.Evolve();
        }
    }
}
