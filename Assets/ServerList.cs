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
        ServerListObject item = CreateItem();
        string ipVal = ipField.text;
        if (ipVal.Length < 2)
        {
            ipVal = "127.0.0.1";
        }
        int port = int.Parse(portField.text);
        item.SetValues(ipVal, port, this);

        int id = listItem.Count;
        string keyIP = id.ToString() + "ListIP";
        PlayerPrefs.SetString(keyIP, item.IP);
        string keyPort = id.ToString() + "ListPort";
        PlayerPrefs.SetInt(keyPort, item.Port);
        PlayerPrefs.Save();
        Debug.Log("saved and added");
    }

    private void AddItem(ServerListObject item)
    {
        listItem.Add(item);
        item.QueueCheckConnection();
    }

    public void RemoveItem(ServerListObject item)
    {
        int id = listItem.Count;
        string keyIP = id.ToString() + "ListIP";
        PlayerPrefs.DeleteKey(keyIP);
        string keyPort = id.ToString() + "ListPort";
        PlayerPrefs.DeleteKey(keyPort);

        listItem.Remove(item);
        Destroy(item.gameObject);
    }

    public void UpdateAll()
    {
        for (int i = 0; i < listItem.Count; i++)
        {
            listItem[i].QueueCheckConnection();
        }
    }
}