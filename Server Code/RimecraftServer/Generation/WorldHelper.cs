using System;
using System.Numerics;

namespace RimecraftServer
{
    internal class WorldHelper
    {
        public static Vector3 GetChunkCoordFromPosition(Vector3 globalPos)
        {
            Vector3 result;
            result.X = (int)Math.Floor(globalPos.X / Constants.CHUNKSIZE);
            result.Y = (int)Math.Floor(globalPos.Y / Constants.CHUNKSIZE);
            result.Z = (int)Math.Floor(globalPos.Z / Constants.CHUNKSIZE);

            return result;
        }

        public static Vector3 GetVoxelLocalPositionInChunk(Vector3 globalPos)
        {
            Vector3 result;
            result.X = Mod((int)Math.Floor(globalPos.X), Constants.CHUNKSIZE);
            result.Y = Mod((int)Math.Floor(globalPos.Y), Constants.CHUNKSIZE);
            result.Z = Mod((int)Math.Floor(globalPos.Z), Constants.CHUNKSIZE);

            return result;
        }

        public static Vector3 GetVoxelGlobalPositionFromChunk(Vector3 localPos, Vector3 coord)
        {
            Vector3 result;
            result.X = (int)Math.Floor(localPos.X) + (Constants.CHUNKSIZE * coord.X);
            result.Y = (int)Math.Floor(localPos.Y) + (Constants.CHUNKSIZE * coord.Y);
            result.Z = (int)Math.Floor(localPos.Z) + (Constants.CHUNKSIZE * coord.Z);
            return result;
        }

        public static bool IsVoxelGlobalPositionInChunk(Vector3 globalPos, Vector3 coord)
        {
            return GetChunkCoordFromPosition(globalPos).Equals(coord);
        }

        public static bool IsInRange(int value, int length)
        {
            return (value >= 0 && value < length);
        }

        public static bool IsInRange(Vector3 value, int length)
        {
            return (IsInRange((int)value.X, length) && IsInRange((int)value.Y, length) && IsInRange((int)value.Z, length));
        }

        public static int Mod(int a, int n)
        {
            return ((a % n) + n) % n;
        }
    }
}