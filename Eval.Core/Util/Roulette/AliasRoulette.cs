using Eval.Core.Util.EARandom;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Eval.Core.Util.Roulette
{
    public class AliasRoulette<T> : IRoulette<T>
    {
        private readonly int n;
        private readonly int[] alias;
        private readonly double[] prob;
        private readonly IRandomNumberGenerator random;
        private readonly IReadOnlyList<T> elements;

        public AliasRoulette(
            IRandomNumberGenerator random,
            IReadOnlyList<T> elements,
            Func<T, double> probabilitySelector)
        {
            this.elements = elements;
            this.random = random;
            n = elements.Count;
            if (n == 0)
            {
                throw new ArgumentException("No entries to add in AliasRoulette");
            }

            alias = new int[n];
            prob = new double[n];

            Initialize(probabilitySelector);
        }

        private void Initialize(Func<T, double> probabilitySelector)
        {
            // Alias algo: http://www.keithschwarz.com/darts-dice-coins/

            // Fetch and normalize probabilities
            var p = new double[n];
            var p_sum = 0.0;
            for (int i = 0; i < n; i++)
            {
                var p_e = probabilitySelector(elements[i]);
                p_sum += p_e;
                p[i] = p_e;
            }

            // Alias algo init
            var small = new Queue<int>();
            var large = new Queue<int>();

            var factor = n / p_sum; // multiply by n because of Alias algo, divide by p_sum to normalize probabilities.
            for (int i = 0; i < n; i++)
            {
                p[i] *= factor;
                if (p[i] < 1.0) small.Enqueue(i);
                else large.Enqueue(i);
            }

            while (small.Count > 0 && large.Count > 0)
            {
                var l = small.Dequeue();
                var g = large.Dequeue();
                prob[l] = p[l];
                alias[l] = g;
                p[g] = (p[g] + p[l]) - 1.0;
                if (p[g] < 1.0) small.Enqueue(g);
                else large.Enqueue(g);
            }

            while (large.Count > 0)
            {
                var g = large.Dequeue();
                prob[g] = 1.0;
            }

            while (small.Count > 0)
            {
                var l = small.Dequeue();
                prob[l] = 1.0;
            }
        }

        public T Spin()
        {
            var i = random.Next(n);
            var sampleIndex = random.NextDouble() < prob[i] ? i : alias[i];
            return elements[sampleIndex];
        }

        public T SpinAndRemove()
        {
            throw new NotImplementedException();
        }
    }
}
