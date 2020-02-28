#region LICENSE
/* 
 * Distributed under the MIT License.
 * Copyright (c) 2019-2020 Viktor Zoric, Bjørnar Walle Alvestad, Endre Larsen
 * 
 * Read full license terms in the accompanying LICENSE file or at https://opensource.org/licenses/MIT
 */
#endregion

using System;

namespace Eval.Core.Models
{
    public interface IPhenotype : IComparable<IPhenotype>
    {
        bool IsEvaluated { get; }
        IGenotype Genotype { get; }
        double Fitness { get; }
        double Evaluate();
    }
}
