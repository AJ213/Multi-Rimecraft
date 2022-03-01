using System;
using System.Collections.Concurrent;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
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

        public WorldData(string seed)
        {
            OpenSimplexNoise simplexNoise = new OpenSimplexNoise(GetSeed(seed));
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

            Lode[] lodes = new Lode[] { new Lode(), new Lode(), new Lode(), new Lode(), new Lode() };
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

            lodes[3].blockID = 4;
            lodes[3].minHeight = -500;
            lodes[3].maxHeight = -5;
            lodes[3].scale = 0.2f;
            lodes[3].threshold = 0.6f;
            lodes[3].noiseOffset = 2623;

            lodes[4].blockID = 0;
            lodes[4].minHeight = -500;
            lodes[4].maxHeight = 400;
            lodes[4].scale = 0.03f;
            lodes[4].threshold = 0.6f;
            lodes[4].noiseOffset = 1923;

            biomes[0].lodes = lodes;
        }

        private static int GetSeed(string seed)
        {
            using var algo = SHA1.Create();
            var hash = BitConverter.ToInt32(algo.ComputeHash(Encoding.UTF8.GetBytes(seed)));
            return hash;
        }

        public ChunkData RequestChunk(Vector3 coord)
        {
            if (chunks.ContainsKey(coord))
            {
                return chunks[coord];
            }
            else
            {
                Console.WriteLine("Request a chunk that doesn't exist");
                return null;
            }
        }

        public void SetVoxel(int fromClient, Vector3 globalPosition, ushort value)
        {
            ChunkData chunk = RequestChunk(WorldHelper.GetChunkCoordFromPosition(globalPosition));
            Vector3 localPosition = WorldHelper.GetVoxelLocalPositionInChunk(globalPosition);

            ServerSend.ModifiedVoxel(fromClient, globalPosition, value);
            chunk.ModifyVoxel(localPosition, value);
        }

        public void LoadChunk(Vector3 coord, int sendTarget)
        {
            if (chunks.ContainsKey(coord))
            {
                return;
            }

            chunks.TryAdd(coord, new ChunkData(coord));

            ChunkData.Populate(chunks[coord]);
            ServerSend.SendChunk(sendTarget, chunks[coord]);
        }
    }
}