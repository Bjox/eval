#region LICENSE
/* 
 * Distributed under the MIT License.
 * Copyright (c) 2019-2020 Viktor Zoric, Bjørnar Walle Alvestad, Endre Larsen
 * 
 * Read full license terms in the accompanying LICENSE file or at https://opensource.org/licenses/MIT
 */
#endregion

namespace Eval.Core.Util.Roulette
{
    public interface IRoulette<T>
    {
        T Spin();
        T SpinAndRemove();
    }
}
