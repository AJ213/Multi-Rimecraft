using System;
using System.Collections.Generic;
using RimecraftServer;
using System.Text;

namespace Noise
{
    public class NoiseGen
    {
        private OpenSimplexNoise noise;

        public NoiseGen(OpenSimplexNoise noise)
        {
            this.noise = noise;
        }

        public double Get2DSimplex(float posX, float posY, float offset, float scale)
        {
            posX += offset;
            posY += offset;
            float chunkScale = Constants.CHUNKSIZE * scale;
            double simplexNoise = noise.Evaluate(posX / chunkScale, posY / chunkScale);
            return simplexNoise;
        }

        public double Get3DSimplex(float posX, float posY, float posZ, float offset, float scale)
        {
            float x = (posX + offset) * scale;
            float y = (posY + offset) * scale;
            float z = (posZ + offset) * scale;

            double simplexNoise = noise.Evaluate(x, y, z);
            return simplexNoise;
        }
    }
}