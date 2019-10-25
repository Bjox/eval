using System;
using System.Linq;
using Eval.Core.Models;
using Eval.Core.Util.EARandom;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Eval.Test.Unit.Models
{
    [TestClass]
    public class GenotypeTests
    {
        private IRandomNumberGenerator random;

        [TestInitialize]
        public void TestInitialize()
        {
            random = new DefaultRandomNumberGenerator();
        }

        [TestMethod]
        public void CloneShouldCreateDeepCopy()
        {
            var factories = new Func<IGenotype>[]
            {
                () => new BinaryGenotype(10),
                () => new CharGenotype(new string('A', 10))
            };

            foreach (var factory in factories)
            {
                var geno = factory.Invoke();
                var genoClone = geno.Clone();

                geno.Should().NotBeSameAs(genoClone); // Reference check
                genoClone.Mutate(0.5, random);
                geno.Should().Be(factory.Invoke(), $"because {geno.GetType()}");
            }
        }
    }
}
