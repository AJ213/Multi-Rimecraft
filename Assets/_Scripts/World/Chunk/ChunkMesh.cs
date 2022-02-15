using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class ChunkMesh
{
    private GameObject chunkObject;
    private MeshRenderer meshRenderer = null;
    private MeshFilter meshFilter = null;

    private int vertexIndex = 0;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<int> transparentTriangles = new List<int>();
    private List<int> shinyTriangles = new List<int>();
    private Material[] materials = new Material[3];
    private List<Vector2> uvs = new List<Vector2>();
    private List<Vector3> normals = new List<Vector3>();

    public int3 coord;
    public int3 position;

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
        chunkObject.transform.position = new Vector3(coord.x * Constants.CHUNKSIZE, coord.y * Constants.CHUNKSIZE, coord.z * Constants.CHUNKSIZE);
        chunkObject.name = "Chunk " + coord.x + ", " + coord.y + "," + coord.z;
        position = new int3(Mathf.FloorToInt(chunkObject.transform.position.x), Mathf.FloorToInt(chunkObject.transform.position.y), Mathf.FloorToInt(chunkObject.transform.position.z));
    }

    public void UpdateChunk()
    {
        if (WorldData.chunks[coord] != null)
        {
            ClearMeshData();
            for (int y = 0; y < Constants.CHUNKSIZE; y++)
            {
                for (int x = 0; x < Constants.CHUNKSIZE; x++)
                {
                    for (int z = 0; z < Constants.CHUNKSIZE; z++)
                    {
                        if (ChunkMeshManager.Instance.blockTypes[WorldData.chunks[coord].map[x, y, z]] != null)
                        {
                            if (ChunkMeshManager.Instance.blockTypes[WorldData.chunks[coord].map[x, y, z]].IsSolid)
                            {
                                UpdateMeshData(new int3(x, y, z));
                            }
                        }
                    }
                }
            }
            ChunkMeshManager.Instance.chunksToDraw.Enqueue(this);
        }
    }

    private void UpdateMeshData(int3 localPosition)
    {
        ushort voxel = WorldData.chunks[coord].map[localPosition.x, localPosition.y, localPosition.z];

        for (int p = 0; p < 6; p++)
        {
            ushort neighbor = WorldData.GetVoxelFromPosition(WorldHelper.GetVoxelGlobalPositionFromChunk(localPosition, coord) + VoxelData.faceChecks[p]);
            if (ChunkMeshManager.Instance.blockTypes[neighbor].RenderNeighborFaces)
            {
                int faceVertCount = 0;
                for (int i = 0; i < ChunkMeshManager.Instance.blockTypes[voxel].MeshData.faces[p].vertData.Length; i++)
                {
                    vertices.Add(localPosition + (float3)ChunkMeshManager.Instance.blockTypes[voxel].MeshData.faces[p].vertData[i].position);
                    normals.Add(ChunkMeshManager.Instance.blockTypes[voxel].MeshData.faces[p].normal);
                    AddTexture(ChunkMeshManager.Instance.blockTypes[voxel].GetTextureID(p), ChunkMeshManager.Instance.blockTypes[voxel].MeshData.faces[p].vertData[i].uv);
                    faceVertCount++;
                }

                // spaghetti ice reflection programming
                if (voxel == 2)
                {
                    for (int i = 0; i < ChunkMeshManager.Instance.blockTypes[voxel].MeshData.faces[p].triangles.Length; i++)
                    {
                        shinyTriangles.Add(vertexIndex + ChunkMeshManager.Instance.blockTypes[voxel].MeshData.faces[p].triangles[i]);
                    }
                }
                else if (!ChunkMeshManager.Instance.blockTypes[voxel].RenderNeighborFaces)
                {
                    for (int i = 0; i < ChunkMeshManager.Instance.blockTypes[voxel].MeshData.faces[p].triangles.Length; i++)
                    {
                        triangles.Add(vertexIndex + ChunkMeshManager.Instance.blockTypes[voxel].MeshData.faces[p].triangles[i]);
                    }
                }
                else
                {
                    for (int i = 0; i < ChunkMeshManager.Instance.blockTypes[voxel].MeshData.faces[p].triangles.Length; i++)
                    {
                        transparentTriangles.Add(vertexIndex + ChunkMeshManager.Instance.blockTypes[voxel].MeshData.faces[p].triangles[i]);
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
        vertexIndex = 0;
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

    private void AddTexture(int textureID, Vector2 uv)
    {
        float y = textureID / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.TextureAtlasSizeInBlocks);

        x *= VoxelData.NoramlizedBlockTextureSize;
        y *= VoxelData.NoramlizedBlockTextureSize;

        y = 1 - y - VoxelData.NoramlizedBlockTextureSize;

        x += VoxelData.NoramlizedBlockTextureSize * uv.x;
        y += VoxelData.NoramlizedBlockTextureSize * uv.y;

        uvs.Add(new Vector2(x, y));
    }
}