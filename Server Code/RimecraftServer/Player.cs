using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace RimecraftServer
{
    internal class Player
    {
        public int id;
        public string username;
        public int loadDistance;
        public Vector3 coord;
        public Vector3 lastCoord;
        public HashSet<Vector3> loadedChunks = new HashSet<Vector3>();

        public Vector3 position;
        public Quaternion rotation;

        public Player(int id, string username, int viewDistance, Vector3 spawnPosition)
        {
            this.id = id;
            this.username = username;
            this.loadDistance = viewDistance;
            this.position = spawnPosition;
            this.rotation = Quaternion.Identity;
            coord = Vector3.Zero;
            loadedChunks.Clear();
            lastCoord = new Vector3(-1, -1, -1);
        }

        public void UpdateLastCoord()
        {
            lastCoord = coord;
        }

        public void Update()
        {
            coord = WorldHelper.GetChunkCoordFromPosition(position);
            ChunkLoader.CheckLoadDistance(this);
            ServerSend.PlayerPosition(this);
            ServerSend.PlayerRotation(this);
        }

        public void SetMovement(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }
    }
}