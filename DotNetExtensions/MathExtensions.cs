using System;
using System.Numerics;

namespace DotNetExtensions
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
            return Math.Sqrt(x * x + y * y);
        }

        public static int AbsMod(int n, int b)
        {
            n %= b;

            if (n < 0)
            {
                n += b;
            }

            return n;
        }

        public static Complex ComplexRandom()
        {
            return ComplexRandom(1);
        }

        public static Complex ComplexRandom(double from, double to)
        {
            var r = RandomNumber(from, to);
            var a = RandomNumber();

            return Complex.FromPolarCoordinates(r, a * Math.PI);
        }

        public static Complex ComplexRandom(double max)
        {
            return ComplexRandom(0, max);
        }

        public static double RandomNumber(double from, double to)
        {
            return from + (to - from) * RandomNumber();
        }
    }
}