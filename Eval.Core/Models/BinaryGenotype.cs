using Eval.Core.Util.EARandom;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Eval.Core.Models
{
    public class BinaryGenotype : IGenotype, IEnumerable<bool>
    {
        private readonly BitArray bits;

        public int Length { get { return bits.Count; } }

        public BinaryGenotype(int bitcount)
        {
            bits = new BitArray(bitcount);
        }

        public BinaryGenotype(BinaryGenotype original)
        {
            bits = new BitArray(original.bits);
        }

        public BinaryGenotype(BitArray bits)
        {
            this.bits = bits;
        }

        public bool this[int key]
        {
            get => bits[key];
        }

        public IGenotype Clone()
        {
            return new BinaryGenotype(this);
        }

        public IGenotype CrossoverWith(IGenotype other, Crossover crossover, IRandomNumberGenerator random)
        {
            switch (crossover)
            {
                case Crossover.OnePoint: return OnePointCrossover(this, (BinaryGenotype)other, random);
                case Crossover.Uniform: return UniformCrossover(this, (BinaryGenotype)other, random);
                default:
                    throw new NotImplementedException($"Crossover {crossover} is not implemented in BinaryGenotype");
            }
        }

        public void Mutate(double probability, IRandomNumberGenerator random)
        {
            for (int i = 0; i < Length; i++)
            {
                if (random.NextDouble() < probability)
                {
                    bits[i] = !bits[i];
                }
            }
        }

        public IEnumerator<bool> GetEnumerator()
        {
            foreach (bool bit in bits)
            {
                yield return bit;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public string ToBitString()
        {
            var sb = new StringBuilder(Length);
            foreach (bool bit in bits)
            {
                sb.Append(bit ? "1" : "0");
            }
            return sb.ToString();
        }

        private static BinaryGenotype OnePointCrossover(BinaryGenotype g1, BinaryGenotype g2, IRandomNumberGenerator random)
        {
            if (g1.Length != g2.Length)
            {
                throw new ArgumentException("BinaryGenotypes differs in length");
            }

            var length = g1.Length;
            var splitIndex = random.Next(length + 1);
            var newBits = new BitArray(length);

            for (int i = 0; i < splitIndex; i++)
            {
                newBits[i] = g1.bits[i];
            }
            for (int i = splitIndex; i < length; i++)
            {
                newBits[i] = g2.bits[i];
            }

            return new BinaryGenotype(newBits);
        }

        private static BinaryGenotype UniformCrossover(BinaryGenotype g1, BinaryGenotype g2, IRandomNumberGenerator random)
        {
            throw new NotImplementedException();
        }

    }
}
