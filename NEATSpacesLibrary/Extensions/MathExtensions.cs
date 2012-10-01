using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NEATSpacesLibrary.Extensions
{
    public static class MathExtensions
    {
        public static double EuclideanDistance(double[] input)
        {
            return Math.Sqrt((from x in input select x*x).Sum());
        }
    }
}
