using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class WorldHelper
{
    public static int3 GetChunkCoordFromPosition(float3 globalPosition)
    {
        int x = Mathf.FloorToInt(globalPosition.x / Constants.CHUNKSIZE);
        int y = Mathf.FloorToInt(globalPosition.y / Constants.CHUNKSIZE);
        int z = Mathf.FloorToInt(globalPosition.z / Constants.CHUNKSIZE);
        return new int3(x, y, z);
    }

    public static int3 GetVoxelLocalPositionInChunk(float3 globalPosition)
    {
        return new int3(Mod(Mathf.FloorToInt(globalPosition.x), Constants.CHUNKSIZE),
                        Mod(Mathf.FloorToInt(globalPosition.y), Constants.CHUNKSIZE),
                        Mod(Mathf.FloorToInt(globalPosition.z), Constants.CHUNKSIZE));
    }

    public static int3 GetVoxelGlobalPositionFromChunk(float3 localPosition, int3 coord)
    {
        return new int3(Mathf.FloorToInt(localPosition.x) + (Constants.CHUNKSIZE * coord.x),
                        Mathf.FloorToInt(localPosition.y) + (Constants.CHUNKSIZE * coord.y),
                        Mathf.FloorToInt(localPosition.z) + (Constants.CHUNKSIZE * coord.z));
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