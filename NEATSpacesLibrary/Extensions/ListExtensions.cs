using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CPPNNEAT.Extensions
{
    /// <summary>
    /// Extends the functionality of objects implementing the IList interface.
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// Shuffles the contents of the list.
        /// </summary>
        public static void Shuffle<T>(this IList<T> self)
        {
            //Fisher-Yates shuffle.
            foreach(var i in Enumerable.Range(0, self.Count - 1)) 
            {
                var j = MathExtensions.RandomInteger(i + 1, self.Count);

                var temp = self[i];
                self[i] = self[j];
                self[j] = temp;
            }
        }

        /// <summary>
        /// Returns a random element from the list.
        /// </summary>
        /// <returns></returns>
        public static T RandomSingle<T>(this IList<T> self)
        {
            return (self.Count == 0)? default(T) : self[MathExtensions.RandomInteger(self.Count)];
        }

        /// <summary>
        /// Yields n random elements from the list.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static IEnumerable<T> RandomTake<T>(this IList<T> self, int n)
        {
            var indexArray = Enumerable.Range(0, self.Count).ToArray();
            indexArray.Shuffle();

            return indexArray.Take(n).Select(elem => self[elem]);
        }

        /// <summary>
        /// Selects and returns a single element from the list using roulette wheel selection.
        /// </summary>
        /// <param name="probabilitySelector">The function which gives the probability for each element.</param>
        /// <returns></returns>
        /// <remarks>
        /// The probability of an element can be any positive real number; the probabilities are treated as
        /// if they were normalised.
        /// </remarks>
        public static T RouletteWheelSingle<T>(this IEnumerable<T> self, Func<T, double> probabilitySelector)
        {
            return self.RouletteWheelTake(probabilitySelector, 1).First();
        }

        /// <summary>
        /// Selects and yields n elements from the list using roulette wheel selection.
        /// </summary>
        /// <param name="probabilitySelector">The function which gives the probability for each element.</param>
        /// <param name="n"></param>
        /// <returns></returns>
        /// <remarks>
        /// The probability of an element can be any positive real number; the probabilities are treated as
        /// if they were normalised.
        /// </remarks>
        public static IEnumerable<T> RouletteWheelTake<T>(this IEnumerable<T> self, 
                                                    Func<T, double> probabilitySelector, int n)
        {
            //Build roulette wheel.
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

            //If the roulette wheel is empty, return default.
            if (rouletteWheel.Count == 0)
            {
                yield return default(T);
            }
            else
            {
                //Otherwise, continue to take several spins of the wheel until n elements have been yielded.
                foreach(var spin in Enumerable.Range(0, n)) 
                {
                    //Generate a random number up to the total probability.
                    var selection = MathExtensions.RandomNumber() * total;

                    //Iterate once through the roulette wheel to find where the selection lands.
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

    /// <summary>
    /// Extends the functionality of objects implementing the IEnumerable interface.
    /// </summary>
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Returns the maximum element in an enumarable, based on the maximum value of a key function.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the enumerable. </typeparam>
        /// <typeparam name="K">The type of the value returned by the key function. </typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T MaxBy<T, K>(this IEnumerable<T> self, Func<T, K> key) where K : IComparable
        {
            //If the enumerable is empty, return default.
            var enumerator = self.GetEnumerator();

            if (!enumerator.MoveNext())
            {
                return default(T);
            }

            //Iterate once through the enumerable to find the maximum based on key.
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
