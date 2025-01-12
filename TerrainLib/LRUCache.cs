using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace TinyTerrain;

internal sealed class LRUCache<T>(int capacity) : IEnumerable<ChunkPosition<T>>, IDisposable where T : struct, IDivisionOperators<T, int, T>, IAdditionOperators<T, T, T>
{
    private readonly Dictionary<Vector2UInt, LinkedListNode<ChunkPosition<T>>> cache = [];
    private readonly LinkedList<ChunkPosition<T>> lruList = new();
    private readonly ReaderWriterLockSlim rwLock = new();

    public int Count => cache.Count;
    public int Capacity { get; init; } = capacity;

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
                return Unsafe.NullRef<TerrainChunk<T>>();
            }

            lruList.Remove(node);
            lruList.AddFirst(node);

            return node.ValueRef.chunk;
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
                if (values[i] is not ChunkPosition<T> value) { continue; }
                values[i] = Swap(value);
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
                node.ValueRef = value;
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

        return Count > Capacity ? EvictLeastRecentlyUsed() : null;
    }

    private ChunkPosition<T> EvictLeastRecentlyUsed()
    {
        LinkedListNode<ChunkPosition<T>> lastNode = lruList.Last!;
        lruList.RemoveLast();
        cache.Remove(lastNode.ValueRef.position);

        return lastNode.ValueRef;
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