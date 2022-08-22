#region LICENSE
/* 
 * Distributed under the MIT License.
 * Copyright (c) 2019-2020 Viktor Zoric, Bjørnar Walle Alvestad, Endre Larsen
 * 
 * Read full license terms in the accompanying LICENSE file or at https://opensource.org/licenses/MIT
 */
#endregion

using System;
using System.Collections;
using System.Collections.Generic;

namespace Eval.Core.Util
{
    
    public class BitArrayList : IList<bool>, ICloneable
    {
        public BitArray BitArray { get; } // TODO: Optimize: create own implementation of BitArray
        public int Count => BitArray.Count;
        public bool IsReadOnly => BitArray.IsReadOnly;

        public BitArrayList(int length)
        {
            BitArray = new BitArray(length);
        }

        public BitArrayList(int length, bool defaultValue)
        {
            BitArray = new BitArray(length, defaultValue);
        }

        public BitArrayList(bool[] values)
        {
            BitArray = new BitArray(values);
        }

        public BitArrayList(BitArrayList original)
        {
            BitArray = new BitArray(original.BitArray);
        }

        public bool this[int index]
        {
            get => BitArray[index];
            set => BitArray[index] = value;
        }

        public void Clear()
        {
            BitArray.SetAll(false);
        }

        public IEnumerator<bool> GetEnumerator()
        {
            foreach (bool bit in BitArray)
            {
                yield return bit;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
            {
                return false;
            }
            var other = (BitArrayList)obj;
            if (Count != other.Count)
            {
                return false;
            }
            for (int i = 0; i < Count; i++)
            {
                if (BitArray[i] != other.BitArray[i])
                {
                    return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            return -1593857355 + EqualityComparer<BitArray>.Default.GetHashCode(BitArray);
        }

        public object Clone()
        {
            return new BitArrayList(this);
        }

        public void CopyTo(bool[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public void Add(bool item)
        {
            throw new NotImplementedException();
        }

        public bool Contains(bool item)
        {
            throw new NotImplementedException();
        }

        public int IndexOf(bool item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, bool item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(bool item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }
    }
}
