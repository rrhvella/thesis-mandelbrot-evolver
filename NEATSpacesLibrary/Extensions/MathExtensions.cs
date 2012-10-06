using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NEATSpacesLibrary.Extensions
{
    public static class MathExtensions
    {
        private static Random random;

        static MathExtensions()
        {
            random = new Random();
        }

        public static double RandomNumber()
        {
            double result = 0;

            lock (random)
            {
                result = random.NextDouble();
            }

            return result;
        }

        public static int RandomInteger(int from, int to)
        {
            int result = 0;

            lock (random)
            {
                result = random.Next(from, to);
            }

            return result;
        }

        public static int RandomInteger(int max)
        {
            return RandomInteger(0, max);
        }
        
        public static double EuclideanDistance(double x, double y)
        {
            return Math.Sqrt(x*x + y*y);
        }
    }
}
