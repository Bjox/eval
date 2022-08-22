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

namespace Eval.Core.Util.Roulette
{
    
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
