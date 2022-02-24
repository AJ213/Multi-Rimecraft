using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace RimecraftServer
{
    internal class Program
    {
        private static bool isRunning = false;
        public static WorldData worldData;

        [STAThread]
        private static void Main(string[] args)
        {
            Console.Title = "Rimecraft Server";
            isRunning = true;

            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();

            int port = 26950;
            int playerCount = 50;
            int seed = 0;

            if (args.Length == 3)
            {
                port = int.Parse(args[0]);
                playerCount = int.Parse(args[1]);
                seed = args[2].GetHashCode();
            }
            else if (args.Length == 0)
            {
                port = 26950;
                playerCount = 50;
                seed = 0;
            }
            else
            {
                Console.WriteLine("incorrect usage, usage: port playerCount seed");
            }

            string hostName = Dns.GetHostName();

            Console.WriteLine(hostName + ": " + GetLocalIP() + "::" + port);
            Console.WriteLine("Max Player Count: " + playerCount);
            Console.WriteLine("Generating world with seed: " + seed);

            worldData = new WorldData(seed);

            Server.Start(playerCount, port);
        }

        private static string GetLocalIP()
        {
            string localIP;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                localIP = endPoint.Address.ToString();
            }
            return localIP;
        }

        private static void MainThread()
        {
            Console.WriteLine($"Main thread started. Running at {Constants.TICKS_PER_SEC} ticks per second");
            DateTime nextLoop = DateTime.Now;

            while (isRunning)
            {
                while (nextLoop < DateTime.Now)
                {
                    GameLogic.Update();

                    nextLoop = nextLoop.AddMilliseconds(Constants.MS_PER_TICK);
                    if (nextLoop > DateTime.Now)
                    {
                        Thread.Sleep(nextLoop - DateTime.Now);
                    }
                }
            }
        }
    }
}