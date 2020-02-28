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

namespace Eval.Core.Models
{
    public enum TerminationReason
    {
        Aborted,
        DurationLimitReached,
        FitnessLimitReached,
        GenerationLimitReached
    }
}
