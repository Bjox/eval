using Eval.Core.Models;
using Moq;

namespace Eval.Test.Unit.Selection.Parent
{
    public class TestPhenotype : Phenotype
    {
        private readonly double _mockedFitness;
        public int Index { get; }
        public int Count { get; set; }
        public int Count2 { get; set; }

        public TestPhenotype(int index, double mockedFitness)
            : base(new Mock<IGenotype>().Object)
        {
            Index = index;
            _mockedFitness = mockedFitness;
            Evaluate();
        }

        protected override double CalculateFitness()
        {
            return _mockedFitness;
        }
    }
}
