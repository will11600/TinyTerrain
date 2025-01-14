using System.Numerics;

namespace TinyTerrain
{
    /// <summary>
    /// Represents a terrain composed of chunks.
    /// </summary>
    /// <typeparam name="T">The biome type</typeparam>
    public sealed class Terrain<T> : IDisposable where T : BiomeSettings
    {
        private const string INDEX_OUT_OF_RANGE_MSG = "The requested chunk is outside the bounds of the terrain.";

        /// <summary>
        /// Triggered when a chunk is loaded
        /// </summary>
        public static event Action<TerrainChunk<T>, Vector2UInt>? ChunkLoaded;

        /// <summary>
        /// The width of the terrain in chunks
        /// </summary>
        public readonly uint width;
        /// <summary>
        /// The height of the terrain in chunks
        /// </summary>
        public readonly uint height;

        /// <summary>
        /// The biomes used in the terrain
        /// </summary>
        public IBiome<T>[] biomes;

        private readonly uint resolution;
        private readonly uint headerLength;

        private readonly LRUCache<T> cache;

        private readonly FileStream stream;

        private readonly LinkedList<WeakReference> handles = [];
        private CancellationTokenSource? cancellationTokenSource;
        private Thread? terrainThread;

        /// <summary>
        /// Gets or sets the chunk at the specified X and Z index.
        /// </summary>
        /// <param name="x">The X index of the chunk</param>
        /// <param name="z">The Z index of the chunk</param>
        /// <returns>The chunk at the specified index</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the requested chunk is outside the bounds of the terrain</exception>
        public TerrainChunk<T> this[uint x, uint z]
        {
            get
            {
                if (x < 0 || x >= width)
                {
                    throw new ArgumentOutOfRangeException(nameof(x), x, INDEX_OUT_OF_RANGE_MSG);
                }

                if (z < 0 || z >= height)
                {
                    throw new ArgumentOutOfRangeException(nameof(z), z, INDEX_OUT_OF_RANGE_MSG);
                }

                if (cache.Get(x, z) is TerrainChunk<T> chunk)
                {
                    return chunk;
                }

                Span<byte> buffer = stackalloc byte[TerrainChunk<T>.BUFFER_SIZE];
                chunk = ReadChunk(x, z, buffer);

                if (cache.Swap(new ChunkPosition<T>(x, z, chunk)) is ChunkPosition<T> chunkPos)
                {
                    WriteChunk(chunkPos, buffer);
                }

                return chunk;
            }
            set
            {
                if (x < 0 || x >= width)
                {
                    throw new ArgumentOutOfRangeException(nameof(x), x, INDEX_OUT_OF_RANGE_MSG);
                }
                if (z < 0 || z >= height)
                {
                    throw new ArgumentOutOfRangeException(nameof(z), z, INDEX_OUT_OF_RANGE_MSG);
                }

                if (cache.Swap(new ChunkPosition<T>(x, z, value)) is ChunkPosition<T> chunkPos)
                {
                    Span<byte> buffer = stackalloc byte[TerrainChunk<T>.BUFFER_SIZE];
                    WriteChunk(chunkPos, buffer);
                }
            }
        }

        private Terrain(string path, FileMode mode, ref IBiome<T>[] biomes, int bufferSize)
        {
            cache = new LRUCache<T>(bufferSize);

            this.biomes = biomes;

            stream = new(path, mode, FileAccess.ReadWrite);
        }

        /// <summary>
        /// Initializes a new instance of the Terrain class, creating a new terrain file.
        /// </summary>
        /// <param name="width">The width of the terrain in chunks</param>
        /// <param name="height">The height of the terrain in chunks</param>
        /// <param name="path">The path to the terrain file</param>
        /// <param name="biomes">The biomes to use in the terrain</param>
        /// <param name="bufferSize">The number of chunks to cache (default 64)</param>
        public Terrain(uint width, uint height, string path, ref IBiome<T>[] biomes, int bufferSize = 64) : this(path, FileMode.CreateNew, ref biomes, bufferSize)
        {
            this.width = width;
            this.height = height;

            WriteInt32(width);
            WriteInt32(height);

            headerLength = (uint)stream.Position;

            resolution = width * height;
        }

        /// <summary>
        /// Initializes a new instance of the Terrain class, loading a terrain from an existing file.
        /// </summary>
        /// <param name="path">The path to the terrain file</param>
        /// <param name="biomes">The biomes to use in the terrain</param>
        /// <param name="bufferSize">The number of chunks to cache (default 64)</param>
        public Terrain(string path, ref IBiome<T>[] biomes, int bufferSize = 64) : this(path, FileMode.Open, ref biomes, bufferSize)
        {
            width = ReadInt32();
            height = ReadInt32();

            headerLength = (uint)stream.Position;

            resolution = width * height;
        }

        /// <summary>
        /// Samples the terrain at the given world position using bilinear interpolation.
        /// </summary>
        /// <param name="worldPosition">The world position to sample from.</param>
        /// <returns>The interpolated sample value.</returns>
        public T BilinearSample(Vector2 worldPosition)
        {
            return BilinearSample(Vector2UInt.WorldSpaceToChunkIndices(worldPosition));
        }

        /// <summary>
        /// Samples the terrain at the given chunk index using bilinear interpolation.
        /// </summary>
        /// <param name="chunkIndex">The chunk index to sample from.</param>
        /// <returns>The interpolated sample value.</returns>
        public T BilinearSample(Vector2UInt chunkIndex)
        {
            TerrainChunk<T> chunk = this[chunkIndex.x, chunkIndex.z];
            T finalSample = biomes[chunk.BiomeId].Settings;

            int sampleCount = 1;
            foreach (IBiome<T> sample in GetBilinearSamplesAroundPoint(chunkIndex))
            {
                sampleCount++;
                finalSample.AggregateAdd(sample.Settings);
            }

            finalSample.DivideBy(sampleCount);

            return finalSample;
        }

        private IEnumerable<IBiome<T>> GetBilinearSamplesAroundPoint(Vector2UInt chunkIndex)
        {
            if (chunkIndex.x + 1 < width) // Directly to the right
            {
                yield return biomes[this[chunkIndex.x + 1, chunkIndex.z].BiomeId];
            }
            if (chunkIndex.z + 1 < height) // Directly below
            {
                yield return biomes[this[chunkIndex.x, chunkIndex.z + 1].BiomeId];
            }
            if (chunkIndex.x + 1 < width && chunkIndex.z + 1 < height) // Directly to the right and below
            {
                yield return biomes[this[chunkIndex.x + 1, chunkIndex.z + 1].BiomeId];
            }
            if (chunkIndex.x > 0 && chunkIndex.z + 1 < height) // Directly to the left and below
            {
                yield return biomes[this[chunkIndex.x - 1, chunkIndex.z + 1].BiomeId];
            }
        } 

        /// <summary>
        /// Creates a new streaming handler for the terrain.
        /// </summary>
        /// <param name="radius">The radius of chunks to stream around the player (default 8)</param>
        /// <returns>A new TerrainStreamingHandler instance</returns>
        public TerrainStreamingHandler CreateStreamingHandler(byte radius = 8)
        {
            TerrainStreamingHandler handler = new TerrainStreamingHandler(radius);

            lock (handles) { handles.AddLast(new WeakReference(handler)); }

            if (terrainThread is null || !terrainThread.IsAlive)
            {
                cancellationTokenSource = new CancellationTokenSource();

                terrainThread = new Thread(TerrainThreadLoop);
                terrainThread.Start();
            }

            return handler;
        }

        private void WriteChunk(ChunkPosition<T> chunkPos, Span<byte> buffer)
        {
            if (buffer.Length != TerrainChunk<T>.BUFFER_SIZE)
            {
                throw new ArgumentOutOfRangeException(nameof(buffer), buffer.Length, $"The length of the buffer must be {TerrainChunk<T>.BUFFER_SIZE}.");
            }

            long offset = StreamPositionFromIndices(chunkPos.position.x, chunkPos.position.z);

            chunkPos.chunk.Encode(buffer);

            lock (stream)
            {
                stream.Seek(offset, SeekOrigin.Begin);
                stream.Write(buffer);
            }
        }

        private TerrainChunk<T> ReadChunk(uint x, uint z, Span<byte> buffer)
        {
            if (buffer.Length != TerrainChunk<T>.BUFFER_SIZE)
            {
                throw new ArgumentOutOfRangeException(nameof(buffer), buffer.Length, $"The length of the buffer must be {TerrainChunk<T>.BUFFER_SIZE}.");
            }

            long offset = StreamPositionFromIndices(x, z);

            if (offset + buffer.Length > stream.Length)
            {
                throw new IndexOutOfRangeException(INDEX_OUT_OF_RANGE_MSG);
            }

            lock (handles)
            {
                stream.Seek(offset, SeekOrigin.Begin);
                stream.Read(buffer);
            }

            return TerrainChunk<T>.Decode(buffer, ref biomes);
        }

        private void TerrainThreadLoop()
        {
            if (Thread.CurrentThread != terrainThread)
            {
                throw new Exception($"{nameof(TerrainThreadLoop)} must run on {nameof(terrainThread)}.");
            }

            while (handles.Count > 0 || cancellationTokenSource!.IsCancellationRequested)
            {
                for (LinkedListNode<WeakReference>? node = handles.First; node is not null; node = PurgeDeadAndMoveNext(node))
                {
                    if (node.ValueRef.Target is not TerrainStreamingHandler handler || handler.Radius < 1 || !handler.IsDirty) { continue; }

                    LoadArea(handler);
                    handler.IsDirty = false;
                }

                Thread.Sleep(500);
            }
        }

        private LinkedListNode<WeakReference>? PurgeDeadAndMoveNext(LinkedListNode<WeakReference> node)
        {
            if (node.Next is LinkedListNode<WeakReference> nextNode && !nextNode.ValueRef.IsAlive)
            {
                lock (handles) { handles.Remove(node); }
                return nextNode;
            }

            return null;
        }

        private uint StreamPositionFromIndices(uint x, uint y)
        {
            uint chunkIndex = (x / TerrainChunk<T>.WIDTH) + (y / TerrainChunk<T>.HEIGHT) * (width / TerrainChunk<T>.WIDTH);
            uint offset = headerLength + (chunkIndex * TerrainChunk<T>.BUFFER_SIZE);

            return offset;
        }

        private void WriteInt32(uint number)
        {
            Span<byte> bytes = BitConverter.GetBytes(number);
            lock (stream) { stream.Write(bytes); }
        }

        private uint ReadInt32()
        {
            Span<byte> buffer = stackalloc byte[4];
            lock (stream) { stream.Read(buffer); }
            return BitConverter.ToUInt32(buffer);
        }

        private void LoadArea(TerrainStreamingHandler handle)
        {
            Vector2UInt topLeft = handle.GetTopLeftOfRange();
            Vector2UInt bottomRight = handle.GetBottomRightOfRange();

            Span<byte> buffer = stackalloc byte[TerrainChunk<T>.BUFFER_SIZE];

            uint totalChunks = Vector2UInt.Area(bottomRight - topLeft);
            uint width = bottomRight.x - topLeft.x + 1;
            uint height = bottomRight.z - topLeft.z + 1;

            long offset = StreamPositionFromIndices(topLeft.x, topLeft.z);

            List<ChunkPosition<T>>? evictedChunks = stream.CanWrite ? new() : null;

            lock (stream)
            {
                stream.Seek(offset, SeekOrigin.Begin);

                for (uint i = 0; i < totalChunks; i++)
                {
                    uint x = topLeft.x + (i % width);
                    uint z = topLeft.z + (i / height);

                    if (cache.Get(x, z) is TerrainChunk<T> chunk)
                    {
                        stream.Seek(buffer.Length, SeekOrigin.Current);
                    }
                    else
                    {
                        stream.Read(buffer);
                        chunk = TerrainChunk<T>.Decode(buffer, ref biomes);
                        if (cache.Swap(new ChunkPosition<T>(x, z, chunk)) is ChunkPosition<T> cached) { evictedChunks?.Add(cached); }
                    }

                    ChunkLoaded?.Invoke(chunk, new(x, z));
                }
            }

            if (evictedChunks is null) { return; }

            foreach (var chunk in evictedChunks)
            {
                WriteChunk(chunk, buffer);
            }
        }

        /// <summary>
        /// Disposes and cleans up the terrain instance.
        /// </summary>
        public void Dispose()
        {
            if (terrainThread is not null)
            {
                cancellationTokenSource!.Cancel();
                terrainThread.Join();
            }

            if (stream.CanWrite)
            {
                Span<byte> buffer = stackalloc byte[TerrainChunk<T>.BUFFER_SIZE];
                foreach (ChunkPosition<T> chunkPos in cache) { WriteChunk(chunkPos, buffer); }
            }

            stream.Dispose();
            cache.Dispose();
        }
    }
}
