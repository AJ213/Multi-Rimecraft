﻿using System.Collections.Generic;
using System.Threading;
using System.IO;
using UnityEngine;

public class World : MonoBehaviour
{
    public Settings settings;
    public BiomeAttributes[] biomes;

    public Transform player;
    public Vector3 spawnPosition;

    public Material material = null;
    public Material transparentMaterial = null;
    public BlockType[] blockTypes = null;

    private Chunk[,,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];

    private List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    public ChunkCoord playerChunkCoord;
    private ChunkCoord playerLastChunkCoord;

    private List<Chunk> chunksToUpdate = new List<Chunk>();
    public Queue<Chunk> chunksToDraw = new Queue<Chunk>();

    private bool applyingModifications = false;

    private Queue<Queue<VoxelMod>> modifications = new Queue<Queue<VoxelMod>>();

    private bool inUI = false;

    public GameObject debugScreen;

    private Thread ChunkUpdateThread;
    public object ChunkUpdateThreadLock = new object();
    public object ChunkListThreadLock = new object();

    private static World instance;
    public static World Instance => instance;

    public WorldData worldData;

    public string appPath;

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

        appPath = Application.persistentDataPath;
    }

    private void Start()
    {
        Debug.Log("Generating new world using seed " + VoxelData.seed);

        worldData = SaveSystem.LoadWorld("Prototype");

        Random.InitState(VoxelData.seed);
        Camera.main.farClipPlane = Mathf.Sqrt(2) * VoxelData.ChunkWidth * 2 * settings.viewDistanceInChunks;
        LoadWorld();

        spawnPosition = new Vector3(VoxelData.WorldSizeInVoxels / 2, VoxelData.WorldSizeInVoxels - 100, VoxelData.WorldSizeInVoxels / 2);
        player.position = spawnPosition;
        //CheckLoadDistance();
        CheckViewDistance();

        playerLastChunkCoord = GetChunkCoordFromVector3(player.position);

        if (settings.enableThreading)
        {
            ChunkUpdateThread = new Thread(new ThreadStart(ThreadedUpdate));
            ChunkUpdateThread.Start();
        }
    }

    private void Update()
    {
        playerChunkCoord = GetChunkCoordFromVector3(player.position);

        if (!playerChunkCoord.Equals(playerLastChunkCoord))
        {
            //CheckLoadDistance();
            CheckViewDistance();
        }

        if (!settings.enableThreading)
        {
            if (!applyingModifications)
            {
                ApplyModifications();
            }

            if (chunksToUpdate.Count > 0)
            {
                UpdateChunks();
            }
        }

        if (chunksToDraw.Count > 0)
        {
            chunksToDraw.Dequeue().CreateMesh();
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            debugScreen.SetActive(!debugScreen.activeSelf);
        }
    }

    private void LoadWorld()
    {
        for (int x = (VoxelData.WorldSizeInChunks / 2) - settings.loadDistance; x < (VoxelData.WorldSizeInChunks / 2) + settings.loadDistance; x++)
        {
            for (int y = (VoxelData.WorldSizeInChunks / 2) - settings.loadDistance; y < (VoxelData.WorldSizeInChunks / 2) + settings.loadDistance; y++)
            {
                for (int z = (VoxelData.WorldSizeInChunks / 2) - settings.loadDistance; z < (VoxelData.WorldSizeInChunks / 2) + settings.loadDistance; z++)
                {
                    worldData.LoadChunk(new Vector3Int(x, y, z));
                }
            }
        }
    }

    public void AddChunkToUpdate(Chunk chunk)
    {
        AddChunkToUpdate(chunk, false);
    }

    public void AddChunkToUpdate(Chunk chunk, bool insert)
    {
        // Lock list to ensure only one thing is using the list at a time.
        lock (ChunkUpdateThreadLock)
        {
            // Make sure update list doesn't already contain chunk.
            if (!chunksToUpdate.Contains(chunk))
            {
                // If insert is true, chunk gets inserted at the top of the list.
                if (insert)
                {
                    chunksToUpdate.Insert(0, chunk);
                }
                else
                {
                    chunksToUpdate.Add(chunk);
                }
            }
        }
    }

    private void UpdateChunks()
    {
        lock (ChunkUpdateThreadLock)
        {
            chunksToUpdate[0].UpdateChunk();
            if (!activeChunks.Contains(chunksToUpdate[0].coord))
            {
                activeChunks.Add(chunksToUpdate[0].coord);
            }
            chunksToUpdate.RemoveAt(0);
        }
    }

    private void ThreadedUpdate()
    {
        while (true)
        {
            if (!applyingModifications)
            {
                ApplyModifications();
            }

            if (chunksToUpdate.Count > 0)
            {
                UpdateChunks();
            }
        }
    }

    private void OnDisable()
    {
        if (settings.enableThreading)
        {
            ChunkUpdateThread.Abort();
        }
    }

    private void ApplyModifications()
    {
        applyingModifications = true;

        while (modifications.Count > 0)
        {
            Queue<VoxelMod> queue = modifications.Dequeue();

            while (queue.Count > 0)
            {
                VoxelMod v = queue.Dequeue();

                worldData.SetVoxel(v.position, v.id);
            }
        }
        applyingModifications = false;
    }

    private ChunkCoord GetChunkCoordFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int y = Mathf.FloorToInt(pos.y / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        return new ChunkCoord(x, y, z);
    }

    public Chunk GetChunkFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int y = Mathf.FloorToInt(pos.y / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        try
        {
            return chunks[x, y, z];
        }
        catch (System.Exception e)
        {
            Debug.Log(pos.x + ", " + pos.y + ", " + pos.z + "| due to " + x + ", " + y + ", " + z);
            throw e;
        }
    }

    private void CheckViewDistance()
    {
        ChunkCoord coord = GetChunkCoordFromVector3(player.position);
        playerLastChunkCoord = playerChunkCoord;

        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunks);

        activeChunks.Clear();

        // Loop through all chunks currently within view distance of the player.
        for (int x = coord.x - settings.viewDistanceInChunks; x < coord.x + settings.viewDistanceInChunks; x++)
        {
            for (int y = coord.y - settings.viewDistanceInChunks; y < coord.y + settings.viewDistanceInChunks; y++)
            {
                for (int z = coord.z - settings.viewDistanceInChunks; z < coord.z + settings.viewDistanceInChunks; z++)
                {
                    ChunkCoord thisChunkCoord = new ChunkCoord(x, y, z);

                    // If the current chunk is in the world...
                    if (IsInRange(thisChunkCoord.ToVector3Int(), VoxelData.WorldSizeInChunks))
                    {
                        // Check if it active, if not, activate it.
                        if (chunks[x, y, z] == null)
                        {
                            chunks[x, y, z] = new Chunk(thisChunkCoord);
                        }

                        chunks[x, y, z].IsActive = true;
                        activeChunks.Add(thisChunkCoord);
                    }

                    // Check through previously active chunks to see if this chunk is there. If it is, remove it from the list.
                    for (int i = 0; i < previouslyActiveChunks.Count; i++)
                    {
                        if (previouslyActiveChunks[i].Equals(thisChunkCoord))
                        {
                            previouslyActiveChunks.RemoveAt(i);
                        }
                    }
                }
            }
        }

        // Any chunks left in the previousActiveChunks list are no longer in the player's view distance, so loop through and disable them.
        foreach (ChunkCoord c in previouslyActiveChunks)
        {
            chunks[c.x, c.y, c.z].IsActive = false;
        }
    }

    private void CheckLoadDistance()
    {
        ChunkCoord coord = GetChunkCoordFromVector3(player.position);
        playerLastChunkCoord = playerChunkCoord;

        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunks);

        activeChunks.Clear();

        // Loop through all chunks currently within view distance of the player.
        for (int x = coord.x - settings.loadDistance; x < coord.x + settings.loadDistance; x++)
        {
            for (int y = coord.y - settings.viewDistanceInChunks; y < coord.y + settings.viewDistanceInChunks; y++)
            {
                for (int z = coord.z - settings.loadDistance; z < coord.z + settings.loadDistance; z++)
                {
                    ChunkCoord thisChunkCoord = new ChunkCoord(x, y, z);

                    // If the current chunk is in the world...
                    if (IsInRange(thisChunkCoord.ToVector3Int(), VoxelData.WorldSizeInChunks))
                    {
                        // Check if it active, if not, activate it.
                        if (chunks[x, y, z] == null)
                        {
                            chunks[x, y, z] = new Chunk(thisChunkCoord);
                        }

                        chunks[x, y, z].IsActive = true;
                        activeChunks.Add(thisChunkCoord);
                    }

                    // Check through previously active chunks to see if this chunk is there. If it is, remove it from the list.
                    for (int i = 0; i < previouslyActiveChunks.Count; i++)
                    {
                        if (previouslyActiveChunks[i].Equals(thisChunkCoord))
                        {
                            previouslyActiveChunks.RemoveAt(i);
                        }
                    }
                }
            }
        }

        // Any chunks left in the previousActiveChunks list are no longer in the player's view distance, so loop through and disable them.
        foreach (ChunkCoord c in previouslyActiveChunks)
        {
            chunks[c.x, c.y, c.z].IsActive = false;
        }
    }

    public bool CheckForVoxel(Vector3Int pos)
    {
        VoxelState voxel = worldData.GetVoxel(pos);
        if (voxel == null)
        {
            return false;
        }

        if (blockTypes[voxel.id].isSolid)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public VoxelState GetVoxelState(Vector3Int pos)
    {
        return worldData.GetVoxel(pos);
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

    public ushort GetVoxel(Vector3Int pos)
    {
        /* IMMUTABLE PASS */

        // If outside word, return air.
        if (!IsInRange(pos, VoxelData.WorldSizeInVoxels))
        {
            return 0;
        }

        // Bottom of world that is unbreakable.
        if (pos.y == 0)
        {
            return 2;
        }

        /* BIOME SELECTION PASS */

        int solidGroundHeight = VoxelData.WorldSizeInVoxels - 150;
        float sumOfHeights = 0;
        int count = 0;
        float strongestWeight = 0;
        int strongestBiomeIndex = 0;

        for (int i = 0; i < biomes.Length; i++)
        {
            float weight = Noise.Get2DPerlin(new Vector2(pos.x, pos.z), biomes[i].offset, biomes[i].scale);

            // Keep track of which weight is strongest.
            if (weight > strongestWeight)
            {
                strongestWeight = weight;
                strongestBiomeIndex = i;
            }

            // Get the height of the terrain (for the current biome) and multiply it by its weight.
            float height = biomes[i].terrainHeight * Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 2 * biomes[i].offset, biomes[i].terrainScale) * weight;

            // If the height value is greater 0 add it to the sum of heights.
            if (height > 0)
            {
                sumOfHeights += height;
                count++;
            }
        }

        // Set biome to the one with the strongest weight.
        BiomeAttributes biome = biomes[strongestBiomeIndex];

        // Get the average of the heights.
        if (count == 0)
        {
            count = 1;
        }
        sumOfHeights /= count;

        int terrainHeight = Mathf.FloorToInt(sumOfHeights + solidGroundHeight);
        ushort voxelValue = 0;

        SurfaceBlocks(ref voxelValue, pos, biome, terrainHeight);
        LodeGeneration(ref voxelValue, pos, biome);
        FloraGeneration(pos, biome, terrainHeight);

        return voxelValue;
    }

    private void SurfaceBlocks(ref ushort voxelValue, Vector3Int pos, BiomeAttributes biome, int terrainHeight)
    {
        if (pos.y == terrainHeight)
        {
            voxelValue = biome.surfaceBlock;
        }
        else if (pos.y < terrainHeight && pos.y > terrainHeight - 4)
        {
            voxelValue = biome.subSurfaceBlock;
        }
        else if (pos.y > terrainHeight)
        {
            voxelValue = 0;
        }
        else
        {
            voxelValue = 3;
        }
    }

    private void LodeGeneration(ref ushort voxelValue, Vector3Int pos, BiomeAttributes biome)
    {
        if (voxelValue == 3 || voxelValue == 1)
        {
            foreach (Lode lode in biome.lodes)
            {
                if (pos.y > lode.minHeight && pos.y < lode.maxHeight)
                {
                    if (Noise.Get3DPerlin(pos, lode.noiseOffset, lode.scale) > lode.threshold)
                    {
                        voxelValue = lode.blockID;
                    }
                }
            }
        }
    }

    private void FloraGeneration(Vector3Int pos, BiomeAttributes biome, int terrainHeight)
    {
        if (pos.y == terrainHeight && biome.placeMajorFlora)
        {
            if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 200, biome.majorFloraZoneScale) > biome.majorFloraZoneThreshold)
            {
                if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 700, biome.majorFloraPlacementScale) > biome.majorFloraPlacementThreshold)
                {
                    modifications.Enqueue(Structure.GenerateMajorFlora(biome.majorFloraIndex, pos, biome.minHeight, biome.maxHeight));
                }
            }
        }
    }

    public static bool IsInRange(int value, int length)
    {
        return (value >= 0 && value < length);
    }

    public static bool IsInRange(Vector3Int value, int length)
    {
        return (IsInRange(value.x, length) && IsInRange(value.y, length) && IsInRange(value.z, length));
    }
}