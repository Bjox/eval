using Eval.Core.Util.EARandom;

namespace Eval.Core.Models
{
    public class CharGenotype : ArrayGenotype<char[], char>
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

        protected override ArrayGenotype<char[], char> CreateNewGenotype(char[] elements)
        {
            return new CharGenotype(elements);
        }

        protected override char MutateElement(char element, double factor, IRandomNumberGenerator random)
        {
            if (random.NextDouble() < factor)
            {
                return (char)random.Next(65, 91); // A = 65, Z = 90
            }
            return element;
        }
    }
}
