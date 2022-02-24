using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;

public class ClientHandle : MonoBehaviour
{
    public static void Welcome(Packet packet)
    {
        string msg = packet.ReadString();
        int myId = packet.ReadInt();

        Debug.Log($"Mesage from server: {msg}");
        Client.Instance.myId = myId;
        ClientSend.WelcomeReceived();

        Client.Instance.udp.Connect(((IPEndPoint)Client.Instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    public static void SetChunk(Packet packet)
    {
        ChunkData data = packet.ReadChunkData();
        WorldData.SetChunk(data);
    }

    public static void SpawnPlayer(Packet packet)
    {
        int id = packet.ReadInt();
        string username = packet.ReadString();
        Vector3 position = packet.ReadVector3();
        Quaternion rotation = packet.ReadQuaternion();

        GameManager.Instance.SpawnPlayer(id, username, position, rotation);
    }

    public static void PlayerPosition(Packet packet)
    {
        int id = packet.ReadInt();
        Vector3 position = packet.ReadVector3();

        GameManager.players[id].transform.position = position;
    }

    public static void PlayerRotation(Packet packet)
    {
        int id = packet.ReadInt();
        Quaternion rotation = packet.ReadQuaternion();

        GameManager.players[id].transform.rotation = rotation;
    }

    public static void ModifiedVoxel(Packet packet)
    {
        Vector3 globalPosition = packet.ReadVector3();
        ushort id = packet.ReadUShort();

        WorldData.UpdateSorroundingVoxels(globalPosition.FloorToInt3());
    }

    public static void DroppedItem(Packet packet)
    {
        Vector3 globalPosition = packet.ReadVector3();
        ushort id = packet.ReadUShort();
        string uuid = packet.ReadString();

        DropItem.TrySpawnDropItem(id, globalPosition, uuid);
    }

    public static void SpawnProjectile(Packet packet)
    {
        Vector3 position = packet.ReadVector3();
        Vector3 direction = packet.ReadVector3();
        Projectile.SpawnProjectile(position, direction, false);
    }

    public static void PickupItem(Packet packet)
    {
        string uuid = packet.ReadString();
        DropItem.DestroyItemWithID(uuid);
        Debug.Log("destroying item with uuid " + uuid);
    }
}