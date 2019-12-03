using Eval.Core.Util.EARandom;

namespace Eval.Core.Models
{
    public interface IGenotype
    {
        /// <summary>
        /// Returns a deep-clone of the current genotype.
        /// </summary>
        /// <returns></returns>
        IGenotype Clone();
        /// <summary>
        /// Performs the specified crossover operator on the current and the specified genotype.<br></br>
        /// The returned value must have all of its members deep-copied from the operands.
        /// </summary>
        /// <param name="other"></param>
        /// <param name="crossover"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        IGenotype CrossoverWith(IGenotype other, CrossoverType crossover, IRandomNumberGenerator random);
        /// <summary>
        /// Mutates this genotype.
        /// </summary>
        /// <param name="probability"></param>
        /// <param name="random"></param>
        void Mutate(double probability, IRandomNumberGenerator random);
    }
}
