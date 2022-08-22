#region LICENSE
/* 
 * Distributed under the MIT License.
 * Copyright (c) 2019-2020 Viktor Zoric, Bjørnar Walle Alvestad, Endre Larsen
 * 
 * Read full license terms in the accompanying LICENSE file or at https://opensource.org/licenses/MIT
 */
#endregion

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
    
    class OneMaxPhenotype : Phenotype
    {
        private int onecount;
        private int[] values;
        
        public OneMaxPhenotype(IGenotype genotype)
            : base(genotype)
        {

        }

        protected override double CalculateFitness()
        {
            BinaryGenotype geno = (BinaryGenotype)Genotype;
            onecount = 0;
            values = new int[geno.Bits.Count];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = geno.Bits[i] ? 1 : 0;
                onecount += geno.Bits[i] ? 1 : 0;
            }

            int value = 0;
            foreach (int n in values)
            {
                value += n;
            }

            return value / (double)values.Length;
        }

        public override string ToString()
        {
            return $"[{((BinaryGenotype)Genotype).ToBitString()}]";
        }
    }

    /// <summary>
    /// Optimizes towards a bitstring containing only ones 
    /// </summary>
    
    public class OneMaxEA : EA
    {
        private int _bitcount = 50;
        

        public OneMaxEA(IEAConfiguration config, IRandomNumberGenerator rng) 
            : base(config, rng)
        {
            
        }

        protected override IPhenotype CreatePhenotype(IGenotype genotype)
        {
            var phenotype = new OneMaxPhenotype(genotype);
            return phenotype;
        }

        protected override IPhenotype CreateRandomPhenotype()
        {
            BinaryGenotype g = new BinaryGenotype(_bitcount);
            for (int i = 0; i < _bitcount; i++)
                g.Bits[i] = RNG.NextBool();
            OneMaxPhenotype p = new OneMaxPhenotype(g);
            return p;
        }


        public static void Run()
        {
            var config = new EAConfiguration
            {
                PopulationSize = 100,
                OverproductionFactor = 2.0,
                MaximumGenerations = 100,
                CrossoverType = CrossoverType.OnePoint,
                AdultSelectionType = AdultSelectionType.GenerationalMixing,
                ParentSelectionType = ParentSelectionType.Tournament,
                CrossoverRate = 0.9,
                MutationRate = 0.01,
                TournamentSize = 10,
                TournamentProbability = 0.8,
                TargetFitness = 1.0,
                Mode = EAMode.MaximizeFitness,
                Elites = 1,
                CalculateStatistics = true
            };

            var onemaxEA = new OneMaxEA(config, new DefaultRandomNumberGenerator());

            var stopwatchtot = new Stopwatch();
            var stopwatchgen = new Stopwatch();

            PopulationStatistics currentStats = new PopulationStatistics();
            
            onemaxEA.PopulationStatisticsCalculated += (stats) =>
            {
                currentStats = stats;
            };
            onemaxEA.NewGenerationEvent += (gen) => {
                //PrintProgressBar(gen, config.MaximumGenerations);
                double progress = (gen / (double)config.MaximumGenerations) * 100.0;
                var totruntime = stopwatchtot.Elapsed;
                var genruntime = stopwatchgen.Elapsed;
                Console.WriteLine();
                Console.WriteLine(string.Format("G# {0}    best_f: {1:F3}    avg_f: {2:F3}    SD: {3:F3}    Progress: {4,5:F2}    Gen: {5}   Tot: {6}", gen, onemaxEA.Best.Fitness, currentStats.AverageFitness, currentStats.StandardDeviationFitness, progress, genruntime, totruntime));
                Console.WriteLine("Generation winner: " + ((BinaryGenotype)onemaxEA.Best?.Genotype).ToBitString());

                stopwatchgen.Restart();
            };

            stopwatchtot.Start();
            stopwatchgen.Start();

            var res = onemaxEA.Evolve();

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
