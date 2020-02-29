using System;
using System.Drawing;
// using System.Drawing.Image;

namespace Gaussian
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                throw new System.ArgumentException("Requires at least 3 arguments");
            }
            int radial = Int32.Parse(args[2]);
            Bitmap bitmap = new Bitmap(args[0], true);
            Gaussian gaussian = new Gaussian(bitmap);
            Bitmap blurredBitmap = gaussian.Process(radial);
            blurredBitmap.Save(args[1]);
            Console.WriteLine("Done!");
        }
    }
}