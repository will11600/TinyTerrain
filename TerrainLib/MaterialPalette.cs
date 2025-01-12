using System.Collections;

namespace TinyTerrain;

/// <summary>
/// Represents a palette of 4 material IDs used in a terrain chunk.
/// </summary>
public struct MaterialPalette : IEnumerable<int>
{
    private ushort map;

    /// <summary>
    /// Gets the material ID at the specified index in the palette.
    /// </summary>
    /// <param name="index">The index of the material ID (0-3).</param>
    /// <returns>The material ID at the given index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the index is not between 0 and 3.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the value is not between 0 and 15.</exception>
    public int this[int index]
    {
        readonly get => (map >> (index * 4)) & 0xF;
        set
        {
            if (index is < 0 or > 3)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, "Index must be between 0 and 3.");
            }

            if (value is < 0 or > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be between 0 and 15.");
            }

            map &= (ushort)~(0xF << (index * 4));
            map |= (ushort)(value << (index * 4));
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaterialPalette"/> struct.
    /// </summary>
    /// <param name="indexes">An array of 4 material IDs.</param>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="indexes"/> array does not contain exactly 4 elements.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if any of the material IDs in <paramref name="indexes"/> is not between 0 and 15.</exception>
    public MaterialPalette(ReadOnlySpan<byte> indexes)
    {
        if (indexes.Length != 4)
        {
            throw new ArgumentException("MaterialPalette requires exactly 4 indexes.", nameof(indexes));
        }

        map = 0;
        for (int i = 0; i < 4; i++)
        {
            if (indexes[i] is < 0 or > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(indexes), indexes[i], "Index must be between 0 and 15.");
            }
            map |= (ushort)(indexes[i] << (i * 4));
        }
    }

    /// <summary>
    /// Gets the index of the specified material ID in the palette.
    /// </summary>
    /// <param name="materialId">The material ID to find.</param>
    /// <returns>The index of the material ID in the palette, or -1 if not found.</returns>
    public readonly int IndexOf(int materialId)
    {
        for (int i = 0; i < 4; i++)
        {
            if (this[i] == materialId) { return i; }
        }
        return -1;
    }

    /// <inheritdoc />
    public readonly IEnumerator<int> GetEnumerator()
    {
        for (int i = 0; i < 4; i++)
        {
            yield return this[i];
        }
    }

    /// <inheritdoc />
    readonly IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
