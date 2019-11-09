using Eval.Core;
using Eval.Core.Config;
using Eval.Core.Models;
using Eval.Core.Selection.Adult;
using Eval.Core.Selection.Parent;
using Eval.Core.Util;
using Eval.Core.Util.EARandom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eval.Examples
{
    [Serializable]
    class LOLZPhenotype : Phenotype
    {
        private int _Z;
        private int _lolzcount;
        
        public LOLZPhenotype(IGenotype genotype, int z)
            : base(genotype)
        {
            _Z = z;
        }

        protected override double CalculateFitness()
        {
            var geno = (BinaryGenotype)Genotype;
            var prefix = geno.Bits[0];
            var lolz = 0;

            var evaluationEndpoint = prefix ? geno.Bits.Count : _Z;
            for (int i = 0; i < evaluationEndpoint; i++)
            {
                if (geno.Bits[i] != prefix)
                    break;
                lolz++;
            }

            _lolzcount = lolz;

            return lolz / (double)geno.Bits.Count;
        }

        public override string ToString()
        {
            return $"[{((BinaryGenotype)Genotype).ToBitString()}]  lolz={_lolzcount}";
        }
    }

    /// <summary>
    /// Leading Ones, Leading Zeroes.
    /// Optimizes towards a bitstring containing only ones, 
    /// but introduces a local maxima when the bitstring has leading zeroes.
    /// With leading zeroes the fitness is only evaluated up to a given point Z in the bitstring.
    /// </summary>
    [Serializable]
    public class LOLZEA : EA
    {
        private int _bitcount = 40;
        private int _z = 21;

        public LOLZEA(IEAConfiguration config, IRandomNumberGenerator rng) 
            : base(config, rng)
        {
            
        }

        protected override IPhenotype CreatePhenotype(IGenotype genotype)
        {
            var phenotype = new LOLZPhenotype(genotype, _z);
            return phenotype;
        }

        protected override IPhenotype CreateRandomPhenotype()
        {
            var g = new BinaryGenotype(_bitcount);
            for (int i = 0; i < _bitcount; i++)
            {
                g.Bits[i] = RNG.NextBool();
            }
            var p = new LOLZPhenotype(g, _z);
            return p;
        }


        public static void Run()
        {
            var config = new EAConfiguration
            {
                PopulationSize = 13,
                OverproductionFactor = 1.5,
                MaximumGenerations = 10000,
                CrossoverType = CrossoverType.Uniform,
                AdultSelectionType = AdultSelectionType.Overproduction,
                ParentSelectionType = ParentSelectionType.Tournament,
                CrossoverRate = 0.21,
                MutationRate = 0.98,
                TournamentSize = 7,
                TournamentProbability = 0.895,
                TargetFitness = 1.0,
                Mode = EAMode.MaximizeFitness,
                Elites = 0,
                CalculateStatistics = true
            };

            var lolzea = new LOLZEA(config, new DefaultRandomNumberGenerator());

            var stopwatchtot = new Stopwatch();
            var stopwatchgen = new Stopwatch();

            PopulationStatistics currentStats = new PopulationStatistics();
            
            lolzea.PopulationStatisticsCalculated += (stats) =>
            {
                currentStats = stats;
            };
            lolzea.NewGenerationEvent += (gen) => {
                //PrintProgressBar(gen, config.MaximumGenerations);

                var progress = (gen / (double)config.MaximumGenerations) * 100.0;
                var totruntime = stopwatchtot.Elapsed;
                var genruntime = stopwatchgen.Elapsed;
                Console.WriteLine();
                Console.WriteLine(string.Format("G# {0}    best_f: {1:F3}    avg_f: {2:F3}    SD: {3:F3}    Progress: {4,5:F2}    Gen: {5}   Tot: {6}", gen, lolzea.Best.Fitness, currentStats.AverageFitness, currentStats.StandardDeviationFitness, progress, genruntime, totruntime));
                Console.WriteLine($"Generation winner: {lolzea.Best.ToString()}");

                stopwatchgen.Restart();
            };

            stopwatchtot.Start();
            stopwatchgen.Start();

            var res = lolzea.Evolve();

            Console.WriteLine("\n\nDone!");
            Console.WriteLine($"Winner: {res.Winner}");
            Console.Read();
        }

        private static void PrintProgressBar(int gen, int genmax)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\r");
            ConsoleProgressBar.BuildProgressBar(sb, 50, gen, genmax);
            sb.Append("  Gen: ");
            sb.Append(gen);
            Console.Write(sb.ToString());
            Console.Out.Flush();
        } 
        
    }
}
