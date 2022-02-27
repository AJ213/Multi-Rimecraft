using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System.Collections.Concurrent;

public class ChunkMesh
{
    private GameObject chunkObject;
    private MeshRenderer meshRenderer = null;
    private MeshFilter meshFilter = null;

    public int3 coord;
    private ConcurrentQueue<Vector3> vertices = new ConcurrentQueue<Vector3>();
    private ConcurrentQueue<int> triangles = new ConcurrentQueue<int>();
    private ConcurrentQueue<int> transparentTriangles = new ConcurrentQueue<int>();
    private ConcurrentQueue<int> shinyTriangles = new ConcurrentQueue<int>();
    private ConcurrentQueue<Vector2> uvs = new ConcurrentQueue<Vector2>();
    private ConcurrentQueue<Vector3> normals = new ConcurrentQueue<Vector3>();

    private Material[] materials = new Material[3];

    private bool isActive;

    public ChunkMesh(int3 coord)
    {
        this.coord = coord;

        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();

        materials[0] = ChunkMeshManager.Instance.material;
        materials[1] = ChunkMeshManager.Instance.transparentMaterial;
        materials[2] = ChunkMeshManager.Instance.shinyMaterial;
        meshRenderer.materials = materials;

        chunkObject.transform.SetParent(RimecraftWorld.Instance.transform);
        chunkObject.transform.position = new Vector3(coord.x * Constants.CHUNK_SIZE, coord.y * Constants.CHUNK_SIZE, coord.z * Constants.CHUNK_SIZE);
        chunkObject.name = "Chunk " + coord.x + ", " + coord.y + "," + coord.z;
    }

    private void UpdateChunk()
    {
        if (RimecraftWorld.worldData.chunks.ContainsKey(coord) && RimecraftWorld.worldData.chunks[coord] != null)
        {
            ClearMeshData();
            int vertexIndex = 0;
            for (int y = 0; y < Constants.CHUNK_SIZE; y++)
            {
                for (int x = 0; x < Constants.CHUNK_SIZE; x++)
                {
                    for (int z = 0; z < Constants.CHUNK_SIZE; z++)
                    {
                        if (ChunkMeshManager.Instance.blockTypes[RimecraftWorld.worldData.chunks[coord][x, y, z]] != null)
                        {
                            if (ChunkMeshManager.Instance.blockTypes[RimecraftWorld.worldData.chunks[coord][x, y, z]].IsSolid)
                            {
                                UpdateMeshData(new int3(x, y, z), ref vertexIndex);
                            }
                        }
                    }
                }
            }
            ChunkMeshManager.Instance.chunksToDraw.Enqueue(this);
        }
    }

    public static void CreateMeshData(object state)
    {
        int3 coord = (int3)state;
        ChunkMesh chunkMesh;
        ChunkMeshManager.Instance.chunkMeshes.TryGetValue(coord, out chunkMesh);
        chunkMesh.UpdateChunk();
    }

    private void UpdateMeshData(int3 localPosition, ref int vertexIndex)
    {
        ushort voxel = RimecraftWorld.worldData.chunks[coord][localPosition.x, localPosition.y, localPosition.z];

        for (int p = 0; p < 6; p++)
        {
            ushort neighbor = RimecraftWorld.worldData.GetVoxelFromPosition(WorldHelper.GetVoxelGlobalPositionFromChunk(localPosition, coord) + VoxelData.faceChecks[p]);
            if (ChunkMeshManager.Instance.blockTypes[neighbor].RenderNeighborFaces)
            {
                int faceVertCount = 0;
                for (int i = 0; i < ChunkMeshManager.Instance.blockTypes[voxel].MeshData.faces[p].vertData.Length; i++)
                {
                    vertices.Enqueue(localPosition + (float3)ChunkMeshManager.Instance.blockTypes[voxel].MeshData.faces[p].vertData[i].position);
                    normals.Enqueue(ChunkMeshManager.Instance.blockTypes[voxel].MeshData.faces[p].normal);
                    AddTexture(ChunkMeshManager.Instance.blockTypes[voxel].GetTextureID(p), ChunkMeshManager.Instance.blockTypes[voxel].MeshData.faces[p].vertData[i].uv, uvs);
                    faceVertCount++;
                }

                // spaghetti ice reflection programming
                if (voxel == 2)
                {
                    for (int i = 0; i < ChunkMeshManager.Instance.blockTypes[voxel].MeshData.faces[p].triangles.Length; i++)
                    {
                        shinyTriangles.Enqueue(vertexIndex + ChunkMeshManager.Instance.blockTypes[voxel].MeshData.faces[p].triangles[i]);
                    }
                }
                else if (!ChunkMeshManager.Instance.blockTypes[voxel].RenderNeighborFaces)
                {
                    for (int i = 0; i < ChunkMeshManager.Instance.blockTypes[voxel].MeshData.faces[p].triangles.Length; i++)
                    {
                        triangles.Enqueue(vertexIndex + ChunkMeshManager.Instance.blockTypes[voxel].MeshData.faces[p].triangles[i]);
                    }
                }
                else
                {
                    for (int i = 0; i < ChunkMeshManager.Instance.blockTypes[voxel].MeshData.faces[p].triangles.Length; i++)
                    {
                        transparentTriangles.Enqueue(vertexIndex + ChunkMeshManager.Instance.blockTypes[voxel].MeshData.faces[p].triangles[i]);
                    }
                }

                vertexIndex += faceVertCount;
            }
        }
    }

    public void CreateMesh()
    {
        Mesh mesh = new Mesh
        {
            vertices = vertices.ToArray(),
            uv = uvs.ToArray()
        };
        mesh.subMeshCount = 3;
        mesh.SetTriangles(triangles.ToArray(), 0);
        mesh.SetTriangles(transparentTriangles.ToArray(), 1);
        mesh.SetTriangles(shinyTriangles.ToArray(), 2);
        mesh.normals = normals.ToArray();
        meshFilter.mesh = mesh;
    }

    private void ClearMeshData()
    {
        vertices.Clear();
        triangles.Clear();
        transparentTriangles.Clear();
        shinyTriangles.Clear();
        uvs.Clear();
        normals.Clear();
    }

    public bool IsActive
    {
        get { return isActive; }
        set
        {
            isActive = value;
            if (chunkObject != null)
            {
                chunkObject.SetActive(value);
            }
        }
    }

    private void AddTexture(int textureID, Vector2 uv, ConcurrentQueue<Vector2> uvs)
    {
        float y = textureID / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.TextureAtlasSizeInBlocks);

        x *= VoxelData.NoramlizedBlockTextureSize;
        y *= VoxelData.NoramlizedBlockTextureSize;

        y = 1 - y - VoxelData.NoramlizedBlockTextureSize;

        x += VoxelData.NoramlizedBlockTextureSize * uv.x;
        y += VoxelData.NoramlizedBlockTextureSize * uv.y;

        uvs.Enqueue(new Vector2(x, y));
    }
}