using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace RimecraftServer
{
    internal class ServerSend
    {
        private static void SendTCPData(int toClient, Packet packet)
        {
            packet.WriteLength();
            Server.clients[toClient].tcp.SendData(packet);
        }

        private static void SendUDPData(int toClient, Packet packet)
        {
            packet.WriteLength();
            Server.clients[toClient].udp.SendData(packet);
        }

        private static void SendTCPDataToAll(Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                Server.clients[i].tcp.SendData(packet);
            }
        }

        private static void SendTCPDataToAll(int exceptClient, Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                if (i != exceptClient)
                {
                    Server.clients[i].tcp.SendData(packet);
                }
            }
        }

        private static void SendUDPDataToAll(Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                Server.clients[i].udp.SendData(packet);
            }
        }

        private static void SendUDPDataToAll(int exceptClient, Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                if (i != exceptClient)
                {
                    Server.clients[i].udp.SendData(packet);
                }
            }
        }

        #region Packets

        public static void Welcome(int toClient, string msg)
        {
            using (Packet packet = new Packet((int)ServerPackets.welcome))
            {
                packet.Write(msg);
                packet.Write(toClient);

                SendTCPData(toClient, packet);
            }
        }

        public static void SpawnPlayer(int toClient, Player player)
        {
            using (Packet packet = new Packet((int)ServerPackets.spawnPlayer))
            {
                packet.Write(player.id);
                packet.Write(player.username);
                packet.Write(player.position);
                packet.Write(player.rotation);

                SendTCPData(toClient, packet);
            }
        }

        public static void DroppedItem(int fromClient, Vector3 position, ushort id, int stackSize, string uuid)
        {
            using (Packet packet = new Packet((int)ServerPackets.droppedItem))
            {
                packet.Write(position);
                packet.Write(id);
                packet.Write(stackSize);
                packet.Write(uuid);

                SendTCPDataToAll(fromClient, packet);
            }
        }

        public static void SpawnProjectile(int fromClient, Vector3 position, Vector3 direction, float speed, int bounceCount)
        {
            using (Packet packet = new Packet((int)ServerPackets.spawnProjectile))
            {
                packet.Write(position);
                packet.Write(direction);
                packet.Write(speed);
                packet.Write(bounceCount);
                SendTCPDataToAll(fromClient, packet);
            }
        }

        public static void SendChunk(int toClient, ChunkData chunk)
        {
            using (Packet packet = new Packet((int)ServerPackets.chunkData))
            {
                packet.Write(chunk);
                SendTCPData(toClient, packet);
            }
        }

        public static void PickupItem(int fromClient, string uuid)
        {
            using (Packet packet = new Packet((int)ServerPackets.pickupItem))
            {
                packet.Write(uuid);

                SendTCPDataToAll(fromClient, packet);
            }
        }

        public static void ModifiedVoxel(int fromClient, Vector3 globalPosition, ushort id)
        {
            using (Packet packet = new Packet((int)ServerPackets.modifiedVoxel))
            {
                packet.Write(globalPosition);
                packet.Write(id);

                SendTCPDataToAll(fromClient, packet);
            }
        }

        /*public static void SendChunkToAll(ChunkData chunk)
        {
            using (Packet packet = new Packet((int)ServerPackets.chunkData))
            {
                packet.Write(chunk);

                SendTCPDataToAll(packet);
            }
        }*/

        public static void PlayerPosition(Player player)
        {
            using (Packet packet = new Packet((int)ServerPackets.playerPosition))
            {
                packet.Write(player.id);
                packet.Write(player.position);

                SendUDPDataToAll(player.id, packet);
            }
        }

        public static void PlayerRotation(Player player)
        {
            using (Packet packet = new Packet((int)ServerPackets.playerRotation))
            {
                packet.Write(player.id);
                packet.Write(player.rotation);

                SendUDPDataToAll(player.id, packet);
            }
        }

        #endregion Packets
    }
}