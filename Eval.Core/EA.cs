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

        public EA(IEAConfiguration configuration)
        {
            EAConfiguration = configuration;
        }

        public EAResult Evolve()
        {
            var population = new Population(EAConfiguration.PopulationSize);

            population.Fill(CreateRandomPhenotype);
            population.Evaluate(EAConfiguration.ReevaluateElites, PhenotypeEvaluatedEvent);

            var generation = 0;

            while (true)
            {
                population.Sort(EAConfiguration.Mode);

                // TODO: determine winner and raise new best event

                // TODO: calc stats (avg, std...)

                if (!RunCondition(generation))
                {
                    break;
                }
                NewGenerationEvent(generation);

                // TODO: extract elites

                // TODO: parent selection

                // TODO: reproduction with crossover and mutation

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

        protected virtual bool RunCondition(int generation)
        {
            return generation < EAConfiguration.MaximumGenerations;
        }

        protected abstract IPhenotype CreateRandomPhenotype();

    }

}
