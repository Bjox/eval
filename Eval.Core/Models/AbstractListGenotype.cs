#region LICENSE
/* 
 * Distributed under the MIT License.
 * Copyright (c) 2019-2020 Viktor Zoric, Bjørnar Walle Alvestad, Endre Larsen
 * 
 * Read full license terms in the accompanying LICENSE file or at https://opensource.org/licenses/MIT
 */
#endregion

using Eval.Core.Util.EARandom;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Eval.Core.Models
{
    /// <summary>
    /// Extend from this class when your genotype consist of a collection of elements, and one of the following cases apply:<br></br>
    /// <list type="bullet">
    /// <item>Genotype elements are stored in an <c>IList</c>, as opposed to a regular <c>Array</c>.</item>
    /// <item>Genotype elements are value types, such as <c>struct</c> or a primitive. (However, reference types are also supported).</item>
    /// </list>
    /// Subclasses may override <c>Clone()</c>, <c>Mutate()</c> and/or <c>CrossoverWith()</c> for flexibility or optimization purposes.
    /// </summary>
    /// <typeparam name="AType">The array type</typeparam>
    /// <typeparam name="EType">The element type</typeparam>
    [Serializable]
    public abstract class AbstractListGenotype<AType, EType> : Genotype, IReadOnlyList<EType>
        where AType : IList<EType>
    {
        public int Count => Elements.Count;

        protected AType Elements { get; }

        protected AbstractListGenotype(int length)
        {
            Elements = CreateArrayTypeOfLength(length);
        }

        protected AbstractListGenotype(AType elements)
        {
            Elements = elements;
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

        public override IGenotype Clone()
        {
            var newElements = CreateArrayTypeOfLength(Count);
            for (int i = 0; i < Count; i++)
            {
                newElements[i] = CloneElement(this[i]);
            }
            return CreateNewGenotype(newElements);
        }

        public override void Mutate(double probability, IRandomNumberGenerator random)
        {
            for (int i = 0; i < Count; i++)
            {
                Elements[i] = MutateElement(this[i], probability, random);
            }
        }

        public override IGenotype CrossoverWith(IGenotype other, CrossoverType crossover, IRandomNumberGenerator random)
        {
            switch (crossover)
            {
                case CrossoverType.OnePoint: return OnePointCrossover((AbstractListGenotype<AType, EType>)other, random);
                case CrossoverType.Uniform: return UniformCrossover((AbstractListGenotype<AType, EType>)other, random);
                default:
                    throw new NotImplementedException($"Crossover {crossover} is not implemented in ArrayGenotype");
            }
        }

        protected virtual AbstractListGenotype<AType, EType> OnePointCrossover(AbstractListGenotype<AType, EType> other, IRandomNumberGenerator random)
        {
            if (Count != other.Count)
            {
                throw new ArgumentException("ArrayGenotypes differs in length"); // TODO: support variable length AbstractListGenotype
            }

            var length = Count;
            var splitIndex = random.Next(length + 1);
            var newElements = CreateArrayTypeOfLength(length);

            for (int i = 0; i < splitIndex; i++)
            {
                newElements[i] = CloneElement(this[i]);
            }
            for (int i = splitIndex; i < length; i++)
            {
                newElements[i] = CloneElement(other[i]);
            }

            return CreateNewGenotype(newElements);
        }

        protected virtual AbstractListGenotype<AType, EType> UniformCrossover(AbstractListGenotype<AType, EType> other, IRandomNumberGenerator random)
        {
            if (Count != other.Count)
            {
                throw new ArgumentException("ArrayGenotypes differs in length");
            }

            var length = Count;
            var newElements = CreateArrayTypeOfLength(length);

            for (int i = 0; i < length; i++)
            {
                newElements[i] = random.NextBool() ? CloneElement(this[i]) : CloneElement(other[i]);
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
        protected abstract AbstractListGenotype<AType, EType> CreateNewGenotype(AType elements);

        /// <summary>
        /// Mutates the specified element, where the provided factor describes the degree of mutation.
        /// The element may be mutated in-place and a reference to itself returned, or a mutated copy may be returned.
        /// </summary>
        /// <param name="element">The element to mutate</param>
        /// <param name="factor">A non-negative factor that determines the degree of mutation</param>
        /// <returns>The mutated element</returns>
        protected abstract EType MutateElement(EType element, double factor, IRandomNumberGenerator random);

        /// <summary>
        /// Returns a deep-clone of the specified element.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        protected abstract EType CloneElement(EType element);
    }
}
