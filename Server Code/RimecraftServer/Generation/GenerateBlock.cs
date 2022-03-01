using System;
using System.Collections.Generic;
using System.Numerics;
using Noise;

namespace RimecraftServer
{
    internal class GenerateBlock
    {
        public static ushort GenerateFlatGround(Vector3 g)
        {
            if (g.Y % 15 == 0)
            {
                return 1;
            }
            return 0;
        }

        public static ushort SamplePosition(Vector3 globalPosition, BiomeAttributes[] biomes, NoiseGen noise)
        {
            // Set biome to the one with the strongest weight.
            int terrainHeight = 0;
            BiomeAttributes mainBiome = biomes[0];
            for (int i = 0; i < biomes.Length; i++)
            {
                terrainHeight += (int)Math.Floor(biomes[i].terrainHeight * noise.Octave2DSimplex(globalPosition.X, globalPosition.Z, 2 * biomes[i].offset, biomes[i].terrainScale, biomes[i].octaves, biomes[i].persistence));
            }
            terrainHeight /= biomes.Length;

            ushort voxelID = 0;

            SurfaceBlocks(ref voxelID, globalPosition, mainBiome, terrainHeight, 2, 15);
            LodeGeneration(ref voxelID, globalPosition, mainBiome, noise);

            if (globalPosition.Y == 30) { voxelID = 1; }

            return voxelID;
        }

        private static void SurfaceBlocks(ref ushort voxelID, Vector3 globalPosition, BiomeAttributes biome, int terrainHeight, int depthMultiplier, int cavernHeight)
        {
            int cavernDepthAtPosition = (int)(Math.Pow(4, Math.Ceiling(Math.Log10(Math.Abs(globalPosition.Y)) / Math.Log10(4))) / depthMultiplier);
            int cavernTerrainHeight = terrainHeight - cavernDepthAtPosition;
            int heightMultiplier = (int)Math.Max(1, Math.Log2(Math.Abs(globalPosition.Y)) - 3);
            if (globalPosition.Y == terrainHeight)
            {
                voxelID = biome.surfaceBlock;
            }
            else if (globalPosition.Y < terrainHeight && globalPosition.Y > terrainHeight - 4)
            {
                voxelID = biome.subSurfaceBlock;
            }
            else if (globalPosition.Y > terrainHeight)
            {
                voxelID = 0;
            }
            else if (globalPosition.Y > cavernTerrainHeight && globalPosition.Y < cavernTerrainHeight + (cavernHeight * heightMultiplier))
            {
                voxelID = 0;
            }
            else
            {
                voxelID = 3;
            }
        }

        private static void LodeGeneration(ref ushort voxelID, Vector3 globalPosition, BiomeAttributes biome, NoiseGen noise)
        {
            if (voxelID == 3 || voxelID == 1)
            {
                foreach (Lode lode in biome.lodes)
                {
                    if (globalPosition.Y > lode.minHeight && globalPosition.Y < lode.maxHeight)
                    {
                        if (noise.Get3DSimplex(globalPosition.X, globalPosition.Y, globalPosition.Z, lode.noiseOffset, lode.scale) > lode.threshold)
                        {
                            voxelID = lode.blockID;
                        }
                    }
                }
            }
        }
    }
}