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
using System.Linq;
using Eval.Core.Util.EARandom;

namespace Eval.Core.Util.Roulette
{
    [Serializable()]
    public class Roulette<T> : IRoulette<T>
    {
        private readonly IRandomNumberGenerator _rng;
        private readonly List<Entry<T>> _entries;
        private double _p_sum = 0.0;

        public Roulette(IRandomNumberGenerator rng, int capacity)
        {
            _entries = new List<Entry<T>>(capacity);
            _rng = rng;
        }

        public Roulette(IRandomNumberGenerator rng) : this(rng, 100)
        {
        }

        public Roulette(
            IRandomNumberGenerator random,
            IReadOnlyList<T> elements,
            Func<T, double> probabilitySelector)
            : this(random, elements.Count)
        {
            foreach (var element in elements)
            {
                Add(element, probabilitySelector(element));
            }
        }



        public void Add(T element, double p)
        {
            _p_sum += p;
            var e = new Entry<T>(element, p);
            _entries.Add(e);
        }

        public void AddAll(IReadOnlyList<T> elements, Func<T, double> probabilitySelector)
        {
            foreach (var element in elements)
            {
                Add(element, probabilitySelector(element));
            }
        }

        public int NumberOfEntries()
        {
            return _entries.Count;
        }

        public T Spin()
        {
            return SpinInternal(false);
        }

        public T SpinAndRemove()
        {
            return SpinInternal(true);
        }

        private T SpinInternal(bool remove)
        {
            if (_entries.Count == 0)
                throw new InvalidOperationException("No entries in RouletteWheel.");

            double r = _rng.NextDouble() * _p_sum;
            double cumulative = 0.0;

            var element = _entries[0];

            for (int i = 0; i < _entries.Count; i++)
            {
                element = _entries[i];
                cumulative += element.Probability;

                if (r <= cumulative)
                {
                    if (remove)
                    {
                        _entries.RemoveAt(i);
                        _p_sum -= element.Probability;
                    }
                    break;
                }
            }

            return element.Value;
        }

        public override string ToString()
        {
            return $"Roulette{{Entries={_entries.ToString()}, p_sum={_p_sum}}}";
        }

    }
}
