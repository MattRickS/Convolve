using System;
using System.Drawing;
// using System.Drawing.Image;

namespace Gaussian
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 4)
            {
                throw new System.ArgumentException("Invalid usage: src dst sigma radius");
            }
            double sigma = Convert.ToDouble(args[2]);
            int radial = Convert.ToInt32(args[3]);
            Bitmap bitmap = new Bitmap(args[0], true);
            Gaussian gaussian = new Gaussian(bitmap);
            Bitmap blurredBitmap = gaussian.Process(sigma, radial);
            blurredBitmap.Save(args[1]);
            Console.WriteLine("Done!");
        }
    }
}