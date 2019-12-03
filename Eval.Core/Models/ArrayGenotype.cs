using System;
using System.Collections.Generic;
using System.Linq;
using Eval.Core.Util.EARandom;

namespace Eval.Core.Models
{
    public interface IGenotypeElement : ICloneable
    {
        void Mutate(double factor, IRandomNumberGenerator random);
    }

    /// <summary>
    /// A genotype represented by objects stored in an <c>Array</c>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class ArrayGenotype<T> : AbstractListGenotype<T[], T>
        where T : IGenotypeElement
    {
        public T[] Objects => Elements;

        public ArrayGenotype(int length)
            : base(length)
        {
        }

        public ArrayGenotype(T[] elements)
            : base(elements)
        {
        }

        protected override T CloneElement(T element)
        {
            return (T)element.Clone();
        }

        protected override T[] CreateArrayTypeOfLength(int length)
        {
            return new T[length];
        }

        protected override AbstractListGenotype<T[], T> CreateNewGenotype(T[] elements)
        {
            return new ArrayGenotype<T>(elements);
        }

        protected override T MutateElement(T element, double factor, IRandomNumberGenerator random)
        {
            element.Mutate(factor, random);
            return element;
        }

        public override bool Equals(object obj)
        {
            return obj is ArrayGenotype<T> genotype && Enumerable.SequenceEqual(Objects, genotype.Objects);
        }

        public override int GetHashCode()
        {
            var hashCode = 760891553;
            hashCode = hashCode * -1521134295 + EqualityComparer<T[]>.Default.GetHashCode(Objects);
            return hashCode;
        }
    }
}
