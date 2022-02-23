using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    private static void SendTCPData(Packet packet)
    {
        packet.WriteLength();
        Client.instance.tcp.SendData(packet);
    }

    #region Packets

    private static void SendUDPData(Packet packet)
    {
        packet.WriteLength();
        Client.instance.udp.SendData(packet);
    }

    public static void WelcomeReceived()
    {
        using (Packet packet = new Packet((int)ClientPackets.welcomeReceived))
        {
            packet.Write(Client.instance.myId);
            packet.Write(UIManager.Instance.usernameField.text);

            SendTCPData(packet);
        }
    }

    public static void PlayerMovement()
    {
        using (Packet packet = new Packet((int)ClientPackets.playerMovement))
        {
            packet.Write(GameManager.players[Client.instance.myId].transform.position);
            packet.Write(GameManager.players[Client.instance.myId].transform.rotation);

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

    #endregion Packets
}