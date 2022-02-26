using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class WorldHelper
{
    public static int3 GetChunkCoordFromPosition(float3 globalPosition)
    {
        int x = Mathf.FloorToInt(globalPosition.x / Constants.CHUNK_SIZE);
        int y = Mathf.FloorToInt(globalPosition.y / Constants.CHUNK_SIZE);
        int z = Mathf.FloorToInt(globalPosition.z / Constants.CHUNK_SIZE);
        return new int3(x, y, z);
    }

    public static int3 GetVoxelLocalPositionInChunk(float3 globalPosition)
    {
        return new int3(Mod(Mathf.FloorToInt(globalPosition.x), Constants.CHUNK_SIZE),
                        Mod(Mathf.FloorToInt(globalPosition.y), Constants.CHUNK_SIZE),
                        Mod(Mathf.FloorToInt(globalPosition.z), Constants.CHUNK_SIZE));
    }

    public static int3 GetVoxelGlobalPositionFromChunk(float3 localPosition, int3 coord)
    {
        return new int3(Mathf.FloorToInt(localPosition.x) + (Constants.CHUNK_SIZE * coord.x),
                        Mathf.FloorToInt(localPosition.y) + (Constants.CHUNK_SIZE * coord.y),
                        Mathf.FloorToInt(localPosition.z) + (Constants.CHUNK_SIZE * coord.z));
    }

    public static bool IsVoxelGlobalPositionInChunk(float3 globalPosition, int3 coord)
    {
        return ((float3)GetVoxelGlobalPositionFromChunk(GetVoxelLocalPositionInChunk(globalPosition), coord)).Equals(globalPosition);
    }

    public static bool IsInRange(int value, int length)
    {
        return (value >= 0 && value < length);
    }

    public static bool IsInRange(int3 value, int length)
    {
        return (IsInRange(value.x, length) && IsInRange(value.y, length) && IsInRange(value.z, length));
    }

    public static int Mod(int a, int n)
    {
        return ((a % n) + n) % n;
    }
}