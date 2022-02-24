using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using System.Collections.Concurrent;

public class RimecraftWorld : MonoBehaviourSingleton<RimecraftWorld>
{
    public static Settings settings;
    public BiomeAttributes[] biomes;
    public GameObject[] sounds;

    public Transform player;
    public Vector3 spawnPosition;

    private HashSet<int3> activeChunks = new HashSet<int3>();

    public int3 playerChunkCoord;
    private int3 playerLastChunkCoord;

    private List<int3> chunksToUpdate = new List<int3>();

    public static WorldData worldData;

    private void Start()
    {
        worldData = new WorldData();

        if (settings == null)
        {
            settings = new Settings();
            settings.viewDistance = 2;
            settings.mouseSensitivity = 2;
        }

        Camera.main.farClipPlane = Mathf.Sqrt(2) * Constants.CHUNKSIZE * 2 * settings.viewDistance;

        spawnPosition = new Vector3(0, 5, 0);
        player = GameObject.FindGameObjectWithTag("Player").transform;
        player.position = spawnPosition;
        CheckLoadDistance();
        CheckViewDistance();

        playerLastChunkCoord = WorldHelper.GetChunkCoordFromPosition(player.position);
    }

    private void Update()
    {
        playerChunkCoord = WorldHelper.GetChunkCoordFromPosition(player.position);

        if (!playerChunkCoord.Equals(playerLastChunkCoord))
        {
            CheckLoadDistance();
            CheckViewDistance();
        }

        if (chunksToUpdate.Count > 0)
        {
            UpdateChunks();
        }
    }

    public void AddChunkToUpdate(int3 coord, bool insert = false)
    {
        if (!ChunkMeshManager.Instance.chunkMeshes.ContainsKey(coord))
        {
            return;
        }
        // Lock list to ensure only one thing is using the list at a time.

        // Make sure update list doesn't already contain chunk.
        if (chunksToUpdate.Contains(coord))
        {
            return;
        }

        // If insert is true, chunk gets inserted at the top of the list.
        if (insert)
        {
            chunksToUpdate.Insert(0, coord);
        }
        else
        {
            chunksToUpdate.Add(coord);
        }
    }

    private void UpdateChunks()
    {
        ChunkMeshManager.Instance.chunkMeshes[chunksToUpdate[0]].UpdateChunk();
        if (!activeChunks.Contains(chunksToUpdate[0]))
        {
            activeChunks.Add(chunksToUpdate[0]);
        }
        chunksToUpdate.RemoveAt(0);
    }

    private void CheckViewDistance()
    {
        int3 coord = WorldHelper.GetChunkCoordFromPosition(player.position);
        playerLastChunkCoord = playerChunkCoord;
        List<int3> previouslyActiveChunks = new List<int3>(activeChunks);
        activeChunks.Clear();

        int3 minimum = new int3(coord.x - settings.viewDistance, coord.y - settings.viewDistance, coord.z - settings.viewDistance);
        int3 maximum = new int3(coord.x + settings.viewDistance, coord.y + settings.viewDistance, coord.z + settings.viewDistance);

        // Uses an alternating sequence. Starts at the center of the player and moves out step by step.
        for (int x = coord.x, counterx = 1; x >= minimum.x && x < maximum.x; x += counterx * (int)math.pow(-1, counterx - 1), counterx++)
        {
            for (int y = coord.y, countery = 1; y >= minimum.y && y < maximum.y; y += countery * (int)math.pow(-1, countery - 1), countery++)
            {
                for (int z = coord.z, counterz = 1; z >= minimum.z && z < maximum.z; z += counterz * (int)math.pow(-1, counterz - 1), counterz++)
                {
                    int3 newCoord = new int3(x, y, z);
                    if (!ChunkMeshManager.Instance.chunkMeshes.ContainsKey(newCoord))
                    {
                        ChunkMeshManager.Instance.chunkMeshes[newCoord] = new ChunkMesh(newCoord);
                        WorldData.RequestChunk(newCoord, false);
                        AddChunkToUpdate(newCoord);
                    }

                    ChunkMeshManager.Instance.chunkMeshes[newCoord].IsActive = true;
                    activeChunks.Add(newCoord);

                    for (int i = 0; i < previouslyActiveChunks.Count; i++)
                    {
                        if (previouslyActiveChunks[i].Equals(newCoord))
                        {
                            previouslyActiveChunks.RemoveAt(i);
                        }
                    }
                }
            }
        }

        // Any chunks left in the previousActiveChunks list are no longer in the player's view
        // distance, so loop through and disable them.
        foreach (int3 c in previouslyActiveChunks)
        {
            ChunkMeshManager.Instance.chunkMeshes[new int3(c.x, c.y, c.z)].IsActive = false;
        }
    }

    private void CheckLoadDistance()
    {
        int3 coord = WorldHelper.GetChunkCoordFromPosition(player.position);
        playerLastChunkCoord = playerChunkCoord;

        // This is our loadDistance * 2 cubed. Shouldn't ever be bigger than this size for the array
        int size = 8 * settings.viewDistance * settings.viewDistance * settings.viewDistance;
        NativeArray<int3> positions = new NativeArray<int3>(size, Allocator.Persistent);
        int usageCount = 0;
        bool newChunks = false;

        for (int x = coord.x - settings.viewDistance; x < coord.x + settings.viewDistance; x++)
        {
            for (int y = coord.y - settings.viewDistance; y < coord.y + settings.viewDistance; y++)
            {
                for (int z = coord.z - settings.viewDistance; z < coord.z + settings.viewDistance; z++)
                {
                    int3 location = new int3(x, y, z);
                    if (!ChunkMeshManager.Instance.chunkMeshes.ContainsKey(location))
                    {
                        positions[usageCount] = location;
                        newChunks = true;
                        usageCount++;
                    }
                }
            }
        }

        if (newChunks)
        {
            for (int i = 0; i < usageCount; i++)
            {
                ClientSend.RequestChunk((float3)positions[i]);
            }
        }
        positions.Dispose();
    }
}