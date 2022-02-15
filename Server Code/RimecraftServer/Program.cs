using System;
using System.Threading;
using System.Numerics;
using Noise;

namespace RimecraftServer
{
    internal class Program
    {
        private static bool isRunning = false;
        public static WorldData worldData;

        private static void Main(string[] args)
        {
            Console.Title = "Rimecraft Server";
            isRunning = true;

            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();

            int seed = 0;
            worldData = new WorldData(seed);
            for (int x = -1; x < 3; x++)
            {
                for (int y = -1; y < 3; y++)
                {
                    for (int z = -1; z < 3; z++)
                    {
                        worldData.LoadChunk(new Vector3(x, y, z));
                    }
                }
            }

            Server.Start(50, 26950);
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