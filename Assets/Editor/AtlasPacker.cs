﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class AtlasPacker : EditorWindow
{
    private int blockSize = 256;
    private int atlasSizeInBlocks = 4;
    private int atlasSize;

    private Object[] rawTextures = new Object[256];
    private List<Texture2D> sortedTextures = new List<Texture2D>();
    private Texture2D atlas = null;

    [MenuItem("Rimecraft/Atlas Packer")] // Create a menu item to show the window.
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(AtlasPacker));
    }

    private void OnGUI()
    {
        atlasSize = blockSize * atlasSizeInBlocks;

        GUILayout.Label("Rimecraft Texture Atlas Packer", EditorStyles.boldLabel);

        blockSize = EditorGUILayout.IntField("Block Size", blockSize);
        atlasSizeInBlocks = EditorGUILayout.IntField("Atlas Size (in blocks)", atlasSizeInBlocks);

        if (GUILayout.Button("Load Textures"))
        {
            LoadTextures();
            PackAtlas();

            Debug.Log("Atlas Packer: Textures loaded.");
        }

        if (GUILayout.Button("Clear Textures"))
        {
            atlas = new Texture2D(atlasSize, atlasSize);
            Debug.Log("Atlas Packer: Textures cleared.");
        }

        if (GUILayout.Button("Save Atlas"))
        {
            byte[] bytes = atlas.EncodeToPNG();
            try
            {
                File.WriteAllBytes(Application.dataPath + "/OurArt/Packed_Atlas.png", bytes);
                Debug.Log("Atlas Packer: Atlas file sucessfully saved. " + bytes.Length);
            }
            catch (System.Exception e)
            {
                Debug.Log("Atlas Packer: Couldn't save atlas to file.");
                throw e;
            }
        }
        GUILayout.Label(atlas);
    }

    private void LoadTextures()
    {
        // Editor variables don't get wiped so we need to clear this list or it will keep growing.
        sortedTextures.Clear();

        rawTextures = Resources.LoadAll("AtlasPacker", typeof(Texture2D));

        // Loop through each one and perform any checks we want to perform.
        int index = 0;
        foreach (Object tex in rawTextures)
        {
            // Convert the current object into a texture and check that it's the correct size before adding.
            Texture2D t = (Texture2D)tex;
            if (t.width == blockSize && t.height == blockSize)
            {
                sortedTextures.Add(t);
            }
            else
            {
                Debug.Log("Asset Packer: " + tex.name + " incorrect size. Texture not loaded.");
            }

            index++;
        }

        Debug.Log("Atlas Packer: " + sortedTextures.Count + " successfully loaded.");
    }

    private void PackAtlas()
    {
        atlas = new Texture2D(atlasSize, atlasSize);
        Color32[] pixels = new Color32[atlasSize * atlasSize];
        for (int x = 0; x < atlasSize; x++)
        {
            for (int y = 0; y < atlasSize; y++)
            {
                // Get the current block that we're looking at.
                int currentBlockX = x / blockSize;
                int currentBlockY = y / blockSize;

                int index = (currentBlockY * atlasSizeInBlocks) + currentBlockX;

                // Get the pixel in the current block.
                //int currentPixelX = x - (currentBlockX * blockSize);
                //int currentPixelY = y - (currentBlockY * blockSize);

                if (index < sortedTextures.Count)
                {
                    pixels[((atlasSize - y - 1) * atlasSize) + x] = sortedTextures[index].GetPixel(x, blockSize - y - 1);
                }
                else
                {
                    pixels[((atlasSize - y - 1) * atlasSize) + x] = new Color(0f, 0f, 0f, 0f);
                }
            }
        }

        atlas.SetPixels32(pixels);
        atlas.Apply();
    }
}