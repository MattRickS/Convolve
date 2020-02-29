using System;
using System.Drawing;

namespace Gaussian
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 4)
            {
                throw new System.ArgumentException(
                    "Invalid usage, requires 4 args: src dst sigma radius"
                );
            }
            double sigma = Convert.ToDouble(args[2]);
            int radial = Convert.ToInt32(args[3]);

            Convolve convolve = new Convolve(new Bitmap(args[0], true));
            Bitmap blurredBitmap = convolve.Process(Distribution(sigma, radial));
            blurredBitmap.Save(args[1]);
            Console.WriteLine("Done!");
        }

        public static double[,] Distribution(double sigma, int radial)
        {
            int width = radial * 2 + 1;
            double[,] distribution = new double[width, width];
            for (int y = -radial; y <= radial; y++)
            {
                for (int x = -radial; x <= radial; x++)
                {
                    distribution[y + radial, x + radial] = GaussianFilter(sigma, x, y);
                }
            }
            return distribution;
        }

        public static double GaussianFilter(double sigma, int x, int y)
        {

            double pow = (x * x + y * y) / (2 * sigma * sigma);
            return (1.0 / (2 * Math.PI * sigma * sigma)) * Math.Pow(Math.E, -pow);
        }
    }
}