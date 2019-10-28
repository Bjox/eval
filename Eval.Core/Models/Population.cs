using Eval.Core.Config;
using Eval.Core.Util.EARandom;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Eval.Core.Models
{
    public class Population : IReadOnlyList<IPhenotype>
    {
        /// <summary>
        /// A flag that indiciates if the population is filled.
        /// </summary>
        public bool IsFilled { get; private set; }
        /// <summary>
        /// A sorted population will always have the best fitness (lowest or highest) at index 0.
        /// </summary>
        public bool IsSorted { get; private set; }

        /// <summary>
        /// Returns the Maxiumum allowed size of the population
        /// </summary>
        public int Size => _population.Length;

        /// <summary>
        /// Returns the amount of elements currently in the population
        /// </summary>
        public int Count => _index;

        private readonly IPhenotype[] _population;
        private int _index;

        public Population(int size)
        {
            _population = new IPhenotype[size];
            _index = 0;
        }

        public IPhenotype this[int key]
        {
            get => _population[key];
        }

        public void Fill(Func<IPhenotype> phenotypeFactory)
        {
            for (int i = _index; i < _population.Length; i++)
            {
                Add(phenotypeFactory());
            }
            IsFilled = true;
            IsSorted = false;
        }

        public void Add(IPhenotype phenotype)
        {
            ThrowIfNullOrFull(phenotype);
            _population[_index++] = phenotype;

            IsFilled = _index == _population.Length;
            IsSorted = false;
        }

        public void Clear()
        {
            Array.Clear(_population, 0, _population.Length);
            _index = 0;
            IsFilled = false;
            IsSorted = false;
        }

        public void Clear(int elitism, EAMode mode)
        {
            ThrowIfNotFilled();

            if (elitism <= 0)
            {
                Clear();
                return;
            }

            // O(n)
            if (elitism == 1)
            {
                IPhenotype elite = _population[0];

                if (!IsSorted)
                {
                    switch (mode)
                    {
                        case EAMode.MaximizeFitness:
                            elite = GetMaxFitness();
                            break;
                        case EAMode.MinimizeFitness:
                            elite = GetMinFitness();
                            break;
                        default:
                            throw new NotImplementedException(mode.ToString());
                    }
                }

                Clear();
                Add(elite);
                return;
            }

            // O(n log n)
            Sort(mode);
            Array.Clear(_population, elitism, _population.Length - elitism);
            _index = elitism;
            IsFilled = false;
            IsSorted = false;
        }

        public void Evaluate(bool reevaluate, Action<IPhenotype> phenotypeEvaluatedEvent)
        {
            ThrowIfNotFilled();
            foreach (var individual in _population.Where(i => i != null && (!i.IsEvaluated || reevaluate)))
            {
                individual.Evaluate();
                phenotypeEvaluatedEvent?.Invoke(individual);
            }
        }

        public void Sort(EAMode mode)
        {
            ThrowIfNotFilled();
            switch (mode)
            {
                case EAMode.MaximizeFitness:
                    Array.Sort(_population, (a, b) => b.Fitness.CompareTo(a.Fitness));
                    break;

                case EAMode.MinimizeFitness:
                    Array.Sort(_population, (a, b) => a.Fitness.CompareTo(b.Fitness));
                    break;

                default:
                    throw new NotImplementedException($"EA mode {mode} is not implemented");
            }
            IsSorted = true;
        }

        private void ThrowIfNotFilled()
        {
            if (!IsFilled)
            {
                throw new InvalidOperationException("The population is not filled");
            }
        }

        private void ThrowIfNullOrFull(IPhenotype toAdd)
        {
            if (toAdd == null)
                throw new ArgumentNullException("phenotype");
            if (Count >= _population.Length)
                throw new InvalidOperationException("Population is full");
        }

        public IEnumerator<IPhenotype> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return _population[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IPhenotype GetMaxFitness()
        {
            var best = _population[0];
            foreach (var p in _population)
            {
                if (best == null && p != null)
                {
                    best = p;
                    continue;
                }
                if (p != null && p.Fitness > best.Fitness)
                    best = p;
            }
            return best;
        }

        public IPhenotype GetMinFitness()
        {
            var best = _population[0];
            foreach (var p in _population)
            {
                if (best == null && p != null)
                {
                    best = p;
                    continue;
                }
                if (p != null && p.Fitness < best.Fitness)
                    best = p;
            }
            return best;
        }

        public PopulationStatistics CalculatePopulationStatistics()
        {
            ThrowIfNotFilled();

            var fitnessMax = double.MinValue;
            var fitnessMin = double.MaxValue;
            var fitnessSum = 0.0;
            var avg = 0.0;
            var m2 = 0.0;
            
            for (int i = 0; i < Count; i++)
            {
                var p = _population[i];
                fitnessSum += p.Fitness;
                fitnessMax = Math.Max(fitnessMax, p.Fitness);
                fitnessMin = Math.Min(fitnessMin, p.Fitness);

                var d = p.Fitness - avg;
                avg += d / (i + 1);
                var d2 = p.Fitness - avg;
                m2 += d * d2;
            }

            var variance = m2 / Count;
            var sampleVariance = m2 / (Count - 1);
            var std = Math.Sqrt(variance);

            return new PopulationStatistics
            {
                AverageFitness = avg,
                MaxFitness = fitnessMax,
                MinFitness = fitnessMin,
                StandardDeviationFitness = std,
                VarianceFitness = variance
            };
        }
        

        /// <summary>
        /// Creates a probability selector according to the provided mode.
        /// The returned selector does not automatically reflect changes to
        /// the population fitness landscape. If the population is mutated,
        /// a new selector must be created.
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public Func<IPhenotype, double> GetProbabilitySelector(EAMode mode)
        {
            if (mode == EAMode.MaximizeFitness)
            {
                return p => p.Fitness;
            }
            else if (mode == EAMode.MinimizeFitness)
            {
                var maxFitness = double.MinValue; // TODO: maybe we can keep track of this when pop is filled?
                var minFitness = double.MaxValue;

                foreach (var p in _population)
                {
                    maxFitness = Math.Max(p.Fitness, maxFitness);
                    minFitness = Math.Min(p.Fitness, minFitness);
                }

                var sum = maxFitness + minFitness;
                return p => sum - p.Fitness;
            }
            else
            {
                throw new NotImplementedException($"GetProbabilitySelector not implemented for mode {mode}");
            }
        }

        public IPhenotype DrawRandom(IRandomNumberGenerator random)
        {
            ThrowIfNotFilled();
            return this[random.Next(Size)];
        }
    }
}
