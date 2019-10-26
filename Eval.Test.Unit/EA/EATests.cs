using Eval.Core;
using Eval.Core.Config;
using Eval.Core.Models;
using Eval.Core.Selection.Adult;
using Eval.Core.Selection.Parent;
using Eval.Core.Util.EARandom;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            AdultSelectionType = Core.Selection.Adult.AdultSelectionType.GenerationalReplacement,
            ParentSelectionType = Core.Selection.Parent.ParentSelectionType.FitnessProportionate,
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
            _ea.NewBestFitnessEvent += (p) => newBestFitnessEventCounter++;

            var generationLimitReachedEventCounter = 0;
            _ea.GenerationLimitReachedEvent += (g) => generationLimitReachedEventCounter++;

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

        
    }

    

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
