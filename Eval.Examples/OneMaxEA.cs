using Eval.Core;
using Eval.Core.Config;
using Eval.Core.Models;
using Eval.Core.Selection.Adult;
using Eval.Core.Selection.Parent;
using Eval.Core.Util.EARandom;
using System;
using System.Collections.Generic;
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
            {
                g.Bits[i] = RNG.NextBool();
            }
            OneMaxPhenotype p = new OneMaxPhenotype(g);
            return p;
        }


        public static void Run()
        {
            var config = new EAConfiguration
            {
                PopulationSize = 10,
                OverproductionFactor = 2.0,
                MaximumGenerations = 1000,
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



            onemaxEA.NewGenerationEvent += (gen) => {
                Console.WriteLine(gen);
            };
            onemaxEA.NewBestFitnessEvent += (pheno) => {
                Console.WriteLine($"[{((BinaryGenotype)pheno.Genotype).ToBitString()}]");
            };

            var res = onemaxEA.Evolve();
            Console.WriteLine("Done!");
            Console.WriteLine($"Winner: {res.Winner}");
            Console.Read();
        }

        private static void Print()
        {

        }
    }
}
