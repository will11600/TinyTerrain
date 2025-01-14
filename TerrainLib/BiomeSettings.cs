namespace TinyTerrain
{
    /// <summary>
    /// Base class for settings that can be aggregated and divided.
    /// </summary>
    public abstract class BiomeSettings
    {
        /// <summary>
        /// Add the settings of another biome to this one.
        /// </summary>
        /// <param name="settings">The settings to add.</param>
        public abstract void AggregateAdd(BiomeSettings settings);

        /// <summary>
        /// Divide the settings by a given count.
        /// </summary>
        /// <param name="count">The count to divide by.</param>
        public abstract void DivideBy(int count);
    }
}
