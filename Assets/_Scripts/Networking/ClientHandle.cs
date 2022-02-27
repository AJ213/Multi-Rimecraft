using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using Unity.Mathematics;

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

    public static void SetAddChunk(Packet packet)
    {
        ChunkData data = packet.ReadChunkData();
        RimecraftWorld.worldData.SetChunk(data);
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
        if (GameManager.players.ContainsKey(id))
        {
            GameManager.players[id].transform.position = position;
        }
    }

    public static void PlayerRotation(Packet packet)
    {
        int id = packet.ReadInt();
        Quaternion rotation = packet.ReadQuaternion();
        if (GameManager.players.ContainsKey(id))
        {
            GameManager.players[id].transform.rotation = rotation;
        }
    }

    public static void ModifiedVoxel(Packet packet)
    {
        int3 globalPosition = packet.ReadVector3().FloorToInt3();
        ushort id = packet.ReadUShort();

        ChunkData data = RimecraftWorld.worldData.GetChunk(globalPosition);
        if (data == null)
        {
            return;
        }

        int3 localPosition = WorldHelper.GetVoxelLocalPositionInChunk(globalPosition);

        bool updatedVoxel = data.ModifyVoxel(localPosition, id);
        if (updatedVoxel)
        {
            WorldData.UpdateSorroundingVoxels(globalPosition);
        }
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
    }
}