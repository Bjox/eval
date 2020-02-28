#region LICENSE
/* 
 * Distributed under the MIT License.
 * Copyright (c) 2019-2020 Viktor Zoric, Bjørnar Walle Alvestad, Endre Larsen
 * 
 * Read full license terms in the accompanying LICENSE file or at https://opensource.org/licenses/MIT
 */
#endregion

using Eval.Examples.Hamming;
using Eval.Examples.MultithreadTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eval.Examples
{
    class Program
    {
        static void Main(string[] args)
        {
            // Examples:

            //OneMaxEA.Run();

            //LOLZEA.Run();

            //HammingEA.Run();
            HammingOptimizer.Run();

            //ThreadEA.Run();
        }
    }
}
