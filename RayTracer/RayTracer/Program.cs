using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RayTracer
{
    class Program
    {
        const int width = 1920;
        const int height = 1080;
        static void Main(string[] args)
        {
            byte[] frontBuffer = new byte[width * height * 4];
            float[] backBuffer = new float[width * height * 4];

            int totalRayCount = 0;
            RayCaster rc = new RayCaster(ref backBuffer, 20f,  width, height, ref totalRayCount);

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int k = i * width * 4 + j * 4;
                    frontBuffer[k + 0] = (byte)(Math.Pow(backBuffer[k + 2], 2.2) * 255);
                    frontBuffer[k + 1] = (byte)(Math.Pow(backBuffer[k + 1], 2.2) * 255);
                    frontBuffer[k + 2] = (byte)(Math.Pow(backBuffer[k + 0], 2.2) * 255);
                    frontBuffer[k + 3] = 255;
                }
            }

            byte[] tgaHeader =
            {
                0, 0, 2, /* true color */
                0, 0, 0, 0,
                0,
                0, 0, 0, 0,
                (byte)(width&0xff), (byte)((width&0xff00) >> 8),
                (byte)(height&0xff), (byte)((height&0xff00) >> 8),
                32 /*bpp*/, 0
            };

            using (BinaryWriter writer = new BinaryWriter(new FileStream("output.tga", FileMode.Create)))
            {
                writer.Write(tgaHeader);
                writer.Write(frontBuffer);
            }
        }
    }
}
