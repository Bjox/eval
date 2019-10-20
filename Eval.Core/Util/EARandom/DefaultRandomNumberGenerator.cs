using System;
using System.Collections.Generic;
using System.Text;

namespace Eval.Core.Util.EARandom
{
    public class DefaultRandomNumberGenerator : Random, IRandomNumberGenerator
    {
        /// <summary>
        /// Initializes a new instance of the class, using a time-dependent default seed value.
        /// </summary>
        public DefaultRandomNumberGenerator()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the class, using the specified seed value.
        /// </summary>
        /// <param name="seed"></param>
        public DefaultRandomNumberGenerator(int seed)
            : base(seed)
        {
        }

        public bool NextBool()
        {
            return NextDouble() < 0.5;
        }
    }
}
