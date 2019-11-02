using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eval.Core.Config;
using Eval.Core.Models;
using Eval.Core.Util.EARandom;

namespace Eval.Core.Selection.Parent
{
    [Obsolete("Use new tournament selection instead")]
    [Serializable]
    public class TournamentParentSelectionOld : IParentSelection
    {
        private readonly int _tournamentSize;
        private readonly double _prob;

        public TournamentParentSelectionOld(IEAConfiguration configuration)
        {
            if (configuration.TournamentSize < 2)
            {
                throw new ArgumentException("TournamentSize");
            }
            _tournamentSize = configuration.TournamentSize;
            _prob = configuration.TournamentProbability;
        }

        public IEnumerable<(IPhenotype, IPhenotype)> SelectParents(Population population, int n, EAMode mode, IRandomNumberGenerator random)
        {
            if (!population.IsSorted)
            {
                throw new ArgumentException("Population is not sorted");
            }
            for (int i = 0; i < n; i++)
            {
                yield return (SelectOne(population, mode, random), SelectOne(population, mode, random));
            }
        }

        private IPhenotype SelectOne(Population population, EAMode mode, IRandomNumberGenerator random)
        {
            List<IPhenotype> pool = new List<IPhenotype>(_tournamentSize);
            for (int i = 0; i < _tournamentSize; i++)
            {
                pool.Add(population.DrawRandom(random));
            }

            if (random.NextDouble() < _prob)
            {
                switch (mode)
                {
                    case EAMode.MaximizeFitness:
                        return pool.Max();
                    case EAMode.MinimizeFitness:
                        return pool.Min();
                    default:
                        throw new NotImplementedException(mode.ToString());
                }
            }
            else
            {
                return pool[random.Next(pool.Count)];
            }
        }
    }
}
