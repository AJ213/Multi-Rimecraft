using System;
using System.Collections.Concurrent;
using System.Numerics;
using Noise;

namespace RimecraftServer
{
    public class WorldData
    {
        private NoiseGen noiseGen;
        public NoiseGen NoiseGeneration => noiseGen;

        private ConcurrentDictionary<Vector3, ChunkData> chunks = new ConcurrentDictionary<Vector3, ChunkData>();

        public ConcurrentDictionary<Vector3, ChunkData> Chunks => chunks;

        public BiomeAttributes[] biomes;

        public WorldData(int seed)
        {
            OpenSimplexNoise simplexNoise = new OpenSimplexNoise(seed);
            noiseGen = new NoiseGen(simplexNoise);

            biomes = new BiomeAttributes[] { new BiomeAttributes() };
            biomes[0].offset = -510;
            biomes[0].scale = 0.8f;
            biomes[0].terrainHeight = 15;
            biomes[0].terrainScale = 3f;
            biomes[0].surfaceBlock = 1;
            biomes[0].subSurfaceBlock = 1;

            biomes[0].maxHeight = 30;
            biomes[0].minHeight = 5;
            biomes[0].octaves = 3;
            biomes[0].persistence = 0.45f;

            Lode[] lodes = new Lode[] { new Lode(), new Lode(), new Lode(), new Lode() };
            lodes[0].blockID = 2;
            lodes[0].minHeight = -500;
            lodes[0].maxHeight = 400;
            lodes[0].scale = 0.2f;
            lodes[0].threshold = 0.5f;
            lodes[0].noiseOffset = 123;

            lodes[1].blockID = 1;
            lodes[1].minHeight = -500;
            lodes[1].maxHeight = 400;
            lodes[1].scale = 0.2f;
            lodes[1].threshold = 0.5f;
            lodes[1].noiseOffset = 623;

            lodes[2].blockID = 5;
            lodes[2].minHeight = -500;
            lodes[2].maxHeight = -5;
            lodes[2].scale = 0.2f;
            lodes[2].threshold = 0.6f;
            lodes[2].noiseOffset = 1623;

            lodes[3].blockID = 0;
            lodes[3].minHeight = -500;
            lodes[3].maxHeight = 400;
            lodes[3].scale = 0.03f;
            lodes[3].threshold = 0.6f;
            lodes[3].noiseOffset = 1923;

            biomes[0].lodes = lodes;
        }

        public ChunkData RequestChunk(Vector3 coord, bool create)
        {
            if (chunks.ContainsKey(coord))
            {
                return chunks[coord];
            }

            if (create)
            {
                LoadChunk(coord);
                return chunks[coord];
            }
            else
            {
                return null;
            }
        }

        public void SetVoxel(int fromClient, Vector3 globalPosition, ushort value)
        {
            ChunkData chunk = RequestChunk(WorldHelper.GetChunkCoordFromPosition(globalPosition), false);
            Vector3 localPosition = WorldHelper.GetVoxelLocalPositionInChunk(globalPosition);

            ServerSend.ModifiedVoxel(fromClient, globalPosition, value);
            chunk.ModifyVoxel(localPosition, value);
        }

        public void LoadChunk(Vector3 coord)
        {
            if (chunks.ContainsKey(coord))
            {
                return;
            }

            chunks.TryAdd(coord, new ChunkData(coord));

            ChunkData.Populate(chunks[coord]);
        }
    }
}