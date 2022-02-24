using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    private static void SendTCPData(Packet packet)
    {
        packet.WriteLength();
        Client.Instance.tcp.SendData(packet);
    }

    #region Packets

    private static void SendUDPData(Packet packet)
    {
        packet.WriteLength();
        Client.Instance.udp.SendData(packet);
    }

    public static void WelcomeReceived()
    {
        using (Packet packet = new Packet((int)ClientPackets.welcomeReceived))
        {
            packet.Write(Client.Instance.myId);
            packet.Write("User");

            SendTCPData(packet);
        }
    }

    public static void PlayerMovement()
    {
        using (Packet packet = new Packet((int)ClientPackets.playerMovement))
        {
            packet.Write(GameManager.players[Client.Instance.myId].transform.position);
            packet.Write(GameManager.players[Client.Instance.myId].transform.rotation);

            SendUDPData(packet);
        }
    }

    public static void RequestChunk(Vector3 coord)
    {
        using (Packet packet = new Packet((int)ClientPackets.requestChunk))
        {
            packet.Write(coord);

            SendTCPData(packet);
        }
    }

    public static void ModifyVoxelChunk(int3 globalPosition, ushort blockId)
    {
        using (Packet packet = new Packet((int)ClientPackets.modifyVoxel))
        {
            packet.Write((float3)globalPosition);
            packet.Write(blockId);

            SendTCPData(packet);
        }
    }

    public static void SpawnProjectile(Vector3 location, Vector3 direction)
    {
        using (Packet packet = new Packet((int)ClientPackets.spawnProjectile))
        {
            packet.Write(location);
            packet.Write(direction);

            SendTCPData(packet);
        }
    }

    public static void PickupItem(string uuid)
    {
        using (Packet packet = new Packet((int)ClientPackets.pickupItem))
        {
            packet.Write(uuid);

            SendTCPData(packet);
        }
    }

    public static void DroppedItem(Vector3 position, ushort id, string uuid)
    {
        using (Packet packet = new Packet((int)ClientPackets.droppedItem))
        {
            packet.Write(position);
            packet.Write(id);
            packet.Write(uuid);

            SendTCPData(packet);
        }
    }

    #endregion Packets
}