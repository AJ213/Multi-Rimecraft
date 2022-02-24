using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using TMPro;
using UnityEngine.UI;
using System.Threading;

public class ServerListObject : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image canConnectImage;
    private ServerList serverList;

    private bool connectWhenPossible;
    private int _canConnect = 0;
    private int _waitingForResult = 0;
    public bool ThreadSafeCanConnect
    {
        get { return (Interlocked.CompareExchange(ref _canConnect, 1, 1) == 1); }
        set
        {
            if (value) Interlocked.CompareExchange(ref _canConnect, 1, 0);
            else Interlocked.CompareExchange(ref _canConnect, 0, 1);
        }
    }
    public bool ThreadSafeWaiting
    {
        get { return (Interlocked.CompareExchange(ref _waitingForResult, 1, 1) == 1); }
        set
        {
            if (value) Interlocked.CompareExchange(ref _waitingForResult, 1, 0);
            else Interlocked.CompareExchange(ref _waitingForResult, 0, 1);
        }
    }

    private string ip = "127.0.0.1";
    private int port = 26950;
    private Client currentClient;
    public string IP => ip;
    public int Port => port;

    public void SetValues(string ip, int port, ServerList list)
    {
        this.port = port;
        this.ip = ip;
        text.text = ip + "::" + port.ToString();
        this.serverList = list;
    }

    public void SetupClientAndConnect()
    {
        currentClient = Client.Instance;
        currentClient.ip = ip;
        currentClient.port = port;
        QueueCheckConnection();
        connectWhenPossible = true;
    }

    private void UpdateImage()
    {
        if (ThreadSafeWaiting)
        {
            canConnectImage.color = Color.yellow;
            return;
        }
        if (ThreadSafeCanConnect)
        {
            canConnectImage.color = Color.green;
        }
        else
        {
            canConnectImage.color = Color.red;
        }
    }

    private void FixedUpdate()
    {
        UpdateImage();
        if (connectWhenPossible && ThreadSafeCanConnect)
        {
            currentClient.ConnectToServer();
        }
        if (!ThreadSafeWaiting)
        {
            connectWhenPossible = false;
        }
    }

    public void DeleteSelf()
    {
        serverList.RemoveItem(this);
    }

    public void QueueCheckConnection()
    {
        canConnectImage.color = Color.yellow;
        ThreadPool.QueueUserWorkItem(new WaitCallback(CheckConnect));
    }

    private void CheckConnect(object _)
    {
        IPAddress IP;
        ThreadSafeWaiting = true;
        if (IPAddress.TryParse(ip, out IP))
        {
            Socket s = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream,
            ProtocolType.Tcp);

            try
            {
                s.Connect(IP, port);
                ThreadSafeCanConnect = true;
                s.Disconnect(false);
            }
            catch
            {
                ThreadSafeCanConnect = false;
            }
        }
        ThreadSafeWaiting = false;
    }
}