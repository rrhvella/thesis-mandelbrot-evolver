using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NEATSpacesLibrary.Extensions;

namespace NEATSpacesTests.Extensions
{
    public class ListExtensionsTest
    {
        private const int NUMBER_OF_TRIALS = 10000;
        private const double Z_SCORE = 2.58; //99% confidence level.

        public class TestClass 
        {
            public string Name;
            public double Value;
        }

        public static IEnumerable<TestCaseData> MaxByTestCases 
        {
            get 
            {
                yield return new TestCaseData(new List<TestClass>() { } ).Returns(null);
                yield return new TestCaseData(new List<TestClass>() { new TestClass() {Name = "Test1", Value = 0}} ).Returns("Test1");

                yield return new TestCaseData(new List<TestClass>()  { new TestClass() {Name = "Test1", Value = 20}, 
                                                    new TestClass() {Name = "Test2", Value = 10}} ).Returns("Test1");

                yield return new TestCaseData(new List<TestClass>() { new TestClass() {Name = "Test1", Value = 10}, 
                                                    new TestClass() {Name = "Test2", Value = 20}} ).Returns("Test2");
            }
        }

        [TestCaseSource(typeof(ListExtensionsTest), "MaxByTestCases")]
        public string TestMaxBy(IEnumerable<TestClass> data)
        {
            var max = data.MaxBy(elem => elem.Value);
            return (max != null)? max.Name : null;
        }

        [TestCase(new double[] { 0.5, 0.5 })]
        [TestCase(new double[] { 0.75, 0.25 })]
        [TestCase(new double[] { 0.5 })]
        [TestCase(new double[] { 0 })]
        [TestCase(new double[] {})]
        [TestCase(new double[] { 1, 2, 3, 4, 5, 6 })]
        [TestCase(new double[] { -1, -2, -3, -4, -5, -6 })]
        [TestCase(new double[] { 1, 2, 3, -4, -5, -6 })]
        public void TestRouletteWheelSelection(double[] probabilities)
        {
            var data = (from i in Enumerable.Range(0, NUMBER_OF_TRIALS)
                        group i by Enumerable.Range(0, probabilities.Count()).Cast<int?>()
                             .RouletteWheelSingle(j => probabilities[(int)j]) into results
                        select results).Where(results => results.Key != null)
                            .ToDictionary(results => results.Key, results => results.Count());

            RouletteWheelTest(probabilities, data);
        }

        [TestCase(new double[] { 0.5, 0.5 })]
        [TestCase(new double[] { 0.75, 0.25 })]
        [TestCase(new double[] { 0.5 })]
        [TestCase(new double[] { 0 })]
        [TestCase(new double[] {})]
        [TestCase(new double[] { 1, 2, 3, 4, 5, 6 })]
        [TestCase(new double[] { -1, -2, -3, -4, -5, -6 })]
        [TestCase(new double[] { 1, 2, 3, -4, -5, -6 })]
        public void TestRouletteWheelTake(double[] probabilities)
        {
            var data = (from i in Enumerable.Range(0, probabilities.Count()).Cast<int?>()
                             .RouletteWheelTake(j => probabilities[(int)j],
                                                  NUMBER_OF_TRIALS)
                        group i by i into results
                        select results).Where(results => results.Key != null)
                            .ToDictionary(results => results.Key, results => results.Count());

            RouletteWheelTest(probabilities, data);
        }


        private void RouletteWheelTest(double[] probabilities, Dictionary<int?, int> data) 
        {
            probabilities = probabilities.Where(elem => elem > 0).ToArray();
            var probabilitiesTotal = probabilities.Sum();

            Assert.IsTrue(data.Count > 0 || probabilitiesTotal <= 0);

            foreach (var pair in data)
            {
                var p = probabilities[(int)pair.Key] / probabilitiesTotal;
                var marginOfError = Math.Sqrt((p * (1 - p)) / NUMBER_OF_TRIALS) * Z_SCORE;

                var actualP = (double)pair.Value / NUMBER_OF_TRIALS;

                Assert.LessOrEqual(actualP, p + marginOfError);
                Assert.GreaterOrEqual(actualP, p - marginOfError);
            }
        }

    }
}
