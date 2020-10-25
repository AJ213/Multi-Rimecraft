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

    private Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];

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

        LoadWorld();

        spawnPosition = new Vector3(VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth / 2, VoxelData.ChunkHeight - 50, VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth / 2);
        player.position = spawnPosition;
        CheckViewDistance();
        playerLastChunkCoord = GetChunkCoordFromVector3(player.position);

        ChunkUpdateThread = new Thread(new ThreadStart(ThreadedUpdate));
        ChunkUpdateThread.Start();
    }

    private void Update()
    {
        playerChunkCoord = GetChunkCoordFromVector3(player.position);

        if (!playerChunkCoord.Equals(playerLastChunkCoord))
        {
            CheckViewDistance();
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
            for (int z = (VoxelData.WorldSizeInChunks / 2) - settings.loadDistance; z < (VoxelData.WorldSizeInChunks / 2) + settings.loadDistance; z++)
            {
                worldData.LoadChunk(new Vector2Int(x, z));
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
        ChunkUpdateThread.Abort();
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
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        return new ChunkCoord(x, z);
    }

    public Chunk GetChunkFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        return chunks[x, z];
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
            for (int z = coord.z - settings.viewDistanceInChunks; z < coord.z + settings.viewDistanceInChunks; z++)
            {
                ChunkCoord thisChunkCoord = new ChunkCoord(x, z);

                // If the current chunk is in the world...
                if (IsChunkInWorld(thisChunkCoord))
                {
                    // Check if it active, if not, activate it.
                    if (chunks[x, z] == null)
                    {
                        chunks[x, z] = new Chunk(thisChunkCoord);
                    }

                    chunks[x, z].IsActive = true;
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

        // Any chunks left in the previousActiveChunks list are no longer in the player's view distance, so loop through and disable them.
        foreach (ChunkCoord c in previouslyActiveChunks)
        {
            chunks[c.x, c.z].IsActive = false;
        }
    }

    public bool CheckForVoxel(Vector3 pos)
    {
        VoxelState voxel = worldData.GetVoxel(pos);

        if (blockTypes[voxel.id].isSolid)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public VoxelState GetVoxelState(Vector3 pos)
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

    public ushort GetVoxel(Vector3 pos)
    {
        /* IMMUTABLE PASS */

        // If outside word, return air.
        if (!IsVoxelInWorld(pos))
        {
            return 0;
        }

        // Bottom of world that is unbreakable.
        if (pos.y == 0)
        {
            return 2;
        }

        /* BIOME SELECTION PASS */

        int solidGroundHeight = 42;
        float sumOfHeights = 0;
        int count = 0;
        float strongestWeight = 0;
        int strongestBiomeIndex = 0;

        for (int i = 0; i < biomes.Length; i++)
        {
            float weight = Noise.Get2DPerlin(new Vector2(pos.x, pos.z), biomes[i].offset, biomes[i].scale);

            // keeps track of the strongest weight
            if (weight > strongestWeight)
            {
                strongestWeight = weight;
                strongestBiomeIndex = i;
            }

            // Get height of terrain and multiply it by its weight
            float height = biomes[i].terrainheight * Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biomes[i].terrainScale) * weight;

            // if height value is greater 0 add it to sum
            if (height > 0)
            {
                sumOfHeights += height;
                count++;
            }
        }

        // Set biome to the one with the strongest weight.
        BiomeAttributes biome = biomes[strongestBiomeIndex];

        // Get the average of heights
        sumOfHeights /= count;
        int terrainHeight = Mathf.FloorToInt(sumOfHeights + solidGroundHeight);

        /* BASIC TERRAIN PASS */

        ushort voxelValue;

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
            return 0;
        }
        else
        {
            voxelValue = 3;
        }

        /* SECOND PASS */

        if (voxelValue == 3 || voxelValue == 1)
        {
            foreach (Lode lode in biome.lodes)
            {
                if (pos.y > lode.minHeight && pos.y < lode.maxHeight)
                {
                    if (Noise.Get3DPerlin(pos, lode.noiseOffset, lode.scale, lode.threshold))
                    {
                        voxelValue = lode.blockID;
                    }
                }
            }
        }

        /* FLORA PASS */

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

        return voxelValue;
    }

    private bool IsChunkInWorld(ChunkCoord coord)
    {
        return (coord.x > 0 && coord.x < VoxelData.WorldSizeInChunks - 1 && coord.z > 0 && coord.z < VoxelData.WorldSizeInChunks - 1);
    }

    private bool IsVoxelInWorld(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < VoxelData.WorldSizeInVoxels && pos.y >= 0 && pos.y < VoxelData.ChunkHeight && pos.z >= 0 && pos.z < VoxelData.WorldSizeInVoxels)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}

[System.Serializable]
public class BlockType
{
    public string blockName;
    public bool isSolid;
    public VoxelMeshData meshData;
    public bool renderNeightborFaces;
    public Sprite icon;

    [Header("Texture Values")]
    public int backFaceTexture;

    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;

    // Back, Front, Top, Bottom, Left, Right

    public int GetTextureID(int faceIndex)
    {
        switch (faceIndex)
        {
            case 0:
                return backFaceTexture;

            case 1:
                return frontFaceTexture;

            case 2:
                return topFaceTexture;

            case 3:
                return bottomFaceTexture;

            case 4:
                return leftFaceTexture;

            case 5:
                return rightFaceTexture;

            default:
                Debug.Log("Error in GetTextureID; invalid face index");
                return 0;
        }
    }
}

public class VoxelMod
{
    public Vector3 position;
    public ushort id;

    public VoxelMod()
    {
        position = new Vector3();
        id = 0;
    }

    public VoxelMod(Vector3 position, ushort id)
    {
        this.position = position;
        this.id = id;
    }
}

[System.Serializable]
public class Settings
{
    [Header("Game Data")]
    public string version = "0.1";

    [Header("Performance")]
    public int loadDistance = 16;

    public int viewDistanceInChunks = 8;

    [Range(0.1f, 10f)]
    public float mouseSensitivity = 2;
}