using System.Numerics;

namespace TinyTerrain;

// 11000000 - Material ID (from palette)
// 00111111 - Height

/// <summary>
/// Represents a single vertex in the terrain.
/// </summary>
/// <typeparam name="T">The biome type</typeparam>
public struct TerrainVertex<T>(short height, byte materialId) where T : struct, IDivisionOperators<T, int, T>, IAdditionOperators<T, T, T>
{
    /// <summary>
    /// The height of the vertex.
    /// </summary>
    public short height = height;

    /// <summary>
    /// The material ID.
    /// </summary>
    /// <value>The material ID.</value>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is less than 0 or greater than 3.</exception>
    public byte materialId = materialId;

    /// <summary>
    /// Decodes a compressed vertex byte into a <see cref="TerrainVertex"/> instance.
    /// </summary>
    /// <param name="vertex">The compressed vertex data.</param>
    /// <param name="chunk">The terrain chunk the vertex belongs to.</param>
    /// <returns>The decoded <see cref="TerrainVertex"/> instance.</returns>
    public static TerrainVertex<T> Decode(byte vertex, TerrainChunk<T> chunk)
    {
        int materialIndex = (vertex >> 6) & 0x03;
        byte materialId = (byte)chunk.palette[materialIndex];

        int heightOffset = vertex & 0x3F;
        if ((heightOffset & 0x20) != 0)
        {
            heightOffset -= 64;
        }

        short finalHeight = (short)(chunk.BaseHeight * 4 + heightOffset);

        return new(finalHeight, materialId);
    }

    /// <summary>
    /// Encodes the vertex data into a compressed byte.
    /// </summary>
    /// <param name="chunk">The terrain chunk the vertex belongs to.</param>
    /// <returns>The encoded vertex data as a byte.</returns>
    public readonly byte Encode(TerrainChunk<T> chunk)
    {
        int heightOffset = height - (chunk.BaseHeight * 4);

        if (heightOffset < 0)
        {
            heightOffset += 64;
        }

        heightOffset &= 0x3F;

        int materialIndex = chunk.palette.IndexOf(materialId);

        return (byte)((materialIndex << 6) | heightOffset);
    }
}
