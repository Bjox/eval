using Eval.Core.Config;
using Eval.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        protected IPhenotype GenerationalBest;
        protected IPhenotype Best;

        public EA(IEAConfiguration configuration)
        {
            EAConfiguration = configuration;

            FitnessAverages = new List<double>();
            FitnessBests = new List<double>();
            FitnessDeviations = new List<double>();
        }

        protected abstract IPhenotype CreateRandomPhenotype();

        /// <summary>
        /// Creates a new population filled with phenotypes from CreateRandomPhenotype method.
        /// Override this method to seed the EA with a specific population.
        /// </summary>
        /// <param name="populationSize"></param>
        /// <returns></returns>
        public virtual Population GenerateInitialPopulation(int populationSize)
        {
            var population = new Population(populationSize);
            population.Fill(CreateRandomPhenotype);
            return population;
        }

        public EAResult Evolve()
        {
            var population = GenerateInitialPopulation(EAConfiguration.PopulationSize);

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

                // TODO: calc stats (avg, std...) and raise events?
                CalculateStatistics(population);

                if (!RunCondition(generation))
                {
                    break;
                }
                NewGenerationEvent(generation);

                // TODO: extract elites

                // TODO: parent selection

                // TODO: reproduction with crossover and mutation, remember overproduction if configured

                // TODO: evaluate offspring

                // TODO: adult selection

                // TODO: reintroduce elites

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
    }

}
