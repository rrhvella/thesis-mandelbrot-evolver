using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NEATSpacesLibrary.Extensions
{
    public static class MathExtensions
    {
        public static double EuclideanDistance(double x, double y)
        {
            return Math.Sqrt(x*x + y*y);
        }
    }
}
