# TinyTerrain

TinyTerrain is a C# library designed for the efficient storage and streaming of voxel terrain data. It prioritizes compact storage and fast retrieval, making it suitable for games and applications where optimized terrain management is crucial.

## Features

* **Compact Storage:** Terrain data is compressed for minimal storage footprint.
* **Fast Access:** Chunks are streamed from disk using an LRU cache for quick access.
* **Customizable Biomes:** Define unique biomes with individual settings and material palettes.
* **Smooth Transitions Between Biomes:** Apply bilinear interpolation to blend between biomes.
* **Asynchronous Streaming:** Chunks are streamed in a separate thread to avoid impacting performance.

## Usage

1. **Installation:** Add the TinyTerrain library to your project.
2. **Define Biomes:** Create biomes that implement the `IBiome` interface.
3. **Create Terrain:** Initialize a `Terrain` instance, specifying dimensions and biomes.
4. **Access Chunks:** Access terrain chunks using the `Terrain[x, z]` indexer.
5. **Stream Chunks:** Create a `TerrainStreamingHandler` to manage chunk streaming around a position.

## Example

```C#
// Define biomes
IBiome[] biomes = new IBiome[2];
biomes[0] = new ForestBiome();
biomes[1] = new DesertBiome();

// Create a new terrain (16x16 chunks)
Terrain terrain = new Terrain(16, 16, "terrain.bin", biomes);

// Access a chunk
TerrainChunk chunk = terrain[0, 0];

// Modify the chunk
chunk.BiomeId = 1;
terrain[0, 0] = chunk;

// Sample the terrain
float terrainHeight = terrain.BilinearSample(new Vector2(5.0f, 2.5f));

// Create a streaming handler
TerrainStreamingHandler handler = terrain.CreateStreamingHandler();

// Update the handler's position
handler.Position = new Vector2(10.0f, 5.0f);

// Dispose of the terrain when finished
terrain.Dispose();