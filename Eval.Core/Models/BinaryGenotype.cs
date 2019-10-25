﻿using Eval.Core.Util;
using Eval.Core.Util.EARandom;
using System;
using System.Collections.Generic;
using System.Text;

namespace Eval.Core.Models
{
    public class BinaryGenotype : ArrayGenotype<BitArrayList, bool>
    {
        public BitArrayList Bits => Elements;

        public BinaryGenotype(int length)
            : base(length)
        {
        }

        public BinaryGenotype(BitArrayList bits)
            : base(bits)
        {
        }

        public string ToBitString()
        {
            var sb = new StringBuilder(Count);
            foreach (bool bit in Bits)
            {
                sb.Append(bit ? "1" : "0");
            }
            return sb.ToString();
        }

        protected override BitArrayList CreateArrayTypeOfLength(int length)
        {
            return new BitArrayList(length);
        }

        protected override ArrayGenotype<BitArrayList, bool> CreateNewGenotype(BitArrayList elements)
        {
            return new BinaryGenotype(elements);
        }

        protected override bool MutateElement(bool element, double factor, IRandomNumberGenerator random)
        {
            return random.NextDouble() < factor ? !element : element;
        }

        public override bool Equals(object other)
        {
            if (other == null || !GetType().Equals(other.GetType()))
            {
                return false;
            }
            return Bits.Equals(((BinaryGenotype) other).Bits);
        }

        public override int GetHashCode()
        {
            return -943821695 + EqualityComparer<BitArrayList>.Default.GetHashCode(Bits);
        }
    }
}
