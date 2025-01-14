namespace TinyTerrain
{
    internal struct ChunkPosition<T> where T : BiomeSettings
    {
        public Vector2UInt position;
        public TerrainChunk<T> chunk;

        public ChunkPosition(uint x, uint y, TerrainChunk<T> chunk)
        {
            position = new Vector2UInt(x, y);
            this.chunk = chunk;
        }

        public static ChunkPosition<T> Decode(ReadOnlySpan<byte> chunkAndVertexData, ref IBiome<T>[] biomes, uint x, uint z)
        {
            return new ChunkPosition<T>(x, z, TerrainChunk<T>.Decode(chunkAndVertexData, ref biomes));
        }
    }
}
