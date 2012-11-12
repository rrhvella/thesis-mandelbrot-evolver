using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NEATSpacesLibrary.NEATSpaces
{
    public class PatternMatchingConstants
    {
        public static readonly int[] TARGET_PATTERN = 
                    Enumerable.Range(0, 16).Select(i => (Math.Sin(40 * Math.Sin(2 * i / 16.0) + 1) + Math.Tanh(i / 16.0) >= 0)? 1 : 0).ToArray() ;
    }
}
