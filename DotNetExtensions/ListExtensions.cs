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
using System.Collections.Generic;
using System.Linq;

namespace DotNetExtensions
{
    public static class ListExtensions
    {
        public static void Shuffle<T>(this IList<T> self)
        {
            foreach (var i in Enumerable.Range(0, self.Count - 1))
            {
                var j = MathExtensions.RandomInteger(i + 1, self.Count);

                var temp = self[i];
                self[i] = self[j];
                self[j] = temp;
            }
        }

        public static T RandomSingle<T>(this IList<T> self)
        {
            return (self.Count == 0) ? default(T) : self[MathExtensions.RandomInteger(self.Count)];
        }

        public static IEnumerable<T> RandomTake<T>(this IList<T> self, int n)
        {
            var indexArray = Enumerable.Range(0, self.Count).ToArray();
            indexArray.Shuffle();

            return indexArray.Take(n).Select(elem => self[elem]);
        }

        public static T RouletteWheelSingle<T>(this IEnumerable<T> self, Func<T, double> probabilitySelector)
        {
            return self.RouletteWheelTake(probabilitySelector, 1).First();
        }

        public static IEnumerable<T> RouletteWheelTake<T>(this IEnumerable<T> self,
Func<T, double> probabilitySelector, int n)
        {
            var rouletteWheel = new List<Tuple<T, double>>();
            var total = 0.0;

            foreach (var tuple in self.Select(item =>
                                            Tuple.Create(item, probabilitySelector(item)))
                                     .OrderByDescending(tuple => tuple.Item2))
            {
                if (tuple.Item2 <= 0)
                {
                    continue;
                }

                rouletteWheel.Add(Tuple.Create(tuple.Item1, tuple.Item2 + total));
                total += tuple.Item2;
            }

            if (rouletteWheel.Count == 0)
            {
                yield return default(T);
            }
            else
            {
                foreach (var spin in Enumerable.Range(0, n))
                {
                    var selection = MathExtensions.RandomNumber() * total;

                    var i = 0;
                    while (rouletteWheel[i].Item2 < selection)
                    {
                        i++;
                    }

                    yield return rouletteWheel[i].Item1;
                }
            }
        }
    }

    public static class IEnumerableExtensions
    {
        public static T MaxBy<T, K>(this IEnumerable<T> self, Func<T, K> key) where K : IComparable
        {
            var enumerator = self.GetEnumerator();

            if (!enumerator.MoveNext())
            {
                return default(T);
            }

            var maxValue = enumerator.Current;
            var maxKey = key(enumerator.Current);

            while (enumerator.MoveNext())
            {
                var currKey = key(enumerator.Current);

                if (currKey.CompareTo(maxKey) >= 0)
                {
                    maxKey = currKey;
                    maxValue = enumerator.Current;
                }
            }

            return maxValue;
        }
    }
}