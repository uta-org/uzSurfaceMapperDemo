using System;

namespace IngameDebugConsole
{
    public class DebugLogIndexList
    {
        private int[] indices;

        public DebugLogIndexList()
        {
            indices = new int[64];
            Count = 0;
        }

        public int Count { get; private set; }

        public int this[int index] => indices[index];

        public void Add(int index)
        {
            if (Count == indices.Length)
            {
                var indicesNew = new int[Count * 2];
                Array.Copy(indices, 0, indicesNew, 0, Count);
                indices = indicesNew;
            }

            indices[Count++] = index;
        }

        public void Clear()
        {
            Count = 0;
        }
    }
}