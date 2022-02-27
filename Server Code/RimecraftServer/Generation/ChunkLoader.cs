using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading;

namespace RimecraftServer
{
    internal class ChunkLoader
    {
        public static void CheckLoadDistance(Player player)
        {
            if (player.coord.Equals(player.lastCoord))
            {
                return;
            }

            player.UpdateLastCoord();
            Vector3 coord = player.coord;
            int loadDistance = player.loadDistance;
            int size = 8 * loadDistance * loadDistance * loadDistance;
            Queue<Vector3> loadChunks = new Queue<Vector3>(size);
            Queue<Vector3> chunksRequested = new Queue<Vector3>(size);
            for (int x = (int)coord.X - loadDistance; x < (int)coord.X + loadDistance; x++)
            {
                for (int y = (int)coord.Y - loadDistance; y < (int)coord.Y + loadDistance; y++)
                {
                    for (int z = (int)coord.Z - loadDistance; z < (int)coord.Z + loadDistance; z++)
                    {
                        Vector3 location = new Vector3(x, y, z);
                        if (!player.loadedChunks.Contains(location))
                        {
                            player.loadedChunks.Add(location);
                            // assumed that any loaded chunks we want to send to the player,
                            // otherwise just send the chunks if we have them
                            if (!Program.worldData.Chunks.ContainsKey(location))
                            {
                                loadChunks.Enqueue(location);
                            }
                            else
                            {
                                chunksRequested.Enqueue(location);
                            }
                        }
                    }
                }
            }

            while (chunksRequested.Count > 0)
            {
                Vector3 requestCoord = chunksRequested.Dequeue();
                ServerSend.SendChunk(player.id, Program.worldData.RequestChunk(requestCoord));
            }
            while (loadChunks.Count > 0)
            {
                Vector3 loadChunkCoord = loadChunks.Dequeue();
                ThreadPool.QueueUserWorkItem(LoadChunks, new object[] { player.id, loadChunkCoord });
            }
        }

        private static void LoadChunks(object state)
        {
            object[] array = state as object[];
            int clientID = Convert.ToInt32(array[0]);
            Vector3 chunkRequested = (Vector3)array[1];

            Program.worldData.LoadChunk(chunkRequested, clientID);
        }
    }
}