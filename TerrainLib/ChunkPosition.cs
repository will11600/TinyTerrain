using System.Numerics;

namespace TinyTerrain;

internal struct ChunkPosition<T>(uint x, uint y, TerrainChunk<T> chunk) where T : IDivisionOperators<T, int, T>, IAdditionOperators<T, T, T>
{
    public Vector2UInt position = new(x, y);
    public TerrainChunk<T> chunk = chunk;

    public static ChunkPosition<T> Decode(ReadOnlySpan<byte> chunkAndVertexData, ref IBiome<T>[] biomes, uint x, uint z)
    {
        return new(x, z, TerrainChunk<T>.Decode(chunkAndVertexData, ref biomes));
    }
}
