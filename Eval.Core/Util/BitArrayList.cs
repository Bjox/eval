using System;
using System.Collections;
using System.Collections.Generic;

namespace Eval.Core.Util
{
    public class BitArrayList : IList<bool>
    {
        public BitArray BitArray { get; }
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
