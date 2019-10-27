using System;
using System.Collections.Generic;
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
        private Func<IGenotype>[] genotypefactories;

        [TestInitialize]
        public void TestInitialize()
        {
            random = new DefaultRandomNumberGenerator();
            genotypefactories = new Func<IGenotype>[] // TODO: make it possible for 3rd party to add custom genotypes here
            {
                () => new BinaryGenotype(10),
                () => new CharGenotype(new string('A', 10)),
                () => new ArrayGenotype<TestGenoElement>(Repeat(() => new TestGenoElement(0), 10).ToArray())
            };
        }

        [TestMethod]
        public void EqualsShouldCheckForValueEquality()
        {
            ForEachFactory(factory =>
            {
                var geno1 = factory();
                var geno2 = factory();

                geno1.Should().Be(geno2);
                geno1.Equals(geno2).Should().BeTrue();
                geno1.Should().NotBeSameAs(geno2);

                geno1.Mutate(1, random);
                geno1.Should().NotBe(geno2);
                geno1.Equals(geno2).Should().BeFalse();
            });
        }

        [TestMethod]
        public void CloneShouldCreateDeepCopy()
        {
            ForEachFactory(factory =>
            {
                var geno = factory();
                var genoClone = geno.Clone();

                geno.Should().NotBeSameAs(genoClone);
                genoClone.Mutate(1, random);
                geno.Should().Be(factory());
            });
        }

        [TestMethod]
        public void CrossoverShouldCreateDeepCopy()
        {
            CloneShouldCreateDeepCopy(); // Fail this test if CloneShouldCreateDeepCopy fails

            void TestUsing(CrossoverType crossover, Func<IGenotype> factory)
            {
                Console.WriteLine($"Using crossover {crossover}");
                var geno1 = factory();
                var geno2 = factory();
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

            ForEachFactory(factory =>
            {
                TestUsing(CrossoverType.OnePoint, factory);
                TestUsing(CrossoverType.Uniform, factory);
            });
        }

        [TestMethod]
        public void ClonedGenotypeShouldEqualOriginal()
        {
            ForEachFactory(factory =>
            {
                var geno = factory();
                var clone = geno.Clone();
                clone.Should().Be(geno);
            });
        }

        [TestMethod]
        public void MutatedGenotypeShouldNotEqualOriginal()
        {
            ForEachFactory(factory =>
            {
                var geno = factory();
                geno.Mutate(1, random);
                geno.Should().NotBe(factory());
            });
        }

        [TestMethod]
        public void MutateWithZeroProbabilityShouldNotMutateGenotype()
        {
            ForEachFactory(factory =>
            {
                var geno = factory();
                geno.Mutate(0.0, random);
                geno.Should().Be(factory());
            });
        }

        private void ForEachFactory(Action<Func<IGenotype>> action)
        {
            foreach (var factory in genotypefactories)
            {
                Console.WriteLine($"Testing {factory().GetType()}");
                action(factory);
            }
        }

        private IEnumerable<T> Repeat<T>(Func<T> elementFactory, int n)
        {
            for (int i = 0; i < n; i++)
            {
                yield return elementFactory();
            }
        }
    }

    public class TestGenoElement : GenotypeElement
    {
        public double Value { get; set; }

        public TestGenoElement(double value)
        {
            Value = value;
        }

        public override object Clone()
        {
            return new TestGenoElement(Value);
        }

        public override void Mutate(double factor, IRandomNumberGenerator random)
        {
            Value += factor * random.NextDouble(-1, 1);
        }

        public override bool Equals(object obj)
        {
            return obj is TestGenoElement other && Value == other.Value;
        }

        public override int GetHashCode()
        {
            var hashCode = -159790080;
            hashCode = hashCode * -1521134295 + Value.GetHashCode();
            return hashCode;
        }
    }
}
