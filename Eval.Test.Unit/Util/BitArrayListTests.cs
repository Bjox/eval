using System;
using Eval.Core.Util;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Eval.Test.Unit.Util
{
    [TestClass]
    public class BitArrayListTests
    {
        [TestMethod]
        public void EqualsShouldCompareValue()
        {
            var a = new BitArrayList(10);
            var b = new BitArrayList(10);
            a.Equals(b).Should().BeTrue();
            a[3] = true;
            a.Equals(b).Should().BeFalse();
            b[3] = true;
            a.Equals(b).Should().BeTrue();
        }
    }
}
