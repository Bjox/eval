using System;
using System.Collections.Generic;
using System.Linq;
using Eval.Core.Config;
using Eval.Core.Models;
using Eval.Core.Selection.Adult;
using Eval.Core.Util.EARandom;
using Eval.Core.Util.Roulette;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Eval.Test.Unit.Selection.Adult
{

    [TestClass]
    public class GenerationalMixingAdultSelectionTests
    {
        private class P : Phenotype
        {
            public double F;
            public string Name;
            public P(string name, double fitness)
                : base(null)
            {
                Name = name;
                F = fitness;
                Evaluate();
            }
            public override bool Equals(object obj)
            {
                return obj is P p && Name == p.Name;
            }
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
            protected override double CalculateFitness()
            {
                return F;
            }
        }

        [TestMethod]
        public void TestSelection()
        {
            var offspring = new Population(2);
            var population = new Population(2);

            var o0 = new P("o0", 1.0);
            offspring.Add(o0);
            var o1 = new P("o1", 0.0);
            offspring.Add(o1);
            var p0 = new P("p0", 1.0);
            population.Add(p0);
            var p1 = new P("p1", 0.0);
            population.Add(p1);

            var genmix = new GenerationalMixingAdultSelection(new DefaultRandomNumberGenerator());
            genmix.SelectAdults(offspring, population, 2, EAMode.MaximizeFitness);

            Assert.IsTrue(population.Contains(o0));
            Assert.IsFalse(population.Contains(o1));
            Assert.IsTrue(population.Contains(p0));
            Assert.IsFalse(population.Contains(p1));
        }

        [TestMethod]
        public void TestSelectionWithMinimizing()
        {
            var offspring = new Population(2);
            var population = new Population(2);

            var o0 = new P("o0", 1.0);
            offspring.Add(o0);
            var o1 = new P("o1", 0.0);
            offspring.Add(o1);
            var p0 = new P("p0", 1.0);
            population.Add(p0);
            var p1 = new P("p1", 0.0);
            population.Add(p1);

            var genmix = new GenerationalMixingAdultSelection(new DefaultRandomNumberGenerator());
            genmix.SelectAdults(offspring, population, 2, EAMode.MinimizeFitness);

            Assert.IsFalse(population.Contains(o0));
            Assert.IsTrue(population.Contains(o1));
            Assert.IsFalse(population.Contains(p0));
            Assert.IsTrue(population.Contains(p1));
        }

        [TestMethod]
        public void TestSelectionWithWrongN()
        {
            var offspring = new Population(2);
            var population = new Population(2);

            var o0 = new P("o0", 1.0);
            offspring.Add(o0);
            var o1 = new P("o1", 0.0);
            offspring.Add(o1);
            var p0 = new P("p0", 1.0);
            population.Add(p0);
            var p1 = new P("p1", 0.0);
            population.Add(p1);

            var genmix = new GenerationalMixingAdultSelection(new DefaultRandomNumberGenerator());
            genmix.Invoking(g => g.SelectAdults(offspring, population, 3, EAMode.MaximizeFitness))
                .Should().Throw<InvalidOperationException>();
        }
        
    }
}
