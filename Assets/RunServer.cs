using System.Diagnostics;
using System;
using UnityEngine;

public class RunServer : MonoBehaviour
{
    [ContextMenu("Run server")]
    public void RunFile()
    {
        UnityEngine.Debug.Log("running file " + Environment.CurrentDirectory + @"\Build\Server\RimecraftServer.exe");
        Process.Start(Environment.CurrentDirectory + @"\Build\Server\RimecraftServer.exe");
    }
}