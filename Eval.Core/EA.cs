using Eval.Core.Config;
using Eval.Core.Models;
using Eval.Core.Selection.Adult;
using Eval.Core.Selection.Parent;
using Eval.Core.Util.EARandom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        protected readonly List<double> FitnessAverages;
        protected readonly List<double> FitnessBests;
        protected readonly List<double> FitnessDeviations;
        protected readonly List<IPhenotype> Elites;

        protected IPhenotype GenerationalBest;
        protected IPhenotype Best;
        protected bool Abort;
        protected IParentSelection ParentSelection;
        protected IAdultSelection AdultSelection;
        protected IRandomNumberGenerator RNG;

        public EA(IEAConfiguration configuration)
        {
            EAConfiguration = configuration;

            FitnessAverages = new List<double>();
            FitnessBests = new List<double>();
            FitnessDeviations = new List<double>();

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
        public virtual Population CreateInitialPopulation(int populationSize)
        {
            var population = new Population(populationSize);
            population.Fill(CreateRandomPhenotype);
            return population;
        }

        public EAResult Evolve()
        {
            var population = CreateInitialPopulation(EAConfiguration.PopulationSize);
            var offspringSize = (int)(EAConfiguration.PopulationSize * (EAConfiguration.OverproductionFactor < 1 ? 1 : EAConfiguration.OverproductionFactor));
            var offspring = new Population(offspringSize);

            population.Evaluate(EAConfiguration.ReevaluateElites, PhenotypeEvaluatedEvent);

            Best = null;
            var generation = 0;

            while (true)
            {
                population.Sort(EAConfiguration.Mode);
                var generationBest = population.First();
                if (IsBetterThan(generationBest, Best))
                {
                    Best = generationBest;
                    NewBestFitnessEvent(Best);
                }
                
                CalculateStatistics(population);

                if (!RunCondition(generation))
                {
                    break;
                }
                NewGenerationEvent(generation);

                // TODO: extract elites
                Elites.Clear();
                for (int i = 0; i < EAConfiguration.Elites; i++)
                    Elites.Add(population[i]);

                // TODO: parent selection
                var parents = ParentSelection.SelectParents(population, offspringSize - Elites.Count, EAConfiguration.Mode, RNG);

                // TODO: reproduction with crossover and mutation, remember overproduction if configured
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

                // TODO: evaluate offspring
                CalculateFitnesses(offspring);

                // TODO: adult selection
                AdultSelection.SelectAdults(offspring, population, EAConfiguration.PopulationSize - Elites.Count, EAConfiguration.Mode);

                // TODO: reintroduce elites
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
                AbortedEvent();
                return false;
            }
            return generation < EAConfiguration.MaximumGenerations;
        }

        protected virtual void CalculateStatistics(Population population)
        {
            if (!EAConfiguration.CalculateStatistics)
                return;

            var popstats = population.CalculatePopulationStatistics(EAConfiguration.Mode);

            FitnessAverages.Add(popstats.AverageFitness);

            switch (EAConfiguration.Mode)
            {
                case EAMode.MaximizeFitness:
                    FitnessBests.Add(popstats.MaxFitness);
                    break;
                case EAMode.MinimizeFitness:
                    FitnessBests.Add(popstats.MinFitness);
                    break;
                default:
                    throw new NotImplementedException($"CalculateStatistics not implemented for EA mode {EAConfiguration.Mode}");
            }

            FitnessDeviations.Add(popstats.StandardDeviationFitness);
        }

        protected virtual void CalculateFitnesses(Population population)
        {
            if (EAConfiguration.WorkerThreads <= 0)
            {
                foreach (var p in population)
                {
                    p.Evaluate();
                }
            }
            else
            {
                foreach (var p in population)
                {
                    ThreadPool.QueueUserWorkItem(FitnessWorker, p);
                }
            }
        }

        private void FitnessWorker(object state)
        {
            var pheno = state as IPhenotype;
            pheno.Evaluate();
        }
    }

}
