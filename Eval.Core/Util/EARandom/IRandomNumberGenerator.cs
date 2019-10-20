using System;
using System.Collections.Generic;
using System.Text;

namespace Eval.Core.Util.EARandom
{
    public interface IRandomNumberGenerator
    {
        /// <summary>
        /// Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0.
        /// </summary>
        /// <returns></returns>
        double NextDouble();

        /// <summary>
        /// Returns a non-negative random integer.
        /// </summary>
        /// <returns></returns>
        int Next();

        /// <summary>
        /// Returns a non-negative random integer that is less than the specified maximum.
        /// </summary>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        int Next(int maxValue);

        /// <summary>
        /// Returns a random integer that is within a specified range.
        /// </summary>
        /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
        /// <param name="maxValue">The exclusive upper bound of the random number returned.</param>
        /// <returns></returns>
        int Next(int minValue, int maxValue);

        /// <summary>
        /// Fills the elements of a specified array of bytes with random numbers.
        /// </summary>
        /// <param name="buffer"></param>
        void NextBytes(byte[] buffer);

        bool NextBool();
    }
}
