using System;
using System.Collections.Generic;
using System.Text;

namespace Eval.Core.Util
{
    public static class MathUtil
    {
        /// <summary>
        /// Positive modulus. For non-negative x values, it returns x % m.
        /// For negative x values, it returns the "wrapped around" remainder.
        /// This method always returns a non-negative result.
        /// E.g:
        /// PositiveMod(5, 5) = 0
        /// PositiveMod(1, 5) = 1
        /// PositiveMod(0, 5) = 0
        /// PositiveMod(-1, 5) = 4
        /// PositiveMod(-5, 5) = 0
        /// </summary>
        /// <param name="x"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        public static int PositiveMod(int x, int m)
        {
            return (x % m + m) % m;
        }

        /// <summary>
        /// Computes the standard deviation of a sequence double values.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static double StandardDeviation(this IEnumerable<double> values)
        {
            double M = 0.0;
            double S = 0.0;
            int k = 1;
            foreach (double value in values)
            {
                double tmpM = M;
                M += (value - tmpM) / k;
                S += (value - tmpM) * (value - M);
                k++;
            }
            return Math.Sqrt(S / (k - 1));
        }
    }
}
