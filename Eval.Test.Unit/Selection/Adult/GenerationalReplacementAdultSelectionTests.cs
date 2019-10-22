using Eval.Core.Config;
using Eval.Core.Models;
using Eval.Core.Selection.Adult;
using Eval.Core.Util.EARandom;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Eval.Test.Unit.Selection.Adult
{
    [TestClass]
    public class GenerationalReplacementAdultSelectionTests
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

            var genmix = new GenerationalReplacementAdultSelection();
            genmix.SelectAdults(offspring, population, 2, EAMode.MaximizeFitness);

            Assert.IsTrue(population.Contains(o0));
            Assert.IsTrue(population.Contains(o1));
            Assert.IsFalse(population.Contains(p0));
            Assert.IsFalse(population.Contains(p1));
        }
    }
}
