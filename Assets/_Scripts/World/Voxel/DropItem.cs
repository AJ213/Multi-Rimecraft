using UnityEngine;
using System.Collections.Generic;
using System;

public class DropItem : MonoBehaviour
{
    [SerializeField] private ItemStack items;
    [SerializeField] private float pickupProximity = 2;
    [SerializeField] private static GameObject player = default;
    private SphericalRigidbody rb;
    [SerializeField] private float decayTime = 60;
    [SerializeField] private Material[] materials = default;

    public static Dictionary<string, DropItem> droppedItems = new Dictionary<string, DropItem>();
    private string uuid;

    private void Awake()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        rb = GetComponent<SphericalRigidbody>();
        Destroy(this.gameObject, decayTime);
    }

    public void SetID(string id)
    {
        uuid = id;
    }

    public static void DestroyItemWithID(string uuid)
    {
        if (droppedItems.ContainsKey(uuid))
        {
            Destroy(droppedItems[uuid].gameObject);
        }
    }

    public static void TrySpawnDropItem(ushort id, Vector3 position, string uuid = "")
    {
        GameObject[] sounds = RimecraftWorld.Instance.sounds;
        if (id == 2)
        {
            Instantiate(sounds[0], position, Quaternion.identity);
        }
        else if (id == 1)
        {
            Instantiate(sounds[1], position, Quaternion.identity);
        }
        else
        {
            Instantiate(sounds[2], position, Quaternion.identity);
        }

        if (id != 0)
        {
            SpawnDropItem(id, position, uuid);
        }
    }

    public static void SpawnDropItem(ushort id, Vector3 position, string uuid = "")
    {
        GameObject droppedBlock = (GameObject)Instantiate(Resources.Load("DroppedItem"), position, Quaternion.identity);
        DropItem dropItem = droppedBlock.GetComponent<DropItem>();
        dropItem.SetItemStack(id, 1);
        if (uuid.Equals(""))
        {
            uuid = Guid.NewGuid().ToString();
            ClientSend.DroppedItem(position, id, uuid);
        }

        dropItem.SetID(uuid);
        droppedItems.Add(uuid, dropItem);
    }

    private void FixedUpdate()
    {
        rb.CalculateVelocity(Vector3.zero, 5);
        transform.Rotate(10 * Vector3.up * Time.fixedDeltaTime);
        if (Vector3.Distance(player.transform.position, this.transform.position) <= pickupProximity)
        {
            PickupItem();
        }
    }

    public void SetItemStack(ushort id, int amount)
    {
        if (id == 0)
        {
            Destroy(this.gameObject);
        }
        else
        {
            items = new ItemStack(id, amount);
            GetComponent<MeshRenderer>().material = materials[items.id];
        }
    }

    private void OnDestroy()
    {
        droppedItems.Remove(uuid);
    }

    private void PickupItem()
    {
        bool success = IGUIManager.Instance.GetInventory.TryAdd(ref items);
        if (success)
        {
            Destroy(this.gameObject);
            ClientSend.PickupItem(uuid);
        }
    }
}