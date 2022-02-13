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

        public Vector3 position;
        public Quaternion rotation;

        public Player(int id, string username, Vector3 spawnPosition)
        {
            this.id = id;
            this.username = username;
            this.position = spawnPosition;
            this.rotation = Quaternion.Identity;
        }

        public void Update()
        {
            ServerSend.PlayerPosition(this);
            ServerSend.PlayerRotation(this);
        }
    }
}