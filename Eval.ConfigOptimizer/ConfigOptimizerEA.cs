﻿using Eval.Core;
using Eval.Core.Config;
using Eval.Core.Models;
using Eval.Core.Selection.Adult;
using Eval.Core.Selection.Parent;
using Eval.Core.Util.EARandom;
using System;

namespace Eval.ConfigOptimizer
{
    public abstract class ConfigOptimizerEA : EA
    {
        public abstract bool TargetEA_ReevaluateElites { get; }
        public abstract int TargetEA_MaximumGenerations { get; }
        public abstract double TargetEA_TargetFitness { get; }
        public abstract EAMode TargetEA_Mode { get; }
        public abstract int TargetEA_FitnessRuns { get; }

        private readonly bool _verbose;

        public ConfigOptimizerEA(IEAConfiguration optimizerConfig, IRandomNumberGenerator random, bool verbose = true)
            : base(optimizerConfig, random)
        {
            _verbose = verbose;
            if (verbose)
            {
                NewBestFitnessEvent += (best, gen) =>
                {
                    Console.WriteLine($"\nNew best at generation #{gen}: fitness = {best.Fitness}");
                    Console.WriteLine(best.ToString());
                    Console.WriteLine();
                };
                NewGenerationEvent += (gen) => Console.Write($"-{gen}");
                FitnessLimitReachedEvent += (fitness) => Console.WriteLine($"\nFitness limit reached ({fitness})");
                GenerationLimitReachedEvent += (gen) => Console.WriteLine($"\nGeneration limit reached ({gen})");
            }
        }

        protected override IPhenotype CreatePhenotype(IGenotype genotype)
        {
            var configGeno = genotype as ConfigGenotype;
            var targetEA = CreateTargetEA(configGeno, CreateRandomNumberGenerator());
            return new ConfigPhenotype(configGeno, TargetEA_FitnessRuns, targetEA);
        }

        protected override IPhenotype CreateRandomPhenotype()
        {
            var geno = CreateRandomizedConfigGenotype();
            geno.Randomize(RNG);

            geno.ReevaluateElites = TargetEA_ReevaluateElites;
            geno.MaximumGenerations = TargetEA_MaximumGenerations;
            geno.TargetFitness = TargetEA_TargetFitness;
            geno.Mode = TargetEA_Mode;

            return CreatePhenotype(geno);
        }

        public override EAResult Evolve()
        {
            var result = base.Evolve();
            if (_verbose)
            {
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Winner:");
                Console.WriteLine(result.Winner);
            }
            return result;
        }

        protected abstract EA CreateTargetEA(IEAConfiguration targetConfig, IRandomNumberGenerator random);
        protected abstract ConfigGenotype CreateRandomizedConfigGenotype();
        protected abstract IRandomNumberGenerator CreateRandomNumberGenerator();

        public static EAConfiguration DefaultOptimizerConfig => new EAConfiguration
        {
            PopulationSize = 100,
            OverproductionFactor = 1.0,
            MaximumGenerations = 10000,
            CrossoverType = CrossoverType.Uniform,
            AdultSelectionType = AdultSelectionType.GenerationalMixing,
            ParentSelectionType = ParentSelectionType.Tournament,
            CrossoverRate = 0.8,
            MutationRate = 0.3,
            TournamentSize = 10,
            TournamentProbability = 0.8,
            TargetFitness = 999999,
            Mode = EAMode.MaximizeFitness,
            Elites = 1,
            CalculateStatistics = true,
            MultiThreaded = true,
            ReevaluateElites = false
        };
    }
}
