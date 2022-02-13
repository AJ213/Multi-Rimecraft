using UnityEngine;
using Unity.Mathematics;

public static class GenerateBlock
{
    public static ushort SamplePosition(int3 globalPosition, BiomeAttributes[] biomes)
    {
        // Set biome to the one with the strongest weight.
        int terrainHeight = 0;
        BiomeAttributes mainBiome = biomes[0];
        for (int i = 0; i < biomes.Length; i++)
        {
            terrainHeight += Mathf.FloorToInt(biomes[i].terrainHeight * Noise.Get2DSimplex(new Vector2(globalPosition.x, globalPosition.z), 2 * biomes[i].offset, biomes[i].terrainScale));
        }
        terrainHeight /= biomes.Length;

        ushort voxelID = 0;

        SurfaceBlocks(ref voxelID, globalPosition, mainBiome, terrainHeight, 2, 15);
        LodeGeneration(ref voxelID, globalPosition, mainBiome);

        return voxelID;
    }

    private static void SurfaceBlocks(ref ushort voxelID, int3 globalPosition, BiomeAttributes biome, int terrainHeight, int depthMultiplier, int cavernHeight)
    {
        int cavernDepthAtPosition = (int)(math.pow(4, math.ceil(math.log10(math.abs(globalPosition.y)) / math.log10(4))) / depthMultiplier);
        int cavernTerrainHeight = terrainHeight - cavernDepthAtPosition;
        int heightMultiplier = (int)math.max(1, math.log2(math.abs(globalPosition.y)) - 3);
        if (globalPosition.y == terrainHeight)
        {
            voxelID = biome.surfaceBlock;
        }
        else if (globalPosition.y < terrainHeight && globalPosition.y > terrainHeight - 4)
        {
            voxelID = biome.subSurfaceBlock;
        }
        else if (globalPosition.y > terrainHeight)
        {
            voxelID = 0;
        }
        else if (globalPosition.y > cavernTerrainHeight && globalPosition.y < cavernTerrainHeight + (cavernHeight * heightMultiplier))
        {
            voxelID = 0;
        }
        else
        {
            voxelID = 3;
        }
    }

    private static void LodeGeneration(ref ushort voxelID, int3 globalPosition, BiomeAttributes biome)
    {
        if (voxelID == 3 || voxelID == 1)
        {
            foreach (Lode lode in biome.lodes)
            {
                if (globalPosition.y > lode.minHeight && globalPosition.y < lode.maxHeight)
                {
                    if (Noise.Get3DSimplex(globalPosition, lode.noiseOffset, lode.scale) > lode.threshold)
                    {
                        voxelID = lode.blockID;
                    }
                }
            }
        }
    }
}