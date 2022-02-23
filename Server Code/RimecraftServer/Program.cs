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