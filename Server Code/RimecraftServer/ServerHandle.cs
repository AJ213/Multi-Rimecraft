using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace RimecraftServer
{
    internal class ServerHandle
    {
        public static void WelcomeReceived(int fromClient, Packet packet)
        {
            int clientIdCheck = packet.ReadInt();
            string username = packet.ReadString();

            Console.WriteLine($"{Server.clients[fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {fromClient}.");
            if (fromClient != clientIdCheck)
            {
                Console.WriteLine($"Player \"{username}\" (ID: {fromClient}) has assumed the wrong client ID ({clientIdCheck})!");
            }

            Server.clients[fromClient].SendIntoGame(username);
        }

        public static void PlayerMovement(int fromClient, Packet packet)
        {
            Vector3 position = packet.ReadVector3();
            Quaternion rotation = packet.ReadQuaternion();

            Server.clients[fromClient].player.SetInput(position, rotation);
        }

        public static void SpawnProjectile(int fromClient, Packet packet)
        {
            Vector3 position = packet.ReadVector3();
            Vector3 direction = packet.ReadVector3();

            ServerSend.SpawnProjectile(fromClient, position, direction);
        }

        public static void RequestChunk(int fromClient, Packet packet)
        {
            Vector3 coord = packet.ReadVector3();
            ChunkData data = Program.worldData.RequestChunk(coord, true);
            ServerSend.SendChunk(fromClient, data);
        }

        public static void ModifyVoxel(int fromClient, Packet packet)
        {
            Vector3 globalPosition = packet.ReadVector3();
            ushort blockID = packet.ReadUShort();

            // Will then send the modified chunk data to all
            Program.worldData.SetVoxel(globalPosition, blockID);
        }
    }
}