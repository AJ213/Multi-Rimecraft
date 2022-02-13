using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using System.Collections.Concurrent;

public class RimecraftWorld : MonoBehaviour
{
    public static Settings settings;
    public BiomeAttributes[] biomes;

    public bool JobsEnabled = false;

    public Transform player;
    public Vector3 spawnPosition;

    public Material material = null;
    public Material transparentMaterial = null;
    public Material shinyMaterial = null;
    public AllBlockTypes blockTypes = null;

    [HideInInspector]
    public Dictionary<int3, ChunkMesh> chunkMeshes = new Dictionary<int3, ChunkMesh>();

    private HashSet<int3> activeChunks = new HashSet<int3>();

    public int3 playerChunkCoord;
    private int3 playerLastChunkCoord;

    private List<int3> chunksToUpdate = new List<int3>();
    public Queue<ChunkMesh> chunksToDraw = new Queue<ChunkMesh>();

    private bool inUI = false;

    private static RimecraftWorld instance;
    public static RimecraftWorld Instance => instance;

    public static WorldData worldData;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        worldData = new WorldData("null", WorldData.seed);
        //Debug.Log("Generating new world using seed " + WorldData.seed);

        if (settings == null)
        {
            settings = new Settings();
            settings.viewDistance = 2;
            settings.mouseSensitivity = 2;
        }

        UnityEngine.Random.InitState(WorldData.seed);
        Camera.main.farClipPlane = Mathf.Sqrt(2) * Constants.ChunkSizeX * 2 * settings.viewDistance;

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

        if (chunksToDraw.Count > 0)
        {
            chunksToDraw.Dequeue().CreateMesh();
        }
    }

    public void AddChunkToUpdate(int3 coord, bool insert = false)
    {
        if (!chunkMeshes.ContainsKey(coord))
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
        chunkMeshes[chunksToUpdate[0]].UpdateChunk();
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
                    if (!chunkMeshes.ContainsKey(newCoord))
                    {
                        chunkMeshes[newCoord] = new ChunkMesh(newCoord);
                        WorldData.RequestChunk(newCoord, true);
                        AddChunkToUpdate(newCoord);
                    }

                    chunkMeshes[newCoord].IsActive = true;
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
            chunkMeshes[new int3(c.x, c.y, c.z)].IsActive = false;
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
                    if (!chunkMeshes.ContainsKey(location))
                    {
                        positions[usageCount] = location;
                        newChunks = true;
                        usageCount++;
                    }
                }
            }
        }

        if (JobsEnabled)
        {
            if (!newChunks)
            {
                // We don't want to bother loading jobs if there is nothing new to load
                positions.Dispose();
            }
            else
            {
                var job = new LoadJob()
                {
                    positions = positions,
                };
                job.Schedule(usageCount, 2);
                JobHandle.ScheduleBatchedJobs();
            }
        }
        else
        {
            for (int i = 0; i < usageCount; i++)
            {
                WorldData.LoadChunk(positions[i]);
            }
            positions.Dispose();
        }
    }

    internal struct LoadJob : IJobParallelFor
    {
        [DeallocateOnJobCompletion] public NativeArray<int3> positions;

        public void Execute(int i)
        {
            WorldData.LoadChunk(positions[i]);
        }
    }

    public ushort CheckForVoxel(int3 globalPosition)
    {
        ushort voxel = worldData.GetVoxel(globalPosition);

        if (blockTypes[voxel].IsSolid)
        {
            return voxel;
        }
        else
        {
            return 0;
        }
    }

    public bool InUI
    {
        get { return inUI; }
        set
        {
            inUI = value;
            if (inUI)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}