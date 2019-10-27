using System;
using System.Collections.Generic;
using System.Linq;
using Eval.Core.Models;
using Eval.Core.Util.EARandom;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Eval.Test.Unit.Models
{
    /// <summary>
    /// Extend from this class in order to test a custom genotype that implements <c>IGenotype</c>.
    /// </summary>
    /// <typeparam name="GType">The genotype Type being tested</typeparam>
    [TestClass]
    public abstract class GenotypeTestBase<GType>
        where GType : IGenotype
    {
        protected abstract GType CreateGenotype { get; }

        private IRandomNumberGenerator random;

        [TestInitialize]
        public void TestInitialize()
        {
            random = new DefaultRandomNumberGenerator();
        }

        [TestMethod]
        public void EqualsShouldCheckForValueEquality()
        {
            var geno1 = CreateGenotype;
            var geno2 = CreateGenotype;

            geno1.Should().Be(geno2);
            geno1.Equals(geno2).Should().BeTrue();
            geno1.Should().NotBeSameAs(geno2);

            geno1.Mutate(1, random);
            geno1.Should().NotBe(geno2);
            geno1.Equals(geno2).Should().BeFalse();
        }

        [TestMethod]
        public void CloneShouldCreateDeepCopy()
        {
            var geno = CreateGenotype;
            var genoClone = geno.Clone();

            geno.Should().NotBeSameAs(genoClone);
            genoClone.Mutate(1, random);
            geno.Should().Be(CreateGenotype);
        }

        [TestMethod]
        public void CrossoverShouldCreateDeepCopy()
        {
            CloneShouldCreateDeepCopy(); // Fail this test if CloneShouldCreateDeepCopy fails

            void TestUsing(CrossoverType crossover)
            {
                Console.WriteLine($"Using crossover {crossover}");
                var geno1 = CreateGenotype;
                var geno2 = CreateGenotype;
                geno1.Mutate(1, random);
                geno2.Mutate(1, random);

                var crossoverGeno = geno1.CrossoverWith(geno2, crossover, random);
                var crossoverClone = crossoverGeno.Clone(); // Assumes that Clone creates a deep-copy (tested in CloneShouldCreateDeepCopy)
                crossoverGeno.Should().NotBeSameAs(crossoverClone);
                crossoverGeno.Should().Be(crossoverClone);

                geno1.Mutate(1, random);
                geno2.Mutate(1, random);

                crossoverGeno.Should().Be(crossoverClone);
            }

            TestUsing(CrossoverType.OnePoint);
            TestUsing(CrossoverType.Uniform);
        }

        [TestMethod]
        public void ClonedGenotypeShouldEqualOriginal()
        {
            var geno = CreateGenotype;
            var clone = geno.Clone();
            clone.Should().Be(geno);
        }

        [TestMethod]
        public void MutatedGenotypeShouldNotEqualOriginal()
        {
            var geno = CreateGenotype;
            geno.Mutate(1, random);
            geno.Should().NotBe(CreateGenotype);
        }

        [TestMethod]
        public void MutateWithZeroProbabilityShouldNotMutateGenotype()
        {
            var geno = CreateGenotype;
            geno.Mutate(0.0, random);
            geno.Should().Be(CreateGenotype);
        }

        protected IEnumerable<T> Repeat<T>(Func<T> elementFactory, int n)
        {
            for (int i = 0; i < n; i++)
            {
                yield return elementFactory();
            }
        }
    }
}
