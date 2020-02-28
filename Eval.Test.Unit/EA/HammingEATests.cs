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
using System.Collections.Generic;
using System.Text;

namespace Eval.Test.Unit.EATests
{
    [TestClass]
    public class HammingEATests
    {

        [TestMethod]
        public void TestHammingEA()
        {
            var config = new EAConfiguration
            {
                PopulationSize = 100,
                OverproductionFactor = 2.0,
                MaximumGenerations = 5000,
                CrossoverType = CrossoverType.OnePoint,
                AdultSelectionType = AdultSelectionType.GenerationalMixing,
                ParentSelectionType = ParentSelectionType.Tournament,
                CrossoverRate = 0.9,
                MutationRate = 0.25,
                TournamentSize = 10,
                TournamentProbability = 0.8,
                TargetFitness = 0.0,
                Mode = EAMode.MinimizeFitness,
                Elites = 1
            };

            var fitnesslimitCounter = 0;

            for (int i = 0; i < 10; i++)
            {
                var hammingEA = new HammingEA(config, new DefaultRandomNumberGenerator());
                hammingEA.TerminationEvent += (r) =>
                {
                    if (r == TerminationReason.FitnessLimitReached)
                        fitnesslimitCounter++;
                };
                var res = hammingEA.Evolve();
            }

            fitnesslimitCounter.Should().BeGreaterOrEqualTo(9);
            Console.WriteLine($"Fitness limit reached {fitnesslimitCounter} times");
        }

    }

    /// <summary>
    /// Optimizes towards a given string. 
    /// This is a minimization problem using hamming distance for fitness function
    /// </summary>
    class HammingEA : EA
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
    }

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

        public override void Mutate(double probability, IRandomNumberGenerator random)
        {
            if (random.NextDouble() >= probability)
                return;

            StringBuilder sb = new StringBuilder(str);
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
    }
}
