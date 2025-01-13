using System.Numerics;

namespace TinyTerrain;

/// <summary>
/// Represents a biome with specific settings and a material palette.
/// </summary>
/// <typeparam name="T">The type of the biome settings struct.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="Biome{T}"/> class.
/// </remarks>
/// <param name="settings">The settings of the biome.</param>
public class Biome<T>(T settings) where T : IDivisionOperators<T, int, T>, IAdditionOperators<T, T, T>
{
    /// <summary>
    /// The material palette associated with the biome.
    /// </summary>
    public MaterialPalette palette;

    /// <summary>
    /// The settings of the biome.
    /// </summary>
    public T settings = settings;

    /// <inheritdoc cref="Biome{T}(T)"/>"
    /// <param name="palette"></param>
    public Biome(T settings, MaterialPalette palette) : this(settings)
    {
        this.palette = palette;
    }
}