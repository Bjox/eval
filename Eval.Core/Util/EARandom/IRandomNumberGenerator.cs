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

        /// <summary>
        /// Returns a random boolean value.
        /// </summary>
        /// <returns></returns>
        bool NextBool();

        /// <summary>
        /// Returns a random double value that is greater than or equal to 0.0, and less than the specified max value.
        /// </summary>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        double NextDouble(double maxValue);

        /// <summary>
        /// Returns a random double value between that is greater than or equal to minValue, and less than maxValue.
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        double NextDouble(double minValue, double maxValue);
    }
}
