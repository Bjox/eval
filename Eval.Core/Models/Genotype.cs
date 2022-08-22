#region LICENSE
/* 
 * Distributed under the MIT License.
 * Copyright (c) 2019-2020 Viktor Zoric, Bjørnar Walle Alvestad, Endre Larsen
 * 
 * Read full license terms in the accompanying LICENSE file or at https://opensource.org/licenses/MIT
 */
#endregion

using System;
using Eval.Core.Util.EARandom;

namespace Eval.Core.Models
{
    
    public abstract class Genotype : IGenotype
    {
        public abstract IGenotype Clone();

        public abstract IGenotype CrossoverWith(IGenotype other, CrossoverType crossover, IRandomNumberGenerator random);

        public abstract void Mutate(double probability, IRandomNumberGenerator random);
    }
}
