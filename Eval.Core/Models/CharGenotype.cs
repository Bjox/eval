using Eval.Core.Util.EARandom;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Eval.Core.Models
{
    /// <summary>
    /// A character based genotype of fixed length, supporting ASCII characters.
    /// </summary>
    public class CharGenotype : AbstractListGenotype<char[], char>
    {
        public char[] Chars => Elements;

        public CharGenotype(int length)
            : base(length)
        {
        }

        public CharGenotype(char[] chars)
            : base(chars)
        {
        }

        public CharGenotype(string chars)
            : this (chars.ToCharArray())
        {
        }

        public string ToCharString()
        {
            return new string(Chars);
        }

        protected override char[] CreateArrayTypeOfLength(int length)
        {
            return new char[length];
        }

        protected override AbstractListGenotype<char[], char> CreateNewGenotype(char[] elements)
        {
            return new CharGenotype(elements);
        }

        protected override char MutateElement(char element, double factor, IRandomNumberGenerator random)
        {
            if (random.NextDouble() < factor)
            {
                return (char)random.Next(32, 127); // 32 = ' ', 126 = '~'
            }
            return element;
        }

        public override bool Equals(object obj)
        {
            return obj is CharGenotype genotype && Enumerable.SequenceEqual(Chars, genotype.Chars);
        }

        public override int GetHashCode()
        {
            return -967413158 + EqualityComparer<char[]>.Default.GetHashCode(Chars);
        }

        protected override char CloneElement(char element)
        {
            return element;
        }
    }
}
