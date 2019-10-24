using System;
using System.Linq;
using Eval.Core.Config;
using Eval.Core.Models;
using Eval.Core.Selection.Parent;
using Eval.Core.Util.EARandom;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Eval.Core.Util;

namespace Eval.Test.Unit.Selection.Parent
{
    [TestClass]
    public class SigmaScalingParentSelectionTests
    {
        [TestMethod]
        public void SigmaScalingParentSelection_VerifySelectionProbability()
        {
            var random = new DefaultRandomNumberGenerator("seed".GetHashCode());
            var population = new Population(11);
            var index = 0;
            population.Fill(() => new TestPhenotype(index++, index * index));
            population.Sort(EAMode.MaximizeFitness);

            var selection = new SigmaScalingParentSelection();
            var selected = selection.SelectParents(population, 275000, EAMode.MaximizeFitness, random);

            foreach (var (a, b) in selected)
            {
                ((TestPhenotype)a).Count++;
                ((TestPhenotype)b).Count++;
            }

            var std = population.Select(p => p.Fitness).StandardDeviation();
            var avg = population.Select(p => p.Fitness).Average();
            var psum = population.Sum(p => 1 + (p.Fitness - avg) / (2 * std));

            foreach (TestPhenotype p in population)
            {
                var prob = (1 + (p.Fitness - avg) / (2 * std)) / psum;
                var expectedCount = prob * 275000 * 2;
                p.Count.Should().BeCloseTo((int)expectedCount, 1000);
            }
        }
    }
}
