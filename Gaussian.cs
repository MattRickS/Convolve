using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Gaussian
{
    class Gaussian
    {
        private Bitmap bitmap;
        private double sigma;

        public Gaussian(Bitmap bitmap)
        {
            this.bitmap = bitmap;
            this.sigma = 3.0;
        }

        public Bitmap Process(int radial)
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

            double[] distribution = Distribution(radial);

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

        public void Convolve(byte[] src, byte[] dst, double[] distribution, int stride, int currX, int currY, int numChan, int radial)
        {
            double value = 0.0;
            double totalWeight = 0.0;
            int distributionWidth = radial * 2 + 1;
            for (int offX = -radial; offX <= radial; offX++)
            {
                for (int offY = -radial; offY <= radial; offY++)
                {
                    double weight = distribution[distributionWidth * (offY + radial) + offX + radial];
                    int targetX = currX + offX * numChan;
                    int targetY = currY + offY;
                    if (targetX >= 0 && targetX < stride && targetY >= 0 && targetY < bitmap.Height)
                    {
                        // Only targets first channel - would have to iterate over all channels then to get each one
                        value += weight * src[targetY * stride + targetX];
                        totalWeight += weight;
                    }
                }
            }
            int pixel = currY * stride + currX;
            // Console.WriteLine($"Writing {currX}, {currY} = {pixel}");
            dst[pixel] = (byte)(value / totalWeight);
        }

        public double[] Distribution(int radial)
        {
            int width = radial * 2 + 1;
            double[] distribution = new double[width * width];
            for (int y = -radial; y <= radial; y++)
            {
                for (int x = -radial; x <= radial; x++)
                {
                    distribution[(y + radial) * width + x + radial] = GaussianFilter(x, y);
                }
            }
            return distribution;
        }

        public double GaussianFilter(int x)
        {
            double pow = (x * x) / (2 * sigma * sigma);
            return (1.0 / Math.Sqrt(2 * Math.PI * sigma * sigma)) * Math.Pow(Math.E, -pow);
        }

        public double GaussianFilter(int x, int y)
        {

            double pow = (x * x + y * y) / (2 * sigma * sigma);
            return (1.0 / (2 * Math.PI * sigma * sigma)) * Math.Pow(Math.E, -pow);
        }
    }
}