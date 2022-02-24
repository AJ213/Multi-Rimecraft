using UnityEngine;
using Unity.Mathematics;
using System.Collections.Concurrent;

[System.Serializable]
public class WorldData
{
    [System.NonSerialized]
    public static ConcurrentDictionary<int3, ChunkData> chunks = new ConcurrentDictionary<int3, ChunkData>();

    public WorldData()
    {
        chunks.Clear();
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

    public static ChunkData GetChunk(int3 globalPosition)
    {
        int3 coord = WorldHelper.GetChunkCoordFromPosition(globalPosition);
        if (!chunks.ContainsKey(coord))
        {
            return null;
        }

        return chunks[WorldHelper.GetChunkCoordFromPosition(globalPosition)];
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

    public static void UpdateSorroundingVoxels(int3 globalPosition)
    {
        for (int p = 0; p < 6; p++)
        {
            int3 currentVoxel = globalPosition + VoxelData.faceChecks[p];
            int3 coord = WorldHelper.GetChunkCoordFromPosition(currentVoxel);

            if (ChunkMeshManager.Instance.chunkMeshes.ContainsKey(coord))
            {
                RimecraftWorld.Instance.AddChunkToUpdate(coord, true);
            }
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