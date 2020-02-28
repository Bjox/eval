#region LICENSE
/* 
 * Distributed under the MIT License.
 * Copyright (c) 2019-2020 Viktor Zoric, Bjørnar Walle Alvestad, Endre Larsen
 * 
 * Read full license terms in the accompanying LICENSE file or at https://opensource.org/licenses/MIT
 */
#endregion

using System;
using Eval.Core.Config;
using Eval.Core.Models;

namespace Eval.Core.Selection.Adult
{
    [Serializable]
    public class GenerationalReplacementAdultSelection : IAdultSelection
    {
        public void SelectAdults(Population offspring, Population population, int n, EAMode eamode)
        {
            population.Clear();
            for (int i = 0; i < n; i++)
            {
                population.Add(offspring[i]);
            }
        }
    }
}
