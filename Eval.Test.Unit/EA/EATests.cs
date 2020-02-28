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
using System.IO;

namespace Eval.Test.Unit.EATests
{
    [TestClass]
    public class EATests
    {
        private TestEA _ea;
        private EAConfiguration _config = new EAConfiguration
        {
            PopulationSize = 10,
            MaximumGenerations = 2,
            AdultSelectionType = AdultSelectionType.GenerationalReplacement,
            ParentSelectionType = ParentSelectionType.FitnessProportionate,
            TargetFitness = 1
        };

        [TestInitialize]
        public void Init()
        {
            _ea = new TestEA(_config, new DefaultRandomNumberGenerator());
        }

        [TestMethod]
        public void EvolveShouldCallCreateMethodsCorrectNumberOfTimes()
        {
            _ea.Evolve();
            _ea.CreateRandomPhenotypeCount.Should().Be(10);
            _ea.CreatePhenotypeCount.Should().Be(10);
        }

        [TestMethod]
        public void SeededEvolveShouldCallCreateMethodsCorrectNumberOfTimes()
        {
            var _ea = new TestEASeeded(_config, new DefaultRandomNumberGenerator());
            _ea.Evolve();
            _ea.CreateRandomPhenotypeCount.Should().Be(10);
            _ea.CreatePhenotypeCount.Should().Be(10);
            _ea.CreateInitialPopulationCount.Should().Be(1);
            _ea.CreateParentSelectionCount.Should().Be(1);
            _ea.CreateAdultSelectionCount.Should().Be(1);
        }

        [TestMethod]
        public void TestEAEvents()
        {
            var newGenerationEventCounter = 0;
            _ea.NewGenerationEvent += (gen) => newGenerationEventCounter++;

            var phenotypeEvaluatedEventCounter = 0;
            _ea.PhenotypeEvaluatedEvent += (p) => phenotypeEvaluatedEventCounter++;

            var newBestFitnessEventCounter = 0;
            _ea.NewBestFitnessEvent += (p, g) => newBestFitnessEventCounter++;

            var generationLimitReachedEventCounter = 0;
            _ea.TerminationEvent += (r) =>
            {
                if (r == TerminationReason.GenerationLimitReached)
                    generationLimitReachedEventCounter++;
            };

            _ea.Evolve();

            newGenerationEventCounter.Should().Be(2);
            phenotypeEvaluatedEventCounter.Should().Be(20);
            newBestFitnessEventCounter.Should().BeGreaterOrEqualTo(1);
            generationLimitReachedEventCounter.Should().Be(1);
        }

        [TestMethod]
        public void TestCalculateStatisticsConfig()
        {
            var statscounter1 = 0;
            _config.CalculateStatistics = true;
            var _ea = new TestEASeeded(_config, new DefaultRandomNumberGenerator());
            _ea.PopulationStatisticsCalculated += (s) => statscounter1++;
            _ea.Evolve();
            _ea.CalculateStatisticsCount.Should().Be(2);
            statscounter1.Should().Be(2);

            var statscounter2 = 0;
            _config.CalculateStatistics = false;
            _ea = new TestEASeeded(_config, new DefaultRandomNumberGenerator());
            _ea.PopulationStatisticsCalculated += (s) => statscounter2++;
            _ea.Evolve();
            _ea.CalculateStatisticsCount.Should().Be(2);
            statscounter2.Should().Be(0);
        }

        [TestMethod]
        public void TestSerialization_FileShouldExist()
        {
            var filename = "testsnap.bin";

            if (File.Exists(filename))
                File.Delete(filename);

            _config.SnapshotFilename = filename;
            _config.SnapshotGenerationInterval = 1;
            _config.MaximumGenerations = 2;

            _ea = new TestEA(_config, new DefaultRandomNumberGenerator());
            _ea.Evolve();
            File.Exists(filename).Should().BeTrue();
            File.Delete(filename);
        }

        [TestMethod]
        public void TestSerialization_FileShouldNotExist_WhenDeactivated()
        {
            var filename = "testsnap.bin";

            if (File.Exists(filename))
                File.Delete(filename);

            _config.SnapshotFilename = filename;
            _config.SnapshotGenerationInterval = 0;
            _config.MaximumGenerations = 2;

            _ea = new TestEA(_config, new DefaultRandomNumberGenerator());
            _ea.Evolve();
            File.Exists(filename).Should().BeFalse();
        }

        [TestMethod]
        public void TestSerialization_ShouldContinueWhereLeftOff()
        {
            var filename = "testsnap.bin";

            if (File.Exists(filename))
                File.Delete(filename);

            _config.SnapshotFilename = filename;
            _config.SnapshotGenerationInterval = 1;
            _config.MaximumGenerations = 5;

            _ea = new TestEA(_config, new DefaultRandomNumberGenerator());
            _ea.Evolve();
            File.Exists(filename).Should().BeTrue();

            _config.MaximumGenerations = 10;
            _ea = new TestEA(_config, new DefaultRandomNumberGenerator());
            _ea.NewGenerationEvent += (g) =>
            {
                g.Should().BeGreaterOrEqualTo(5);
            };
            _ea.Evolve();

            File.Delete(filename);
        }

        [TestMethod]
        public void TestDurationTermination_ShouldTerminateWhenMaxDurationHasPassed()
        {
            var ok = false;
            _config.MaximumGenerations = 500000;
            _config.MaxDuration = new TimeSpan(0, 0, 1);
            _ea = new TestEA(_config, new DefaultRandomNumberGenerator());
            _ea.TerminationEvent += (r) =>
            {
                if (r == TerminationReason.DurationLimitReached)
                    ok = true;
            };
            _ea.Evolve();

            Assert.IsTrue(ok);
            Assert.IsTrue(_ea.GetDuration < new TimeSpan(0, 0, 2));
        }
    }

    
    [Serializable]
    class TestPhenotype : Phenotype
    {
        
        public TestPhenotype(IGenotype genotype) : base(genotype) {}

        protected override double CalculateFitness()
        {
            var g = Genotype as BinaryGenotype;
            var c = 0;
            for (int i = 0; i < g.Count; i++)
            {
                c += g.Bits[i] ? 1 : 0;
            }
            return c / (double)g.Count;
        }
    }

    [Serializable]
    class TestEA : EA
    {
        public int bits = 10;
        public int CreatePhenotypeCount = 0;
        public int CreateRandomPhenotypeCount = 0;

        public TestEA(IEAConfiguration configuration, IRandomNumberGenerator rng) : base(configuration, rng)
        {
        }

        protected override IPhenotype CreatePhenotype(IGenotype genotype)
        {
            CreatePhenotypeCount++;
            return new TestPhenotype(genotype);
        }

        protected override IPhenotype CreateRandomPhenotype()
        {
            CreateRandomPhenotypeCount++;
            var geno = new BinaryGenotype(bits);
            return new TestPhenotype(geno);
        }
    }

    class TestEASeeded : EA
    {
        public int bits = 10;
        public int CreatePhenotypeCount = 0;
        public int CreateRandomPhenotypeCount = 0;
        public int CreateInitialPopulationCount = 0;
        public int CreateAdultSelectionCount = 0;
        public int CreateParentSelectionCount = 0;
        public int CalculateStatisticsCount = 0;

        public TestEASeeded(IEAConfiguration configuration, IRandomNumberGenerator rng) : base(configuration, rng)
        {
        }

        protected override IPhenotype CreatePhenotype(IGenotype genotype)
        {
            CreatePhenotypeCount++;
            return new TestPhenotype(genotype);
        }

        protected override IPhenotype CreateRandomPhenotype()
        {
            CreateRandomPhenotypeCount++;
            var geno = new BinaryGenotype(bits);
            return new TestPhenotype(geno);
        }

        protected override Population CreateInitialPopulation(int populationSize)
        {
            CreateInitialPopulationCount++;
            var pop = new Population(populationSize);
            for (int i = 0; i < pop.Size; i++)
            {
                pop.Add(CreateRandomPhenotype());
            }
            return pop;
        }

        protected override IAdultSelection CreateAdultSelection()
        {
            CreateAdultSelectionCount++;
            return base.CreateAdultSelection();
        }

        protected override IParentSelection CreateParentSelection()
        {
            CreateParentSelectionCount++;
            return base.CreateParentSelection();
        }

        protected override void CalculateStatistics(Population population)
        {
            CalculateStatisticsCount++;
            base.CalculateStatistics(population);
        }
    }
}
