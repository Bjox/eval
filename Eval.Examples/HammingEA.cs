using Eval.Core;
using Eval.Core.Config;
using Eval.Core.Models;
using Eval.Core.Selection.Adult;
using Eval.Core.Selection.Parent;
using Eval.Core.Util.EARandom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Eval.Examples
{
    class StringGenotype : Genotype
    {
        public string str;

        public StringGenotype(string str) : base()
        {
            this.str = str;
        }

        public override IGenotype Clone()
        {
            var geno = new StringGenotype(string.Copy(str));
            return geno;
        }

        public override IGenotype CrossoverWith(IGenotype other, CrossoverType crossover, IRandomNumberGenerator random)
        {
            var newgeno = new StringGenotype(null);
            var og = other as StringGenotype;
            switch (crossover)
            {
                case CrossoverType.Uniform:
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < Math.Min(str.Length, og.str.Length); i++)
                        sb.Append(random.NextBool() ? str[i] : og.str[i]);
                    newgeno.str = sb.ToString();
                    break;

                case CrossoverType.OnePoint:
                    var point = random.Next(Math.Min(str.Length, og.str.Length));
                    newgeno.str = str.Substring(0, point) + og.str.Substring(point);
                    break;

                default: throw new NotImplementedException(crossover.ToString());
            }
            return newgeno;
        }

        public override bool Equals(object obj)
        {
            var genotype = obj as StringGenotype;
            return genotype != null && str == genotype.str;
        }

        public override int GetHashCode()
        {
            return -1349951472 + EqualityComparer<string>.Default.GetHashCode(str);
        }

        public override void Mutate(double probability, IRandomNumberGenerator random)
        {
            if (random.NextDouble() >= probability)
                return;

            StringBuilder sb = new StringBuilder();
            sb.Append(str);
            if (str.Length == 0)
                sb.Append((char)random.Next(32, 122));
            else
                sb[random.Next(str.Length)] = (char)random.Next(32, 122);

            if (random.NextDouble() < 0.25)
            {
                if (random.NextBool())
                    sb.Append((char)random.Next(32, 122));
                else
                    sb.Remove(sb.Length - 1, 1);
            }

            str = sb.ToString();
        }

        public override string ToString()
        {
            return str;
        }
        
    }

    class HammingPhenotype : Phenotype
    {
        private StringGenotype geno;

        public HammingPhenotype(IGenotype genotype) : base(genotype)
        {
            geno = genotype as StringGenotype;
        }

        protected override double CalculateFitness()
        {
            int hammingdist = 2 * Math.Abs(geno.str.Length - HammingEA.TARGET.Length);
            for (int i = 0; i < Math.Min(geno.str.Length, HammingEA.TARGET.Length); i++)
                hammingdist += geno.str[i] == HammingEA.TARGET[i] ? 0 : 1;
            return hammingdist;
        }

        public override string ToString()
        {
            return geno.ToString();
        }
    }

    /// <summary>
    /// Optimizes towards a given string. 
    /// This is a minimization problem using hamming distance for fitness function
    /// </summary>
    public class HammingEA : EA
    {
        public static string TARGET = "Lorem ipsum dolor sit amet";

        public HammingEA(IEAConfiguration config, IRandomNumberGenerator rng) : base(config, rng)
        {}

        protected override IPhenotype CreatePhenotype(IGenotype genotype)
        {
            var phenotype = new HammingPhenotype(genotype);
            return phenotype;
        }

        protected override IPhenotype CreateRandomPhenotype()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < RNG.Next(1, 10); i++)
                sb.Append((char)RNG.Next(32, 122));
            HammingPhenotype p = new HammingPhenotype(new StringGenotype(sb.ToString()));
            return p;
        }


        public static void Run()
        {
            var config = new EAConfiguration
            {
                PopulationSize = 100,
                OverproductionFactor = 2.0,
                MaximumGenerations = 100000,
                CrossoverType = CrossoverType.OnePoint,
                AdultSelectionType = AdultSelectionType.GenerationalMixing,
                ParentSelectionType = ParentSelectionType.Tournament,
                CrossoverRate = 0.9,
                MutationRate = 0.25,
                TournamentSize = 10,
                TournamentProbability = 0.8,
                TargetFitness = 0.0,
                Mode = EAMode.MinimizeFitness,
                Elites = 1,
                CalculateStatistics = true
            };

            var hammingEA = new HammingEA(config, new DefaultRandomNumberGenerator());

            var stopwatchtot = new Stopwatch();
            var stopwatchgen = new Stopwatch();

            PopulationStatistics currentStats = new PopulationStatistics();

            hammingEA.PopulationStatisticsCalculated += (stats) =>
            {
                currentStats = stats;
            };
            
            hammingEA.NewBestFitnessEvent += (pheno) => {
                var gen = hammingEA.Generation;

                double progress = (gen / (double)config.MaximumGenerations) * 100.0;
                var totruntime = stopwatchtot.Elapsed;
                var genruntime = stopwatchgen.Elapsed;
                Console.WriteLine();
                Console.WriteLine(string.Format("G# {0}    best_f: {1:F2}    avg_f: {2:F2}    SD: {3:F2}    Progress: {4,5:F2}    Gen: {5}   Tot: {6}", gen, hammingEA.Best.Fitness, currentStats.AverageFitness, currentStats.StandardDeviationFitness, progress, genruntime, totruntime));
                Console.WriteLine("Generation winner: " + ((StringGenotype)hammingEA.Best?.Genotype));

                stopwatchgen.Restart();
            };

            stopwatchtot.Start();
            stopwatchgen.Start();

            var res = hammingEA.Evolve();

            Console.WriteLine("\n\nDone!");
            Console.WriteLine($"Winner: {res.Winner}");
            WriteResultToFile(hammingEA.PopulationStatistics);
            Console.Read();
        }
            
        private static void WriteResultToFile(List<PopulationStatistics> stats)
        {
            using (StreamWriter file = new StreamWriter($"hamming_{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}.csv"))
            {
                file.WriteLine("MinFitness,MaxFitness,AverageFitness,StandardDeviationFitness,VarianceFitness");
                foreach (var stat in stats)
                {
                    file.WriteLine($"{stat.MinFitness},{stat.MaxFitness},{stat.AverageFitness},{stat.StandardDeviationFitness},{stat.VarianceFitness}");
                }
            }
        }
    }
}
