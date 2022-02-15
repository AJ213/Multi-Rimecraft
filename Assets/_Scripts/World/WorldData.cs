using UnityEngine;
using Unity.Mathematics;
using System.Collections.Concurrent;

[System.Serializable]
public class WorldData
{
    public static string worldName = "Prototype";
    public static int seed;

    [System.NonSerialized]
    public static ConcurrentDictionary<int3, ChunkData> chunks = new ConcurrentDictionary<int3, ChunkData>();

    public WorldData(string name, int theSeed)
    {
        worldName = name;
        seed = theSeed;
    }

    public static ChunkData RequestChunk(int3 coord, bool create)
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

    public static ChunkData RequestChunkViaGlobalPosition(int3 globalPosition, bool create)
    {
        return RequestChunk(WorldHelper.GetChunkCoordFromPosition(globalPosition), create);
    }

    public static ushort CheckForVoxel(int3 globalPosition)
    {
        ushort voxel = GetVoxel(globalPosition);

        if (ChunkMeshManager.Instance.blockTypes[voxel].IsSolid)
        {
            return voxel;
        }
        else
        {
            return 0;
        }
    }

    public static ushort GetVoxelFromPosition(float3 globalPosition)
    {
        ChunkData chunk = RequestChunkViaGlobalPosition((int3)globalPosition, false);
        if (chunk == null)
        {
            return 0;
        }
        else
        {
            return chunk.VoxelFromPosition(WorldHelper.GetVoxelLocalPositionInChunk(globalPosition));
        }
    }

    public static void LoadChunk(int3 coord)
    {
        if (chunks.ContainsKey(coord))
        {
            return;
        }

        chunks.TryAdd(coord, new ChunkData(coord));

        ChunkData.Populate(chunks[coord]);
    }

    public static void SetChunk(ChunkData chunk)
    {
        chunks.TryAdd(chunk.Coord, chunk);
    }

    public void SetVoxel(int3 globalPosition, ushort value)
    {
        ChunkData chunk = RequestChunk(WorldHelper.GetChunkCoordFromPosition(globalPosition), false);

        int3 voxel = WorldHelper.GetVoxelLocalPositionInChunk(globalPosition);

        chunk.ModifyVoxel(voxel, value, true);
    }

    public static ushort GetVoxel(int3 globalPosition)
    {
        ChunkData chunk = RequestChunk(WorldHelper.GetChunkCoordFromPosition(globalPosition), false);
        if (chunk == null)
        {
            return 0;
        }

        int3 voxel = WorldHelper.GetVoxelLocalPositionInChunk(globalPosition);
        try
        {
            return chunk.map[voxel.x, voxel.y, voxel.z];
        }
        catch (System.Exception e)
        {
            Debug.Log(globalPosition.x + ", " + globalPosition.y + ", " + globalPosition.z);
            Debug.Log(voxel.x + ", " + voxel.y + ", " + voxel.z);
            throw e;
        }
    }
}