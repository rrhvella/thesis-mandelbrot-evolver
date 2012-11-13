using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;

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

        public static int AbsMod(int n, int b)
        {
            n %= b;

            if(n < 0) 
            {
                n += b;     
            }

            return n;
        }

        public static Complex ComplexRandom()
        {
            return ComplexRandom(0, 1);
        }

        public static Complex ComplexRandom(double from, double to)
        {
            var range = (to - from);
            var r = from + range * RandomNumber();
            var a = from + range * RandomNumber();

            return Complex.FromPolarCoordinates(r, a * Math.PI);
        }
    }
}
