using UnityEngine;
using Unity.Mathematics;
using System.Collections.Concurrent;

[System.Serializable]
public class WorldData
{
    [System.NonSerialized]
    public ConcurrentDictionary<int3, ChunkData> chunks = new ConcurrentDictionary<int3, ChunkData>();

    public WorldData()
    {
        chunks.Clear();
    }

    public ChunkData RequestChunk(int3 coord)
    {
        if (chunks.ContainsKey(coord))
        {
            return chunks[coord];
        }
        return null;
    }

    public ChunkData RequestChunkViaGlobalPosition(int3 globalPosition)
    {
        return RequestChunk(WorldHelper.GetChunkCoordFromPosition(globalPosition));
    }

    public ushort CheckForVoxel(int3 globalPosition)
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

    public ushort GetVoxelFromPosition(float3 globalPosition)
    {
        ChunkData chunk = RequestChunkViaGlobalPosition((int3)globalPosition);
        if (chunk == null)
        {
            return 0;
        }
        else
        {
            return chunk.GetVoxel(WorldHelper.GetVoxelLocalPositionInChunk(globalPosition));
        }
    }

    public ChunkData GetChunk(int3 globalPosition)
    {
        int3 coord = WorldHelper.GetChunkCoordFromPosition(globalPosition);
        if (!chunks.ContainsKey(coord))
        {
            return null;
        }

        return chunks[WorldHelper.GetChunkCoordFromPosition(globalPosition)];
    }

    public void SetChunk(ChunkData chunk)
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
                RimecraftWorld.Instance.AddChunkToUpdate(coord);
            }
        }
    }

    public void SetVoxel(int3 globalPosition, ushort value)
    {
        ClientSend.ModifyVoxelChunk(globalPosition, value);
        RimecraftWorld.worldData.GetChunk(globalPosition).ModifyVoxel(WorldHelper.GetVoxelLocalPositionInChunk(globalPosition), value);
    }

    public ushort GetVoxel(int3 globalPosition)
    {
        ChunkData chunk = RequestChunk(WorldHelper.GetChunkCoordFromPosition(globalPosition));
        if (chunk == null)
        {
            return 0;
        }

        int3 voxel = WorldHelper.GetVoxelLocalPositionInChunk(globalPosition);
        try
        {
            return chunk[voxel.x, voxel.y, voxel.z];
        }
        catch (System.Exception e)
        {
            Debug.Log(globalPosition.x + ", " + globalPosition.y + ", " + globalPosition.z);
            Debug.Log(voxel.x + ", " + voxel.y + ", " + voxel.z);
            throw e;
        }
    }
}