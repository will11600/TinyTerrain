using System.Numerics;

namespace TinyTerrain;

/// <summary>
/// Represents a biome with specific settings and a material palette.
/// </summary>
/// <typeparam name="T">The type of the biome settings struct.</typeparam>
public interface IBiome<T> where T : IDivisionOperators<T, int, T>, IAdditionOperators<T, T, T>
{
    /// <summary>
    /// Gets the material palette associated with the biome.
    /// </summary>
    public MaterialPalette Palette { get; }

    /// <summary>
    /// Gets the settings of the biome.
    /// </summary>
    public T Settings { get; }
}