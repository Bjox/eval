using Eval.Core.Config;
using Eval.Core.Models;
using Eval.Core.Selection.Adult;
using Eval.Core.Selection.Parent;
using Eval.Core.Util.EARandom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Eval.Core
{
    public abstract class EA
    {
        public IEAConfiguration EAConfiguration { get; set; }

        public event Action<int> NewGenerationEvent;
        public event Action<IPhenotype> NewBestFitnessEvent;
        public event Action<double> FitnessLimitReachedEvent;
        public event Action<int> GenerationLimitReachedEvent;
        public event Action<IPhenotype> PhenotypeEvaluatedEvent;
        public event Action AbortedEvent;
        public event Action<PopulationStatistics> PopulationStatisticsCalculated;

        protected readonly List<PopulationStatistics> PopulationStatistics;
        protected readonly List<IPhenotype> Elites;

        protected IPhenotype GenerationalBest;
        protected IPhenotype Best;
        protected bool Abort;
        protected IParentSelection ParentSelection;
        protected IAdultSelection AdultSelection;
        protected IRandomNumberGenerator RNG;

        public EA(IEAConfiguration configuration, IRandomNumberGenerator rng)
        {
            EAConfiguration = configuration;
            RNG = rng;

            PopulationStatistics = new List<PopulationStatistics>(512);
            Elites = new List<IPhenotype>(Math.Max(EAConfiguration.Elites, 0));
            AdultSelection = CreateAdultSelection();
            ParentSelection = CreateParentSelection();

            if (EAConfiguration.WorkerThreads > 1)
            {
                ThreadPool.SetMinThreads(EAConfiguration.WorkerThreads, EAConfiguration.IOThreads);
                ThreadPool.SetMaxThreads(EAConfiguration.WorkerThreads, EAConfiguration.IOThreads);
            }
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

        public EAResult Evolve()
        {
            var population = CreateInitialPopulation(EAConfiguration.PopulationSize);
            var offspringSize = (int)(EAConfiguration.PopulationSize * Math.Max(EAConfiguration.OverproductionFactor, 1));
            var offspring = new Population(offspringSize);

            population.Evaluate(EAConfiguration.ReevaluateElites, PhenotypeEvaluatedEvent);
            var generation = 1;

            Best = null;

            while (true)
            {
                population.Sort(EAConfiguration.Mode);
                var generationBest = population.First();

                if (IsBetterThan(generationBest, Best))
                {
                    Best = generationBest;
                    NewBestFitnessEvent?.Invoke(Best);
                }

                NewGenerationEvent?.Invoke(generation);
                
                CalculateStatistics(population);

                if (!RunCondition(generation))
                {
                    break;
                }
                
                Elites.Clear();
                for (int i = 0; i < EAConfiguration.Elites; i++)
                    Elites.Add(population[i]);
                
                var parents = ParentSelection.SelectParents(population, offspringSize - Elites.Count, EAConfiguration.Mode, RNG);
                
                offspring.Clear();
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
                    offspring.Add(child);
                }
                
                CalculateFitnesses(offspring);
                
                AdultSelection.SelectAdults(offspring, population, EAConfiguration.PopulationSize - Elites.Count, EAConfiguration.Mode);
                
                for (int i = 0; i < EAConfiguration.Elites; i++)
                    population.Add(Elites[i]);

                generation++;
            }

            return new EAResult
            {
                Winner = population[0],
                EndPopulation = population
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
            if (EAConfiguration.WorkerThreads <= 1)
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
    }

}
