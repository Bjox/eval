#region LICENSE
/* 
 * Distributed under the MIT License.
 * Copyright (c) 2019-2020 Viktor Zoric, Bjørnar Walle Alvestad, Endre Larsen
 * 
 * Read full license terms in the accompanying LICENSE file or at https://opensource.org/licenses/MIT
 */
#endregion

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
        public void CloneShouldNotBeSameAsOriginal()
        {
            var geno = CreateGenotype;
            var genoClone = geno.Clone();
            geno.Should().NotBeSameAs(genoClone);
        }

        [TestMethod]
        public void MutatedGenotypeShouldNotEqualOriginal()
        {
            var geno = CreateGenotype;
            geno.Mutate(1, random);
            geno.Should().NotBe(CreateGenotype);
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
