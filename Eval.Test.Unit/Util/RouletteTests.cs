using System;
using System.Collections.Generic;
using Eval.Core.Models;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Eval.Core.Util.EARandom;
using Eval.Core.Util.Roulette;

namespace Eval.Test.Unit.Util
{
    class RouletteEntry
    {
        public int N { get; set; }
        public RouletteEntry(int n)
        {
            N = n;
        }
    }


    [TestClass]
    public class RouletteTests
    {

        [TestMethod]
        public void TestRouletteProbabilitiesWithoutRemoval()
        {
            var roulette = new Roulette<RouletteEntry>(new DefaultRandomNumberGenerator());

            roulette.Add(new RouletteEntry(0), 0.5);
            roulette.Add(new RouletteEntry(1), 0.25);
            roulette.Add(new RouletteEntry(2), 0.25);

            var count = new int[3];

            for (int i = 0; i < 1000000; i++)
            {
                var e = roulette.Spin();
                count[e.N]++;
            }

            double entry0 = count[0] / 10000.0;
            double entry1 = count[1] / 10000.0;
            double entry2 = count[2] / 10000.0;

            entry0.Should().BeApproximately(50, 1);
            entry1.Should().BeApproximately(25, 1);
            entry2.Should().BeApproximately(25, 1);
        }

        [TestMethod]
        public void TestRouletteProbabilitiesWithRemoval()
        {
            var count = new int[3];
            var count2 = new int[3];

            for (int i = 0; i < 1000000; i++)
            {
                var roulette = new Roulette<RouletteEntry>(new DefaultRandomNumberGenerator());

                roulette.Add(new RouletteEntry(0), 0.99990);
                roulette.Add(new RouletteEntry(1), 0.00009);
                roulette.Add(new RouletteEntry(2), 0.00001);


                var e1 = roulette.SpinAndRemove();
                count[e1.N]++;
                var e2 = roulette.SpinAndRemove();
                count2[e2.N]++;
            }

            double entry0 = count[0] / 10000.0;
            double entry1 = count[1] / 10000.0;
            double entry2 = count[2] / 10000.0;

            double entry20 = count2[0] / 10000.0;
            double entry21 = count2[1] / 10000.0;
            double entry22 = count2[2] / 10000.0;

            entry0.Should().BeApproximately(100, 1);
            entry1.Should().BeApproximately(0, 1);
            entry2.Should().BeApproximately(0, 1);

            entry20.Should().BeApproximately(0, 2);
            entry21.Should().BeApproximately(90, 2);
            entry22.Should().BeApproximately(10, 2);
        }

        [TestMethod]
        public void TestRouletteShouldThrowExceptionOnEmpty()
        {
            var roulette = new Roulette<RouletteEntry>(new DefaultRandomNumberGenerator());

            roulette.Add(new RouletteEntry(0), 1);

            roulette.Invoking(r => r.SpinAndRemove()).Should().NotThrow();
            roulette.Invoking(r => r.SpinAndRemove()).Should().Throw<InvalidOperationException>();
        }
    }
}
