using System;
using System.Linq;
using Eval.Core.Config;
using Eval.Core.Models;
using Eval.Core.Selection.Parent;
using Eval.Core.Util.EARandom;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Eval.Core.Util;
using System.Diagnostics;

namespace Eval.Test.Unit.Selection.Parent
{
    [TestClass]
    public class ProportionateParentSelectionTests
    {
        [TestMethod]
        public void SelectParentsShouldSelectCorrectNumber()
        {
            var random = new DefaultRandomNumberGenerator("seed".GetHashCode());
            var population = new Population(20);
            for (int i = 0; i < population.Size; i++)
            {
                var pmock = new Mock<IPhenotype>();
                pmock.SetupGet(p => p.Fitness).Returns(i);
                pmock.SetupGet(p => p.IsEvaluated).Returns(true);
                population.Add(pmock.Object);
            }

            var selection = new ProportionateParentSelection();
            var selected = selection.SelectParents(population, 20, EAMode.MaximizeFitness, random);
            selected.Count().Should().Be(20);
        }

        [TestMethod]
        public void ProportionateParentSelection_VerifySelectionProbability()
        {
            var random = new DefaultRandomNumberGenerator("seed".GetHashCode());
            var population = new Population(11);
            var index = 0;
            population.Fill(() => new TestPhenotype(index, index++));
            population.Sort(EAMode.MaximizeFitness);

            var selection = new ProportionateParentSelection();
            var selected = selection.SelectParents(population, 275000, EAMode.MaximizeFitness, random);

            var bucket = new int[population.Size];
            foreach (var (a, b) in selected)
            {
                bucket[((TestPhenotype)a).Index]++;
                bucket[((TestPhenotype)b).Index]++;
            }

            for (int i = 0; i < bucket.Length; i++)
            {
                bucket[i].Should().BeCloseTo(i * 10000, 1000);
            }
        }
    }
}
