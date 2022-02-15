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

        public WorldData(int seed)
        {
            OpenSimplexNoise simplexNoise = new OpenSimplexNoise(seed);
            noiseGen = new NoiseGen(simplexNoise);
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