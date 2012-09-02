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
    }
}
