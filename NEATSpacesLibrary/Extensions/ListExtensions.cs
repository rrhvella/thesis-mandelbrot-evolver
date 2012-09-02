using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NEATSpacesLibrary.Extensions
{
    public static class ListExtensions
    {
        public static void Shuffle<T>(this IList<T> self)
        {
            var random = new Random();

            foreach(var i in Enumerable.Range(0, self.Count - 1)) 
            {
                var j = random.Next(i + 1, self.Count);

                var temp = self[i];
                self[i] = self[j];
                self[j] = temp;
            }
        }

        public static IEnumerable<T> RandomTake<T>(this IList<T> self, int size)
        {
            var indexArray = Enumerable.Range(0, self.Count).ToArray();
            indexArray.Shuffle();

            return indexArray.Take(size).Select(elem => self[elem]);
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
