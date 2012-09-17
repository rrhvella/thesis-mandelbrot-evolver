using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NEATSpacesLibrary.Extensions
{
    public static class ListExtensions
    {
        private static Random random;

        static ListExtensions()
        {
            random = new Random();
        }

        public static void Shuffle<T>(this IList<T> self)
        {
            foreach(var i in Enumerable.Range(0, self.Count - 1)) 
            {
                var j = random.Next(i + 1, self.Count);

                var temp = self[i];
                self[i] = self[j];
                self[j] = temp;
            }
        }

        public static T RandomSingle<T>(this IList<T> self)
        {
            return (self.Count == 0)? default(T) : self[random.Next(self.Count)];
        }

        public static IEnumerable<T> RandomTake<T>(this IList<T> self, int size)
        {
            var indexArray = Enumerable.Range(0, self.Count).ToArray();
            indexArray.Shuffle();

            return indexArray.Take(size).Select(elem => self[elem]);
        }

        public static T RouletteWheelSingle<T>(this IEnumerable<T> self, Func<T, double> probabilitySelector)
        {
            //Build roulette wheel.
            var rouletteWheel = new Queue<Tuple<T, double>>();
            var total = 0.0;

            foreach (var tuple in self.Select(item => 
                                            Tuple.Create(item, probabilitySelector(item)))
                                     .OrderByDescending(tuple => tuple.Item2))

            {
                if (tuple.Item2 == 0)
                {
                    continue;
                }

                rouletteWheel.Enqueue(Tuple.Create(tuple.Item1, tuple.Item2 + total));
                total += tuple.Item2;
            }

            if (rouletteWheel.Count == 0)
            {
                return default(T);
            }

            //Make selection.
            var selection = random.NextDouble() * total;

            while (rouletteWheel.Peek().Item2 < selection)
            {
                rouletteWheel.Dequeue();
            }

            return rouletteWheel.Dequeue().Item1;
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
