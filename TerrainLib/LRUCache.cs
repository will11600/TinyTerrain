using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace TinyTerrain
{
    internal sealed class LRUCache<T> : IEnumerable<ChunkPosition<T>>, IDisposable where T : BiomeSettings
    {
        private readonly Dictionary<Vector2UInt, LinkedListNode<ChunkPosition<T>>> cache;
        private readonly LinkedList<ChunkPosition<T>> lruList;
        private readonly ReaderWriterLockSlim rwLock;

        public int Count => cache.Count;
        public readonly int capacity;

        public LRUCache(int capacity)
        {
            this.capacity = capacity;
            cache = new Dictionary<Vector2UInt, LinkedListNode<ChunkPosition<T>>>(capacity);
            lruList = new LinkedList<ChunkPosition<T>>();
            rwLock = new ReaderWriterLockSlim();
        }

        public TerrainChunk<T>? Get(uint x, uint y)
        {
            return Get(new Vector2UInt(x, y));
        }

        public TerrainChunk<T>? Get(Vector2UInt position)
        {
            rwLock.EnterReadLock();
        
            try
            {
                if (!cache.TryGetValue(position, out LinkedListNode<ChunkPosition<T>>? node))
                {
                    return null;
                }

                lruList.Remove(node);
                lruList.AddFirst(node);

                return node.Value.chunk;
            }
            finally { rwLock.ExitReadLock(); }
        }

        public void Swap(Span<ChunkPosition<T>?> values)
        {
            rwLock.EnterWriteLock();

            try
            {
                for (int i = 0; i < values.Length; i++)
                {
                    if (values[i] is ChunkPosition<T> value) { Swap(value); }
                }
            }
            finally { rwLock.ExitWriteLock(); }
        }

        public ChunkPosition<T>? Swap(ChunkPosition<T> value)
        {
            rwLock.EnterWriteLock();

            try
            {
                if (cache.TryGetValue(value.position, out LinkedListNode<ChunkPosition<T>>? node))
                {
                    node.Value = value;
                    lruList.Remove(node);
                    lruList.AddFirst(node);

                    return null;
                }

                return AddNewNode(value);
            }
            finally { rwLock.ExitWriteLock(); }
        }

        private ChunkPosition<T>? AddNewNode(ChunkPosition<T> chunkPos)
        {
            LinkedListNode<ChunkPosition<T>> node = lruList.AddFirst(chunkPos);
            cache.Add(chunkPos.position, node);

            if (Count > capacity)
            {
                return EvictLeastRecentlyUsed();
            }

            return null;
        }

        private ChunkPosition<T> EvictLeastRecentlyUsed()
        {
            LinkedListNode<ChunkPosition<T>> lastNode = lruList.Last!;
            lruList.RemoveLast();
            cache.Remove(lastNode.Value.position);

            return lastNode.Value;
        }

        public IEnumerator<ChunkPosition<T>> GetEnumerator()
        {
            return ((IEnumerable<ChunkPosition<T>>)lruList).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)lruList).GetEnumerator();
        }

        public void Dispose()
        {
            rwLock.Dispose();
        }
    }
}