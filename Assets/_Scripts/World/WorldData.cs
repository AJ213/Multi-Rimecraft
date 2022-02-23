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
            ClientSend.RequestChunk((float3)coord);
            Debug.Log("will the chunk arrive in time? is chunk null?" + chunks[coord] == null);
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

    public static void SetChunk(ChunkData chunk)
    {
        if (chunks.ContainsKey(chunk.Coord))
        {
            // Maybe remove and then add?
            chunks[chunk.Coord] = chunk;
            RimecraftWorld.Instance.AddChunkToUpdate(chunk.Coord);
        }
        else
        {
            chunks.TryAdd(chunk.Coord, chunk);
            RimecraftWorld.Instance.AddChunkToUpdate(chunk.Coord);
        }
    }

    public void SetVoxel(int3 globalPosition, ushort value)
    {
        ClientSend.ModifyVoxelChunk(globalPosition, value);
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