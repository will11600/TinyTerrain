using System.Numerics;

namespace TinyTerrain;

// 11111000 - Height
// 00000111 - Biome ID

/// <summary>
/// Represents a chunk of terrain data.
/// </summary>
public struct TerrainChunk<T> where T : struct, IDivisionOperators<T, int, T>, IAdditionOperators<T, T, T>
{
    internal const byte WIDTH = 8;
    internal const byte HEIGHT = 8;
    internal const byte RESOLUTION = WIDTH * HEIGHT;
    internal const byte BUFFER_SIZE = RESOLUTION + 1;

    private readonly TerrainVertex<T>[] _vertices;

    /// <summary>
    /// Gets the number of vertices in the chunk.
    /// </summary>
    public readonly int Length => _vertices.Length;

    private byte _biomeId;
    /// <summary>
    /// Gets or sets the biome ID.
    /// </summary>
    /// <value>The biome ID.</value>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is less than 0 or greater than 7.</exception>
    public int BiomeId
    {
        readonly get => _biomeId;
        set
        {
            if (value is < 0 or > 7)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "The biome ID must be between 0 and 15.");
            }

            _biomeId = (byte)value;
        }
    }

    private byte _baseHeight;
    /// <summary>
    /// Gets or sets the base height.
    /// </summary>
    /// <value>The base height.</value>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is less than 0 or greater than 31.</exception>
    public int BaseHeight
    {
        readonly get => _baseHeight;
        set
        {
            if (value is < 0 or > 31)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "The biome ID must be between 0 and 31.");
            }

            _baseHeight = (byte)value;
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
        get => _vertices[x * WIDTH + y];
        set => _vertices[x * WIDTH + y] = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TerrainChunk"/> struct with an empty vertices array.
    /// </summary>
    public TerrainChunk()
    {
        _vertices = new TerrainVertex<T>[RESOLUTION];
    }

    private TerrainChunk(ref TerrainVertex<T>[] vertices)
    {
        _vertices = vertices;
    }

    /// <summary>
    /// Decodes the compressed chunk data and vertex data into a <see cref="TerrainChunk"/> instance.
    /// </summary>
    /// <param name="chunkData">The compressed chunk data.</param>
    /// <param name="chunkAndVertexData">A span of compressed vertex data.</param>
    /// <param name="biome">The biome for the chunk.</param>
    /// <returns>The decoded <see cref="TerrainChunk"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown if the length of the <paramref name="chunkAndVertexData"/> span does not match the expected number of vertices in the chunk.</exception>
    public static TerrainChunk<T> Decode(ReadOnlySpan<byte> chunkAndVertexData, ref IBiome<T>[] biomes)
    {
        if (chunkAndVertexData.Length != BUFFER_SIZE)
        {
            throw new ArgumentException($"The length of the vertices array must be {BUFFER_SIZE}.", nameof(chunkAndVertexData));
        }

        TerrainVertex<T>[] terrainVertices = GC.AllocateUninitializedArray<TerrainVertex<T>>(RESOLUTION);

        byte chunkData = chunkAndVertexData[0];

        byte biomeId = (byte)(chunkData & 0x07);

        TerrainChunk<T> chunk = new(ref terrainVertices)
        {
            palette = biomes[biomeId].Palette,
            _baseHeight = (byte)((chunkData >> 3) & 0x1F),
            _biomeId = biomeId
        };

        for (int i = 1; i < chunk.Length; i++)
        {
            terrainVertices[i] = TerrainVertex<T>.Decode(chunkAndVertexData[i], chunk);
        }

        return chunk;
    }

    /// <summary>
    /// Encodes the terrain chunk data into a sequence of bytes and writes them to the buffer.
    /// </summary>
    /// <param name="buffer">The buffer to write the encoded data to.</param>
    /// <param name="biomeId">The ID of the biome.</param>
    /// <exception cref="ArgumentException">Thrown if the buffer size is not equal to <see cref="BUFFER_SIZE"/>.</exception>
    public readonly void Encode(Span<byte> buffer)
    {
        if (buffer.Length != BUFFER_SIZE)
        {
            throw new ArgumentException($"The buffer size must be {BUFFER_SIZE}.", nameof(buffer));
        }

        buffer[0] = (byte)((_baseHeight << 3) | _biomeId);

        for (int i = 1; i < buffer.Length; i++)
        {
            buffer[i] = _vertices[i].Encode(this);
        }
    }
}
