using Eval.Core.Config;
using Eval.Core.Models;
using Eval.Core.Selection.Adult;
using Eval.Core.Selection.Parent;
using Eval.Core.Util.EARandom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

namespace Eval.Core
{

    [Serializable]
    public abstract class EA
    {
        [field: NonSerialized]
        public event Action<int> NewGenerationEvent;
        [field: NonSerialized]
        public event Action<IPhenotype, int> NewBestFitnessEvent;
        [field: NonSerialized]
        public event Action<double> FitnessLimitReachedEvent;
        [field: NonSerialized]
        public event Action<int> GenerationLimitReachedEvent;
        [field: NonSerialized]
        public event Action<IPhenotype> PhenotypeEvaluatedEvent;
        [field: NonSerialized]
        public event Action AbortedEvent;
        [field: NonSerialized]
        public event Action<PopulationStatistics> PopulationStatisticsCalculated;

        public IEAConfiguration EAConfiguration { get; set; }
        public bool IsRunning { get; private set; }
        public bool IsStarted { get; private set; }
        public int Generation { get; private set; }
        public TimeSpan GetDuration => _stopwatch.Elapsed;

        protected List<PopulationStatistics> PopulationStatistics { get; private set; }
        protected List<IPhenotype> Elites;
        protected IPhenotype GenerationalBest;
        protected IPhenotype Best;
        protected bool Abort;
        protected IParentSelection ParentSelection;
        protected IAdultSelection AdultSelection;
        protected IRandomNumberGenerator RNG;
        protected Population Population { get; private set; }
        private int _offspringSize;
        private Population _offspring;

        [NonSerialized]
        private Stopwatch _stopwatch;
        private TimeSpan _runtime;
        public TimeSpan Runtime 
        {
            get 
            {
                return _runtime + _stopwatch.Elapsed;
            }
            private set 
            {
                _runtime = value; 
            } 
        }

        public EA(IEAConfiguration configuration, IRandomNumberGenerator rng)
        {
            EAConfiguration = configuration;
            RNG = rng;

            PopulationStatistics = new List<PopulationStatistics>(512);
            Elites = new List<IPhenotype>(Math.Max(EAConfiguration.Elites, 0));
            AdultSelection = CreateAdultSelection();
            ParentSelection = CreateParentSelection();
        }

        protected abstract IPhenotype CreateRandomPhenotype();
        protected abstract IPhenotype CreatePhenotype(IGenotype genotype);

        /// <summary>
        /// Creates a new population filled with phenotypes from CreateRandomPhenotype method.
        /// Override this method to seed the EA with a specific population.
        /// </summary>
        /// <param name="populationSize"></param>
        /// <returns></returns>
        protected virtual Population CreateInitialPopulation(int populationSize)
        {
            var population = new Population(populationSize);
            population.Fill(CreateRandomPhenotype);
            return population;
        }

        protected virtual IAdultSelection CreateAdultSelection()
        {
            switch (EAConfiguration.AdultSelectionType)
            {
                case AdultSelectionType.GenerationalMixing:
                    return new GenerationalMixingAdultSelection(RNG);
                case AdultSelectionType.GenerationalReplacement:
                    return new GenerationalReplacementAdultSelection();
                case AdultSelectionType.Overproduction:
                    return new OverproductionAdultSelection(RNG);
                default:
                    throw new NotImplementedException($"AdultSelectionType {EAConfiguration.AdultSelectionType}");
            }
        }

        protected virtual IParentSelection CreateParentSelection()
        {
            switch (EAConfiguration.ParentSelectionType)
            {
                case ParentSelectionType.FitnessProportionate:
                    return new ProportionateParentSelection();
                case ParentSelectionType.Rank:
                    return new RankParentSelection();
                case ParentSelectionType.SigmaScaling:
                    return new SigmaScalingParentSelection();
                case ParentSelectionType.Tournament:
                    return new TournamentParentSelection(EAConfiguration);
                default:
                    throw new NotImplementedException($"ParentSelectionType {EAConfiguration.ParentSelectionType}");
            }
        }

        public virtual EAResult Evolve()
        {
            _stopwatch = Stopwatch.StartNew();
            IsRunning = true;
            IsStarted = true;

            if (EAConfiguration.SnapshotGenerationInterval > 0 
                && !string.IsNullOrEmpty(EAConfiguration.SnapshotFilename)
                && File.Exists(EAConfiguration.SnapshotFilename))
            {
                BinaryDeserialize(EAConfiguration.SnapshotFilename);
            }
            else
            {
                Population = CreateInitialPopulation(EAConfiguration.PopulationSize);
                _offspringSize = (int)(EAConfiguration.PopulationSize * Math.Max(EAConfiguration.OverproductionFactor, 1));
                _offspring = new Population(_offspringSize);

                CalculateFitnesses(Population);
                Generation = 1;

                Best = null;
            }

            while (true)
            {
                Serialize();
                Population.Sort(EAConfiguration.Mode);
                var generationBest = Population.First();

                if (IsBetterThan(generationBest, Best))
                {
                    Best = generationBest;
                    NewBestFitnessEvent?.Invoke(Best, Generation);
                }
                
                CalculateStatistics(Population);

                NewGenerationEvent?.Invoke(Generation);

                if (!RunCondition(Generation))
                {
                    break;
                }
                
                Elites.Clear();
                for (int i = 0; i < EAConfiguration.Elites; i++)
                    Elites.Add(Population[i]);
                
                var parents = ParentSelection.SelectParents(Population, _offspringSize - Elites.Count, EAConfiguration.Mode, RNG);
                
                _offspring.Clear();
                foreach (var couple in parents)
                {
                    IGenotype geno1 = couple.Item1.Genotype;
                    IGenotype geno2 = couple.Item2.Genotype;

                    IGenotype newgeno;
                    if (RNG.NextDouble() < EAConfiguration.CrossoverRate)
                        newgeno = geno1.CrossoverWith(geno2, EAConfiguration.CrossoverType, RNG);
                    else
                        newgeno = geno1.Clone();

                    newgeno.Mutate(EAConfiguration.MutationRate, RNG);
                    var child = CreatePhenotype(newgeno);
                    _offspring.Add(child);
                }
                
                CalculateFitnesses(_offspring);
                
                AdultSelection.SelectAdults(_offspring, Population, EAConfiguration.PopulationSize - Elites.Count, EAConfiguration.Mode);
                
                for (int i = 0; i < EAConfiguration.Elites; i++)
                    Population.Add(Elites[i]);

                Generation++;
            }

            IsRunning = false;
            _stopwatch.Stop();
            return new EAResult
            {
                Winner = Population[0],
                EndPopulation = Population
            };
        }

        private bool IsBetterThan(IPhenotype subject, IPhenotype comparedTo)
        {
            if (comparedTo == null)
            {
                return true;
            }
            switch (EAConfiguration.Mode)
            {
                case EAMode.MaximizeFitness:
                    return subject.Fitness > comparedTo.Fitness;
                case EAMode.MinimizeFitness:
                    return subject.Fitness < comparedTo.Fitness;
                default:
                    throw new NotImplementedException($"IsBetterThan not implemented for EA mode {EAConfiguration.Mode}");
            }
        }

        protected virtual bool RunCondition(int generation)
        {
            if (Abort)
            {
                AbortedEvent?.Invoke();
                return false;
            }
            if (generation >= EAConfiguration.MaximumGenerations)
            {
                GenerationLimitReachedEvent?.Invoke(generation);
                return false;
            }
            if ((EAConfiguration.Mode == EAMode.MaximizeFitness && Best.Fitness >= EAConfiguration.TargetFitness) ||
                (EAConfiguration.Mode == EAMode.MinimizeFitness && Best.Fitness <= EAConfiguration.TargetFitness))
            {
                FitnessLimitReachedEvent?.Invoke(Best.Fitness);
                return false;
            }
            return true;
        }

        

        protected virtual void CalculateStatistics(Population population)
        {
            if (!EAConfiguration.CalculateStatistics)
                return;

            var popstats = population.CalculatePopulationStatistics();
            PopulationStatisticsCalculated?.Invoke(popstats);
            PopulationStatistics.Add(popstats);
        }

        protected virtual void CalculateFitnesses(Population population)
        {
            if (!EAConfiguration.MultiThreaded)
            {
                foreach (var p in population)
                {
                    p.Evaluate();
                    PhenotypeEvaluatedEvent?.Invoke(p);
                }
            }
            else
            {
                using (var countdownEvent = new CountdownEvent(population.Count))
                {
                    foreach (var p in population)
                    {
                        ThreadPool.QueueUserWorkItem(FitnessWorker, new object[] { p, countdownEvent });
                    }
                    countdownEvent.Wait();
                }
            }
        }

        private void FitnessWorker(object state)
        {
            var input = state as object[];
            var pheno = input[0] as IPhenotype;
            var countdownEvent = input[1] as CountdownEvent;

            pheno.Evaluate();
            PhenotypeEvaluatedEvent?.Invoke(pheno);

            countdownEvent.Signal();
        }

        private void Serialize()
        {
            if (EAConfiguration.SnapshotGenerationInterval <= 0
                || Generation % EAConfiguration.SnapshotGenerationInterval != 0
                && Generation != 1) // run on first generation as a test in case some class is missing [Serializable]
                return;

            BinarySerialize(EAConfiguration.SnapshotFilename);
        }

        /// <summary>
        /// Throws exception on serialization failure
        /// </summary>
        /// <param name="filename"></param>
        public virtual void BinarySerialize(string filename)
        {
            Runtime += _stopwatch.Elapsed;
            var stream = File.OpenWrite(filename);
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, this);
            stream.Close();
        }

        /// <summary>
        /// Throws exception on deserialization failure
        /// </summary>
        /// <param name="filename"></param>
        public virtual void BinaryDeserialize(string filename)
        {
            var stream = File.OpenRead(filename);
            var formatter = new BinaryFormatter();
            var ea = (EA)formatter.Deserialize(stream);
            stream.Close();

            AssertConfigurationCompatibility(ea.EAConfiguration);

            this.Population = ea.Population;
            this.PopulationStatistics = ea.PopulationStatistics;
            this.ParentSelection = ea.ParentSelection;
            this.AdultSelection = ea.AdultSelection;
            this.Best = ea.Best;
            this.Elites = ea.Elites;
            this.Generation = ea.Generation;
            this.GenerationalBest = ea.GenerationalBest;
            this.RNG = ea.RNG;
            this._offspring = ea._offspring;
            this._offspringSize = ea._offspringSize;
            this._runtime = ea._runtime;
        }

        public virtual void AssertConfigurationCompatibility(IEAConfiguration other)
        {
            if (EAConfiguration.PopulationSize != other.PopulationSize)
                throw new ArgumentException("Population size cannot differ between snapshots. In snapshot: "+other.PopulationSize);
        }
    }

}
