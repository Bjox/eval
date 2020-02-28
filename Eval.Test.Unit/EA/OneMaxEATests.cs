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
using Eval.Core.Util.EARandom;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Eval.Test.Unit.EATests
{
    [TestClass]
    public class OneMaxEATests
    {

        [TestMethod]
        public void TestOneMaxEA()
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

            var fitnesslimitCounter = 0;

            for (int i = 0; i < 100; i++)
            {
                var onemaxEA = new OneMaxEA(config, new DefaultRandomNumberGenerator());
                onemaxEA.TerminationEvent += (r) =>
                {
                    if (r == TerminationReason.FitnessLimitReached)
                        fitnesslimitCounter++;
                };
                var res = onemaxEA.Evolve();
            }

            fitnesslimitCounter.Should().BeGreaterOrEqualTo(99);
            Console.WriteLine($"Fitness limit reached {fitnesslimitCounter} times");
        }

    }

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

    class OneMaxEA : EA
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
    }
}
