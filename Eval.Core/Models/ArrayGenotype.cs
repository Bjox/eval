using Eval.Core.Util.EARandom;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Eval.Core.Models
{
    /// <summary>
    /// </summary>
    /// <typeparam name="AType">The array type</typeparam>
    /// <typeparam name="EType">The element type</typeparam>
    public abstract class ArrayGenotype<AType, EType> : IGenotype, IReadOnlyList<EType> where AType : IList<EType>
    {
        public int Count => Elements.Count;

        protected AType Elements { get; }

        protected ArrayGenotype(int length)
        {
            Elements = CreateArrayTypeOfLength(length);
        }

        protected ArrayGenotype(AType elements)
        {
            this.Elements = elements;
        }

        public EType this[int key]
        {
            get => Elements[key];
            protected set => Elements[key] = value;
        }

        public IEnumerator<EType> GetEnumerator()
        {
            return Elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IGenotype CrossoverWith(IGenotype other, CrossoverType crossover, IRandomNumberGenerator random)
        {
            switch (crossover)
            {
                case CrossoverType.OnePoint: return OnePointCrossover(this, (ArrayGenotype<AType, EType>)other, random);
                case CrossoverType.Uniform: return UniformCrossover(this, (ArrayGenotype<AType, EType>)other, random);
                default:
                    throw new NotImplementedException($"Crossover {crossover} is not implemented in ArrayGenotype");
            }
        }

        public IGenotype Clone()
        {
            var newElements = CreateArrayTypeOfLength(Count);
            for (int i = 0; i < Count; i++)
            {
                newElements[i] = this[i];
            }
            return CreateNewGenotype(newElements);
        }

        public virtual void Mutate(double probability, IRandomNumberGenerator random)
        {
            for (int i = 0; i < Count; i++)
            {
                Elements[i] = MutateElement(this[i], probability, random);
            }
        }

        private ArrayGenotype<AType, EType> OnePointCrossover(ArrayGenotype<AType, EType> g1, ArrayGenotype<AType, EType> g2, IRandomNumberGenerator random)
        {
            if (g1.Count != g2.Count)
            {
                throw new ArgumentException("ArrayGenotypes differs in length"); // TODO: support
            }

            var length = g1.Count;
            var splitIndex = random.Next(length + 1);
            var newElements = CreateArrayTypeOfLength(length);

            for (int i = 0; i < splitIndex; i++)
            {
                newElements[i] = g1[i];
            }
            for (int i = splitIndex; i < length; i++)
            {
                newElements[i] = g2[i];
            }

            return CreateNewGenotype(newElements);
        }

        private ArrayGenotype<AType, EType> UniformCrossover(ArrayGenotype<AType, EType> g1, ArrayGenotype<AType, EType> g2, IRandomNumberGenerator random)
        {
            if (g1.Count != g2.Count)
            {
                throw new ArgumentException("ArrayGenotypes differs in length"); // TODO: support
            }

            var length = g1.Count;
            var newElements = CreateArrayTypeOfLength(length);

            for (int i = 0; i < length; i++)
            {
                newElements[i] = random.NextBool() ? g1[i] : g2[i];
            }

            return CreateNewGenotype(newElements);
        }

        /// <summary>
        /// Create a new, empty array of type AType with the specified length.
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        protected abstract AType CreateArrayTypeOfLength(int length);

        /// <summary>
        /// Create a new genotype using the specified elements.
        /// </summary>
        /// <param name="elements"></param>
        /// <returns></returns>
        protected abstract ArrayGenotype<AType, EType> CreateNewGenotype(AType elements);

        /// <summary>
        /// Mutates of the specified element, where the provided factor describes the degree og mutation.
        /// The element may be mutated in-place and a reference to itself returned, or a mutated copy may be returned.
        /// </summary>
        /// <param name="element">The element to mutate</param>
        /// <param name="factor">A non-negative factor that determines the degree of mutation</param>
        /// <returns>The mutated element</returns>
        protected abstract EType MutateElement(EType element, double factor, IRandomNumberGenerator random);
    }
}
