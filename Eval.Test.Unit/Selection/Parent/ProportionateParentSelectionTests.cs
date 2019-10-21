using System.Linq;
using Eval.Core.Config;
using Eval.Core.Models;
using Eval.Core.Selection.Parent;
using Eval.Core.Util.EARandom;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Eval.Test.Unit.Selection.Parent
{
    [TestClass]
    public class ProportionateParentSelectionTests
    {
        private IRandomNumberGenerator random;
        private Population population;

        [TestInitialize]
        public void TestInitialize()
        {
            random = new DefaultRandomNumberGenerator("seed".GetHashCode());
            population = new Population(20);
            for (int i = 0; i < population.Size; i++)
            {
                var pmock = new Mock<IPhenotype>();
                pmock.SetupGet(p => p.Fitness).Returns(i);
                pmock.SetupGet(p => p.IsEvaluated).Returns(true);
                population.Add(pmock.Object);
            }
        }

        [TestMethod]
        public void SelectParentsShouldSelectCorrectNumber()
        {
            var selection = new ProportionateParentSelection();
            var selected = selection.SelectParents(population, 20, EAMode.MaximizeFitness, random);
            selected.Count().Should().Be(20);
        }

        // TODO: implement VerifySelectionProbability test like we have for Rank selection
    }
}
