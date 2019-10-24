using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Eval.Core.Util;
using FluentAssertions;

namespace Eval.Test.Unit.Util
{
    [TestClass]
    public class MathUtilTests
    {
        [TestMethod]
        public void StandardDeviationTest()
        {
            var values = new double[] { 10, 12, 23, 23, 16, 23, 21, 16 };
            values.StandardDeviation().Should().BeApproximately(4.898979485566, 1e-12);
        }
    }
}
