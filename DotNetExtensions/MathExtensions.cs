/*
Copyright (c) 2013, robert.r.h.vella@gmail.com
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met: 

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer. 
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution. 

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

The views and conclusions contained in the software and documentation are those
of the authors and should not be interpreted as representing official policies, 
either expressed or implied, of the FreeBSD Project.
*/

﻿using System;
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