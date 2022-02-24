using System.Diagnostics;
using System;
using UnityEngine;
using TMPro;

public class RunServer : MonoBehaviour
{
    [SerializeField] private TMP_InputField portField;
    [SerializeField] private TMP_InputField playerField;
    [SerializeField] private TMP_InputField seedField;

    [ContextMenu("Run server")]
    public void ServerRun()
    {
        StartRimecraftServer();
    }

    public void ArgServerRun()
    {
        string arguments = portField.text + " " + playerField.text + " " + seedField.text;
        StartRimecraftServer(arguments);
    }

    public static void StartRimecraftServer(string arguments = "")
    {
        UnityEngine.Debug.Log("running file " + Environment.CurrentDirectory + @"\Server\RimecraftServer.exe" + " args: " + arguments);
        Process.Start(Environment.CurrentDirectory + @"\Server\RimecraftServer.exe", arguments);
    }
}