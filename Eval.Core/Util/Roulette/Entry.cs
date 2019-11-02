using System;
using System.Collections.Generic;
using System.Text;

namespace Eval.Core.Util.Roulette
{
    [Serializable]
    public class Entry<T>
    {
        public T Value { get; private set; }
        public double Probability { get; private set; }

        public Entry(T value, double probability)
        {
            this.Value = value;
            this.Probability = probability;
        }

        public override string ToString()
        {
            return $"Entry{{Value={Value}, Probability={Probability}}}";
        }
    }
}
