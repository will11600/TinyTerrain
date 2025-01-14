using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace TinyTerrain
{
    // 11111000 - Height
    // 00000111 - Biome ID

    /// <summary>
    /// Represents a chunk of terrain data.
    /// </summary>
    public struct TerrainChunk<T> where T : BiomeSettings
    {
        /// <summary>
        /// The width of the chunk.
        /// </summary>
        public const byte WIDTH = 8;

        /// <summary>
        /// The height of the chunk.
        /// </summary>
        public const byte HEIGHT = 8;

        /// <summary>
        /// The total number of vertices in the chunk.
        /// </summary>
        public const byte RESOLUTION = WIDTH * HEIGHT;

        internal const byte BUFFER_SIZE = RESOLUTION + 1;

        private readonly TerrainVertex<T>[] vertices;

        /// <summary>
        /// Gets the number of vertices in the chunk.
        /// </summary>
        public readonly int Length => vertices.Length;

        private byte biomeId;
        /// <summary>
        /// Gets or sets the biome ID.
        /// </summary>
        /// <value>The biome ID.</value>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is less than 0 or greater than 7.</exception>
        public int BiomeId
        {
            readonly get => biomeId;
            set
            {
                if (value < 0 || value > 7)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The biome ID must be between 0 and 15.");
                }

                biomeId = (byte)value;
            }
        }

        private byte baseHeight;
        /// <summary>
        /// Gets or sets the base height.
        /// </summary>
        /// <value>The base height.</value>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is less than 0 or greater than 31.</exception>
        public int BaseHeight
        {
            readonly get => baseHeight;
            set
            {
                if (value < 0 || value > 31)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The biome ID must be between 0 and 31.");
                }

                baseHeight = (byte)value;
            }
        }

        /// <summary>
        /// The material palette used by the chunk.
        /// </summary>
        public MaterialPalette palette;

        /// <summary>
        /// Gets a reference to the vertex at the specified coordinates within the chunk.
        /// </summary>
        /// <param name="x">The x-coordinate of the vertex.</param>
        /// <param name="y">The y-coordinate of the vertex.</param>
        /// <returns>A reference to the vertex.</returns>
        public readonly TerrainVertex<T> this[int x, int y]
        {
            get => vertices[x * WIDTH + y];
            set => vertices[x * WIDTH + y] = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TerrainChunk"/> struct.
        /// </summary>
        /// <param name="palette">The material palette used by the chunk.</param>
        /// <param name="baseHeight">The base height of the chunk.</param>
        /// <param name="biomeId">The biome ID of the chunk.</param>
        public TerrainChunk(MaterialPalette palette, byte baseHeight, byte biomeId)
        {
            this.palette = palette;
            this.baseHeight = baseHeight;
            this.biomeId = biomeId;
            #if NET8_0_OR_GREATER
            vertices = GC.AllocateUninitializedArray<TerrainVertex<T>>(RESOLUTION);
            #else
            vertices = new TerrainVertex<T>[RESOLUTION];
            #endif
        }

        /// <summary>
        /// Decodes the compressed chunk data and vertex data into a <see cref="TerrainChunk"/> instance.
        /// </summary>
        /// <param name="chunkAndVertexData">A span of compressed vertex data.</param>
        /// <param name="biomes">The biome for the chunk.</param>
        /// <returns>The decoded <see cref="TerrainChunk"/> instance.</returns>
        /// <exception cref="ArgumentException">Thrown if the length of the <paramref name="chunkAndVertexData"/> span does not match the expected number of vertices in the chunk.</exception>
        public static TerrainChunk<T> Decode(ReadOnlySpan<byte> chunkAndVertexData, ref IBiome<T>[] biomes)
        {
            if (chunkAndVertexData.Length != BUFFER_SIZE)
            {
                throw new ArgumentException($"The length of the vertices array must be {BUFFER_SIZE}.", nameof(chunkAndVertexData));
            }

            byte chunkData = chunkAndVertexData[0];
            byte baseHeight = (byte)((chunkData >> 3) & 0x1F);
            byte biomeId = (byte)(chunkData & 0x07);
            MaterialPalette palette = biomes[biomeId].Palette;

            TerrainChunk<T> chunk = new TerrainChunk<T>(palette, baseHeight, biomeId);

            for (int i = 1; i < RESOLUTION; i++)
            {
                chunk.vertices[i] = TerrainVertex<T>.Decode(chunkAndVertexData[i], chunk);
            }

            return chunk;
        }

        /// <summary>
        /// Encodes the terrain chunk data into a sequence of bytes and writes them to the buffer.
        /// </summary>
        /// <param name="buffer">The buffer to write the encoded data to.</param>
        /// <exception cref="ArgumentException">Thrown if the buffer size is not equal to <see cref="BUFFER_SIZE"/>.</exception>
        public readonly void Encode(Span<byte> buffer)
        {
            if (buffer.Length != BUFFER_SIZE)
            {
                throw new ArgumentException($"The buffer size must be {BUFFER_SIZE}.", nameof(buffer));
            }

            buffer[0] = (byte)((baseHeight << 3) | biomeId);

            for (int i = 1; i < buffer.Length; i++)
            {
                buffer[i] = vertices[i].Encode(this);
            }
        }
    }
}
