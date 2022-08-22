#region LICENSE
/* 
 * Distributed under the MIT License.
 * Copyright (c) 2019-2020 Viktor Zoric, Bjørnar Walle Alvestad, Endre Larsen
 * 
 * Read full license terms in the accompanying LICENSE file or at https://opensource.org/licenses/MIT
 */
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using Eval.Core.Config;
using Eval.Core.Models;
using Eval.Core.Util.EARandom;

namespace Eval.Core.Selection.Parent
{
    
    public class TournamentParentSelection : IParentSelection
    {
        private readonly int _tournamentSize;
        private readonly double _prob;

        public TournamentParentSelection(IEAConfiguration configuration)
        {
            if (configuration.TournamentSize < 2)
            {
                throw new ArgumentException($"Tournament size must be >= 2, but is {configuration.TournamentSize}", "TournamentSize");
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
                yield return (SelectOne(population, random), SelectOne(population, random));
            }
        }

        private IPhenotype SelectOne(Population population, IRandomNumberGenerator random)
        {
            if (_tournamentSize == 2)
            {
                var selection1 = random.Next(population.Size);
                if (random.NextDouble() < _prob)
                {
                    var selection2 = random.Next(population.Size);
                    return selection1 < selection2 ? population[selection1] : population[selection2];
                }
                else
                {
                    return population[selection1];
                }
            }

            if (random.NextDouble() < _prob)
            {
                // Simulate selecting the best from a tournament
                var normalizedSelection = 1.0 - Math.Pow(random.NextDouble(), 1.0 / _tournamentSize);
                var selectedIndex = (int)(normalizedSelection * population.Size);
                return population[Math.Min(selectedIndex, population.Size - 1)];
            } 
            else
            {
                // Select randomly from the tournament (which is equal to just drawing a random from the whole population)
                return population.DrawRandom(random);
            }
        }
    }
}
