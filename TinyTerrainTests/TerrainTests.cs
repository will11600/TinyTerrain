namespace TinyTerrain.Tests;

[TestClass]
public class TerrainTests
{
    private const byte CHUNK_WIDTH = 8;
    private const byte CHUNK_HEIGHT = 8;
    private const byte CHUNK_RESOLUTION = CHUNK_WIDTH * CHUNK_HEIGHT;
    private const byte CHUNK_BUFFER_SIZE = CHUNK_RESOLUTION + 1;

    // Test Biome implementation
    private readonly struct TestBiome(int settings, MaterialPalette palette) : IBiome<int>
    {
        public int Settings { get; } = settings;
        public MaterialPalette Palette { get; } = palette;
    }

    private IBiome<int>[] biomes = [new TestBiome(0, new MaterialPalette([1, 2, 3, 4]))];

    [TestMethod]
    public void TerrainChunk_EncodeDecode_ShouldMatch()
    {
        // Arrange
        TerrainChunk<int> originalChunk = new()
        {
            BiomeId = 0,
            BaseHeight = 15,
            palette = new MaterialPalette([1, 2, 3, 4])
        };
        for (int i = 0; i < CHUNK_WIDTH; i++)
        {
            for (int j = 0; j < CHUNK_HEIGHT; j++)
            {
                originalChunk[i, j] = new TerrainVertex<int>(100, 2);
            }
        }

        // Act
        Span<byte> buffer = stackalloc byte[CHUNK_BUFFER_SIZE];
        originalChunk.Encode(buffer);
        TerrainChunk<int> decodedChunk = TerrainChunk<int>.Decode(buffer, ref biomes);

        // Assert
        Assert.AreEqual(originalChunk.BiomeId, decodedChunk.BiomeId);
        Assert.AreEqual(originalChunk.BaseHeight, decodedChunk.BaseHeight);
        Assert.AreEqual(originalChunk.palette, decodedChunk.palette);
        for (int i = 0; i < CHUNK_WIDTH; i++)
        {
            for (int j = 0; j < CHUNK_HEIGHT; j++)
            {
                Assert.AreEqual(originalChunk[i, j].height, decodedChunk[i, j].height);
                Assert.AreEqual(originalChunk[i, j].materialId, decodedChunk[i, j].materialId);
            }
        }
    }

    [TestMethod]
    public void TerrainVertex_EncodeDecode_ShouldMatch()
    {
        TerrainChunk<int> chunk = new()
        {
            BiomeId = 0,
            BaseHeight = 15,
            palette = new MaterialPalette([1, 2, 3, 4])
        };
        TerrainVertex<int> originalVertex = new(100, 2);

        // Act
        byte encoded = originalVertex.Encode(chunk);
        TerrainVertex<int> decodedVertex = TerrainVertex<int>.Decode(encoded, chunk);

        // Assert
        Assert.AreEqual(originalVertex.height, decodedVertex.height);
        Assert.AreEqual(originalVertex.materialId, decodedVertex.materialId);
    }

    [TestMethod]
    public void MaterialPalette_Indexer_ShouldSetAndGetValues()
    {
        // Arrange
        MaterialPalette palette = new();

        // Act
        palette[0] = 1;
        palette[1] = 2;
        palette[2] = 3;
        palette[3] = 4;

        // Assert
        Assert.AreEqual(1, palette[0]);
        Assert.AreEqual(2, palette[1]);
        Assert.AreEqual(3, palette[2]);
        Assert.AreEqual(4, palette[3]);
    }

    [TestMethod]
    public void MaterialPalette_Constructor_ShouldSetValues()
    {
        // Arrange
        byte[] indexes = [1, 2, 3, 4];

        // Act
        MaterialPalette palette = new(indexes);

        // Assert
        Assert.AreEqual(1, palette[0]);
        Assert.AreEqual(2, palette[1]);
        Assert.AreEqual(3, palette[2]);
        Assert.AreEqual(4, palette[3]);
    }

    [TestMethod]
    public void MaterialPalette_IndexOf_ShouldReturnCorrectIndex()
    {
        // Arrange
        MaterialPalette palette = new([1, 2, 3, 4]);

        // Act
        int index = palette.IndexOf(3);

        // Assert
        Assert.AreEqual(2, index);
    }

    [TestMethod]
    public void Vector2UInt_Area_ShouldReturnCorrectArea()
    {
        // Arrange
        Vector2UInt vector = new(5, 10);

        // Act
        uint area = Vector2UInt.Area(vector);

        // Assert
        Assert.AreEqual(50u, area);
    }

    [TestMethod]
    public void Vector2UInt_Operators_ShouldWorkCorrectly()
    {
        // Arrange
        Vector2UInt v1 = new(5, 10);
        Vector2UInt v2 = new(3, 7);

        // Act & Assert
        Assert.AreEqual(new Vector2UInt(8, 17), v1 + v2);
        Assert.AreEqual(new Vector2UInt(2, 3), v1 - v2);
        Assert.AreEqual(new Vector2UInt(1, 1), v1 / 5);
        Assert.AreEqual(new Vector2UInt(15, 70), v1 * v2);
        Assert.IsTrue(v1 == new Vector2UInt(5, 10));
        Assert.IsTrue(v1 != v2);
        Assert.IsTrue(v1 >= v2);
        Assert.IsTrue(v2 <= v1);
        Assert.IsTrue(v1 > v2);
        Assert.IsTrue(v2 < v1);
    }
}