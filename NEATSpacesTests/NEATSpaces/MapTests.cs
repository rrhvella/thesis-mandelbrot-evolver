using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NEATSpacesLibrary.NEATSpaces;
using System.Drawing;

namespace NEATSpacesTests.NEATSpaces
{
    class MapNodeTests
    {

        public static IEnumerable<TestCaseData> EqualsTestCases 
        {
            get 
            {
                yield return new TestCaseData(0, new MapNode()).Returns(false);
                yield return new TestCaseData(new MapNode(1, 1), new MapNode()).Returns(false);
                yield return new TestCaseData(new MapNode(), new MapNode()).Returns(true);
            }
        }

        [TestCaseSource(typeof(MapNodeTests), "EqualsTestCases")]
        public bool TestEquals(object obj, MapNode node)
        {
            return node.Equals(obj);
        }

        public static IEnumerable<TestCaseData> MinusTestCases 
        {
            get 
            {
                yield return new TestCaseData(new MapNode(), new MapNode()).Returns(new MapNode());
                yield return new TestCaseData(new MapNode(1, 1), new MapNode(-1, -1)).Returns(new MapNode(2, 2));
                yield return new TestCaseData(new MapNode(-1, 0), new MapNode(1, 0)).Returns(new MapNode(-2, 0));
                yield return new TestCaseData(new MapNode(0, 1), new MapNode(0, 2)).Returns(new MapNode(0, -1));
            }
        }

        [TestCaseSource(typeof(MapNodeTests), "MinusTestCases")]
        public MapNode TestMinus(MapNode node1, MapNode node2)
        {
            return node1 - node2;
        }

        public static IEnumerable<TestCaseData> EqualsOperatorTestCases
        {
            get 
            {
                yield return new TestCaseData(new MapNode(1, 1), new MapNode()).Returns(false);
                yield return new TestCaseData(new MapNode(), new MapNode()).Returns(true);
            }
        }

        [TestCaseSource(typeof(MapNodeTests), "EqualsOperatorTestCases")]
        public bool TestEqualsOperator(MapNode node1, MapNode node2)
        {
            return node1 == node2;
        }

        public static IEnumerable<TestCaseData> NotEqualsOperatorTestCases 
        {
            get 
            {
                yield return new TestCaseData(new MapNode(1, 1), new MapNode()).Returns(true);
                yield return new TestCaseData(new MapNode(), new MapNode()).Returns(false);
            }
        }

        [TestCaseSource(typeof(MapNodeTests), "NotEqualsOperatorTestCases")]
        public bool TestNotEqualsOperator(MapNode node1, MapNode node2)
        {
            return node1 != node2;
        }

        public static IEnumerable<TestCaseData> GetHashCodeTestCases 
        {
            get 
            {
                yield return new TestCaseData(new MapNode(2, 2)).Returns("2,2".GetHashCode());
                yield return new TestCaseData(new MapNode()).Returns("0,0".GetHashCode());
                yield return new TestCaseData(new MapNode(int.MaxValue, int.MaxValue)).Returns((int.MaxValue + "," + int.MaxValue).GetHashCode());
                yield return new TestCaseData(new MapNode(2, 2)).Returns((new MapNode(2, 2)).GetHashCode());
            }
        }

        [TestCaseSource(typeof(MapNodeTests), "GetHashCodeTestCases")]
        public int TestGetHashCode(MapNode node)
        {
            return node.GetHashCode();
        }

        public static IEnumerable<TestCaseData> ToStringTestCases
        {
            get
            {
                yield return new TestCaseData(new MapNode(2, 2)).Returns("2,2");
                yield return new TestCaseData(new MapNode()).Returns("0,0");
                yield return new TestCaseData(new MapNode(int.MaxValue, int.MaxValue)).Returns((int.MaxValue + "," + int.MaxValue));
            }
        }

        [TestCaseSource(typeof(MapNodeTests), "ToStringTestCases")]
        public string TestToString(MapNode node)
        {
            return node.ToString();
        }
    }

    public class MapTestData 
    {
        //A normal map with a valid distance to end.
        public Map TestMap1;

        //A map with no path through a mandatory checkpoint.
        public Map TestMap2;
        
        //A map with no path to the end.
        public Map TestMap3;

        private static MapTestData instance;
        public static MapTestData Instance 
        {
            get 
            {
                return instance;
            }
            private set 
            {
                instance = value;
            }
        }

        static MapTestData() 
        {
            Instance = new MapTestData();
        }

        private MapTestData()
        {
            var defaultMapFactory = new Func<Map>(delegate() {
                return new Map(3, 3, new MapNode(0, 0), new MapNode(2, 2), new[] { new MapNode(0, 2) }, 1);
            });

            //Map 1.
            TestMap1 = defaultMapFactory();

            TestMap1[0, 1] = true;
            TestMap1[2, 1] = true;

            //Map 2.
            TestMap2 = defaultMapFactory();

            TestMap1[0, 1] = true;
            TestMap1[1, 1] = true;
            TestMap1[1, 2] = true;

            //Map 3.
            TestMap3 = defaultMapFactory();

            TestMap1[0, 1] = true;
            TestMap1[1, 1] = true;
            TestMap1[2, 1] = true;
        }
    }

    public class MapTests
    {
        public static IEnumerable<TestCaseData> IndexerTestCases
        {
            get
            {
                yield return new TestCaseData(MapTestData.Instance.TestMap1, 0, 1).Returns(true);
                yield return new TestCaseData(MapTestData.Instance.TestMap2, 1, 1).Returns(false);
            }
        }

        [TestCaseSource(typeof(MapTests), "IndexerTestCases")]
        public bool TestIndexer(Map map, int x, int y)
        {
            return map[x, y];
        }

        public static IEnumerable<TestCaseData> DistanceFromStartToEndTestCases
        {
            get
            {
                yield return new TestCaseData(MapTestData.Instance.TestMap1).Returns(5);
                yield return new TestCaseData(MapTestData.Instance.TestMap2).Returns(0);
                yield return new TestCaseData(MapTestData.Instance.TestMap3).Returns(0);
            }
        }

        [TestCaseSource(typeof(MapTests), "DistanceFromStartToEndTestCases")]
        public double TestDistanceFromStartToEnd(Map map)
        {
            return map.DistanceFromStartToEnd;
        }

        public static IEnumerable<TestCaseData> DistanceBetweenTestCases {
            get
            {
                yield return new TestCaseData(MapTestData.Instance.TestMap1, new MapNode(), new MapNode()).Returns(0);
                yield return new TestCaseData(MapTestData.Instance.TestMap1, new MapNode(), new MapNode(1, 0)).Returns(1);
                yield return new TestCaseData(MapTestData.Instance.TestMap1, new MapNode(), new MapNode(0, 1)).Returns(null);
                yield return new TestCaseData(MapTestData.Instance.TestMap1, new MapNode(), new MapNode(-2, -2)).Returns(null);
                yield return new TestCaseData(MapTestData.Instance.TestMap1, new MapNode(), new MapNode(2, 2)).Returns(5);
                yield return new TestCaseData(MapTestData.Instance.TestMap2, new MapNode(), new MapNode(2, 2)).Returns(5);
                yield return new TestCaseData(MapTestData.Instance.TestMap3, new MapNode(), new MapNode(2, 2)).Returns(null);
            }
        }

        [TestCaseSource(typeof(MapTests), "DistanceBetweenTestCases")]
        public double TestDistanceBetween(Map map, MapNode from, MapNode to)
        {
            return map.DistanceFromStartToEnd;
        }

        public static IEnumerable<TestCaseData> ImageTestCases {
        get {
        yield return new TestCaseData(MapTestData.Instance.TestMap1, 0, 0).Returns(Map.START_COLOUR);
        yield return new TestCaseData(MapTestData.Instance.TestMap1, 1, 0).Returns(Map.TILE_COLOUR);
        yield return new TestCaseData(MapTestData.Instance.TestMap1, 2, 0).Returns(Map.TILE_COLOUR);
        yield return new TestCaseData(MapTestData.Instance.TestMap1, 0, 1).Returns(Map.WALL_COLOUR);
        yield return new TestCaseData(MapTestData.Instance.TestMap1, 1, 1).Returns(Map.TILE_COLOUR);
        yield return new TestCaseData(MapTestData.Instance.TestMap1, 2, 1).Returns(Map.WALL_COLOUR);
        yield return new TestCaseData(MapTestData.Instance.TestMap1, 0, 2).Returns(Map.CHECKPOINT_COLOUR);
        yield return new TestCaseData(MapTestData.Instance.TestMap1, 1, 2).Returns(Map.TILE_COLOUR);
        yield return new TestCaseData(MapTestData.Instance.TestMap1, 2, 2).Returns(Map.END_COLOUR);
    }}

        [TestCaseSource(typeof(MapTests), "ImageTestCases")]
        public Color TestImage(Map map, int x, int y)
        {
            return map.Image.GetPixel(x, y);
        }
    }
}
