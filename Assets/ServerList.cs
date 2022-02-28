using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ServerList : MonoBehaviour
{
    private List<ServerListObject> listItem = new List<ServerListObject>();
    [SerializeField] private TMP_InputField ipField;
    [SerializeField] private TMP_InputField portField;
    [SerializeField] private GameObject listObject;
    [SerializeField] private Transform parentToSpawnUnder;

    private void Start()
    {
        int count = 1;
        while (PlayerPrefs.HasKey(count + "ListIP"))
        {
            string ip = PlayerPrefs.GetString(count + "ListIP");
            int port = PlayerPrefs.GetInt(count + "ListPort");
            ServerListObject item = CreateItem();
            item.SetValues(ip, port, this);
            count++;
        }
    }

    private ServerListObject CreateItem()
    {
        GameObject go = Instantiate(listObject, parentToSpawnUnder);
        ServerListObject item = go.GetComponent<ServerListObject>();
        AddItem(item);
        return item;
    }

    public void CreateAndAddItem()
    {
        string ipVal = ipField.text;
        if (ipVal.Length < 1)
        {
            ipVal = "127.0.0.1";
        }
        int port = int.Parse(portField.text);

        for (int i = 0; i < listItem.Count; i++)
        {
            if (listItem[i].IP == ipVal && listItem[i].Port == port)
            {
                return;
            }
        }

        ServerListObject item = CreateItem();
        item.SetValues(ipVal, port, this);

        int id = listItem.Count;
        string keyIP = id.ToString() + "ListIP";
        PlayerPrefs.SetString(keyIP, item.IP);
        string keyPort = id.ToString() + "ListPort";
        PlayerPrefs.SetInt(keyPort, item.Port);
        PlayerPrefs.Save();
    }

    private void AddItem(ServerListObject item)
    {
        listItem.Add(item);
        item.QueueCheckConnection();
    }

    public void RemoveItem(ServerListObject item)
    {
        for (int i = listItem.IndexOf(item); i < listItem.Count - 1; i++)
        {
            SwapNext(item);
        }
        int id = listItem.Count;
        string keyIP = id.ToString() + "ListIP";
        PlayerPrefs.DeleteKey(keyIP);
        string keyPort = id.ToString() + "ListPort";
        PlayerPrefs.DeleteKey(keyPort);
        PlayerPrefs.Save();
        listItem.Remove(item);
        Destroy(item.gameObject);
    }

    private void SwapNext(ServerListObject item)
    {
        int id = listItem.IndexOf(item) + 1;
        ServerListObject nextObj = listItem[id];
        string keyIP = id.ToString() + "ListIP";
        string keyPort = id.ToString() + "ListPort";

        PlayerPrefs.SetString(keyIP, nextObj.IP);
        PlayerPrefs.SetInt(keyPort, nextObj.Port);
        listItem[id - 1] = nextObj;
        listItem[id] = item;
    }

    public void UpdateAll()
    {
        for (int i = 0; i < listItem.Count; i++)
        {
            listItem[i].QueueCheckConnection();
        }
    }
}