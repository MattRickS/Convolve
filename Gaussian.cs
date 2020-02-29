using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Gaussian
{
    class Gaussian
    {
        private Bitmap bitmap;

        public Gaussian(Bitmap bitmap)
        {
            this.bitmap = bitmap;
        }

        public Bitmap Process(double sigma, int radial)
        {
            BitmapData bmpData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite,
                bitmap.PixelFormat
            );
            int bytes = Math.Abs(bmpData.Stride) * bmpData.Height;
            byte[] src = new byte[bytes];
            Marshal.Copy(bmpData.Scan0, src, 0, bytes);
            bitmap.UnlockBits(bmpData);

            int numChannels = bmpData.Stride / bitmap.Width;
            byte[] dst = new byte[bytes];

            double[] distribution = Distribution(sigma, radial);

            for (int x = 0; x < bmpData.Stride; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    Convolve(src, dst, distribution, bmpData.Stride, x, y, numChannels, radial);
                }
            }

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

        public void Convolve(byte[] src, byte[] dst, double[] distribution, int stride, int x, int y, int numChan, int radial)
        {
            double value = 0.0;
            double totalWeight = 0.0;
            int distributionWidth = radial * 2 + 1;
            for (int offX = -radial; offX <= radial; offX++)
            {
                for (int offY = -radial; offY <= radial; offY++)
                {
                    double weight = distribution[distributionWidth * (offY + radial) + offX + radial];
                    // Convolve should only target the same channel on each pixel
                    int targetX = x + offX * numChan;
                    int targetY = y + offY;
                    if (targetX >= 0 && targetX < stride && targetY >= 0 && targetY < bitmap.Height)
                    {
                        value += weight * src[targetY * stride + targetX];
                        totalWeight += weight;
                    }
                }
            }
            dst[y * stride + x] = (byte)(value / totalWeight);
        }

        public double[] Distribution(double sigma, int radial)
        {
            int width = radial * 2 + 1;
            double[] distribution = new double[width * width];
            for (int y = -radial; y <= radial; y++)
            {
                for (int x = -radial; x <= radial; x++)
                {
                    distribution[(y + radial) * width + x + radial] = GaussianFilter(sigma, x, y);
                }
            }
            return distribution;
        }

        public double GaussianFilter(double sigma, int x)
        {
            double pow = (x * x) / (2 * sigma * sigma);
            return (1.0 / Math.Sqrt(2 * Math.PI * sigma * sigma)) * Math.Pow(Math.E, -pow);
        }

        public double GaussianFilter(double sigma, int x, int y)
        {

            double pow = (x * x + y * y) / (2 * sigma * sigma);
            return (1.0 / (2 * Math.PI * sigma * sigma)) * Math.Pow(Math.E, -pow);
        }
    }
}