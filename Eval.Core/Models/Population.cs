using Eval.Core.Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Eval.Core.Models
{
    public class Population : IReadOnlyList<IPhenotype>
    {
        public bool IsFilled { get; private set; }
        private int _index;
        private readonly IPhenotype[] _population;

        public Population(int size)
        {
            _population = new IPhenotype[size];
            _index = 0;
        }

        /// <summary>
        /// Returns the Maxiumum allowed size of the population
        /// </summary>
        public int Size => _population.Length;

        /// <summary>
        /// Returns the amount of elements currently in the population
        /// </summary>
        public int Count => _index;


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
        }

        public void Add(IPhenotype phenotype)
        {
            ThrowIfAddOnFull();
            _population[_index++] = phenotype;

            IsFilled = _index == _population.Length;
        }

        public void Clear()
        {
            Array.Clear(_population, 0, _population.Length);
            _index = 0;
            IsFilled = false;
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
                IPhenotype elite = null;
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

                Clear();
                _population[0] = elite;
                _index = 1;
                IsFilled = false;
                return;
            }

            // O(n log n)
            Sort(mode);
            Array.Clear(_population, elitism, _population.Length - elitism);
            _index = elitism;
            IsFilled = false;
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
        }

        private void ThrowIfNotFilled()
        {
            if (!IsFilled)
            {
                throw new InvalidOperationException("The population is not filled");
            }
        }

        private void ThrowIfAddOnFull()
        {
            if (Count >= _population.Length)
                throw new InvalidOperationException("Population is full");
        }

        public IEnumerator<IPhenotype> GetEnumerator()
        {
            foreach (var individual in _population)
            {
                yield return individual;
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
    }
}
