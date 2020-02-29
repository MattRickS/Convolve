using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;


namespace Gaussian
{
    class Convolve
    {
        private Bitmap bitmap;

        public Convolve(Bitmap bitmap)
        {
            this.bitmap = bitmap;
        }

        public Bitmap Process(double[,] distribution)
        {
            // Extract the bitmap to a 1D byte array, determine the number of channels
            BitmapData bmpData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite,
                bitmap.PixelFormat
            );
            int stride = bmpData.Stride;
            int numChannels = bmpData.Stride / bitmap.Width;
            int bytes = Math.Abs(bmpData.Stride) * bmpData.Height;
            byte[] src = new byte[bytes];
            byte[] dst = new byte[bytes];
            Marshal.Copy(bmpData.Scan0, src, 0, bytes);
            bitmap.UnlockBits(bmpData);

            // Run the convolution on each channel of each pixel
            int height = bitmap.Height;
            Parallel.For(0, height, y =>
                {
                    for (int x = 0; x < stride; x++)
                    {
                        Apply(src, dst, distribution, stride, height, numChannels, x, y);
                    }
                }
            );

            // Copy the output into a new bitmap
            Bitmap output = new Bitmap(bitmap.Width, bitmap.Height);
            bmpData = output.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite,
                bitmap.PixelFormat
            );
            Marshal.Copy(dst, 0, bmpData.Scan0, dst.Length);
            output.UnlockBits(bmpData);

            return output;
        }

        private void Apply(byte[] src, byte[] dst, double[,] distribution, int stride, int height, int numChan, int x, int y)
        {
            int dHeight = distribution.GetLength(0);
            int dWidth = distribution.GetLength(1);
            int midY = (dHeight - 1) / 2;
            int midX = (dWidth - 1) / 2;

            double value = 0.0;
            double totalWeight = 0.0;
            for (int offY = 0; offY < dHeight; offY++)
            {
                for (int offX = 0; offX < dWidth; offX++)
                {
                    double weight = distribution[offY, offX];
                    // Convolve should only target the same channel on each pixel
                    int targetX = x + (offX - midX) * numChan;
                    int targetY = y + (offY - midY);
                    if (targetX >= 0 && targetX < stride && targetY >= 0 && targetY < height)
                    {
                        value += weight * src[targetY * stride + targetX];
                        totalWeight += weight;
                    }
                }
            }
            dst[y * stride + x] = (byte)(value / totalWeight);
        }
    }
}