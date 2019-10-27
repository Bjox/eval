using System;
using System.Collections.Generic;
using System.Linq;
using Eval.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Eval.Test.Unit.Models
{
    [TestClass]
    public class ArrayGenotypeTests : AbstractListGenotypeTestBase<ArrayGenotype<TestGenoElement>, TestGenoElement[], TestGenoElement>
    {
        protected override ArrayGenotype<TestGenoElement> GetFirstGenotype(Func<Func<TestGenoElement>, IEnumerable<TestGenoElement>> elementFactory)
        {
            return new ArrayGenotype<TestGenoElement>(elementFactory(() => new TestGenoElement(1)).ToArray()); // All 1
        }

        protected override ArrayGenotype<TestGenoElement> GetSecondGenotype(Func<Func<TestGenoElement>, IEnumerable<TestGenoElement>> elementFactory)
        {
            return new ArrayGenotype<TestGenoElement>(elementFactory(() => new TestGenoElement(2)).ToArray()); // All 2
        }
    }
}
