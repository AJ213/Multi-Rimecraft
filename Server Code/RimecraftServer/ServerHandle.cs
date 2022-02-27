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
            int viewDistance = packet.ReadInt();

            Console.WriteLine($"{Server.clients[fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {fromClient}.");
            if (fromClient != clientIdCheck)
            {
                Console.WriteLine($"(ID: {fromClient}) has assumed the wrong client ID ({clientIdCheck})!");
                //Console.WriteLine($"Player \"{username}\" (ID: {fromClient}) has assumed the wrong client ID ({clientIdCheck})!");
            }

            Server.clients[fromClient].SendIntoGame(username, viewDistance);
        }

        public static void PlayerMovement(int fromClient, Packet packet)
        {
            Vector3 position = packet.ReadVector3();
            Quaternion rotation = packet.ReadQuaternion();

            if (Server.clients.ContainsKey(fromClient))
            {
                if (Server.clients[fromClient].player != null)
                {
                    Server.clients[fromClient].player.SetInput(position, rotation);
                }
            }
        }

        public static void SpawnProjectile(int fromClient, Packet packet)
        {
            Vector3 position = packet.ReadVector3();
            Vector3 direction = packet.ReadVector3();

            ServerSend.SpawnProjectile(fromClient, position, direction);
        }

        public static void DroppedItem(int fromClient, Packet packet)
        {
            Vector3 position = packet.ReadVector3();
            ushort id = packet.ReadUShort();
            string uuid = packet.ReadString();

            ServerSend.DroppedItem(fromClient, position, id, uuid);
        }

        public static void ModifyVoxel(int fromClient, Packet packet)
        {
            Vector3 globalPosition = packet.ReadVector3();
            ushort blockID = packet.ReadUShort();

            // Will then send the modified chunk data to all
            Program.worldData.SetVoxel(fromClient, globalPosition, blockID);
        }

        public static void PickupItem(int fromClient, Packet packet)
        {
            string uuid = packet.ReadString();

            ServerSend.PickupItem(fromClient, uuid);
        }
    }
}