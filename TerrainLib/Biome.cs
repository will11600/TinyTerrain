using System.Numerics;

namespace TinyTerrain;

/// <summary>
/// Represents a biome with specific settings and a material palette.
/// </summary>
/// <typeparam name="T">The type of the biome settings struct.</typeparam>
public class Biome<T> where T : struct, IDivisionOperators<T, int, T>, IAdditionOperators<T, T, T>
{
    /// <summary>
    /// The material palette associated with the biome.
    /// </summary>
    public MaterialPalette palette;

    /// <summary>
    /// The settings of the biome.
    /// </summary>
    public T settings;
}