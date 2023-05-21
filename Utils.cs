using System;
using System.IO;
using System.Collections.Generic;

namespace AIProg
{
    public class DynamicSet
    {
        // holds a set of integers
        // allows for incremental removal of items, and restoring the set to a previous point

        public int Size;
        public int Count;
        private int[] Data;

        public DynamicSet(int n)
        {
            Data = new int[n];
            Size = n;
            Count = 0;
        }

        public void Init()
        {
            for (int i = 0; i < Size; i++)
            {
                Data[i] = i;
            }

            Count = Size;
        }

        public int Get(int i)
        {
            return Data[i];
        }

        public void Remove(int i)
        {
            Count -= 1;

            Swap(i, Count);
        }

        public void Restore(int n)
        {
            Count = n;
        }

        private void Swap(int i, int j)
        {
            int x = Data[i];
            int y = Data[j];

            Data[i] = y;
            Data[j] = x;
        }
    }
}