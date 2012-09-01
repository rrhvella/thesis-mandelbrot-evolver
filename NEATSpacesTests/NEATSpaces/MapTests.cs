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

        [TestCase(0, new MapNode(), Result=false)]
        [TestCase(new MapNode(1, 1), new MapNode(), Result=false)]
        [TestCase(new MapNode(), new MapNode(), Result=true)]
        public bool TestEquals(object obj, MapNode node)
        {
            return node.Equals(obj);
        }

        [TestCase(new MapNode(), new MapNode(), Result=new MapNode())]
        [TestCase(new MapNode(1, 1), new MapNode(-1, -1), Result=new MapNode(2, 2))]
        [TestCase(new MapNode(-1, 0), new MapNode(1, 0), Result=new MapNode(-2, 0))]
        [TestCase(new MapNode(0, 1), new MapNode(0, 2), Result=new MapNode(0, -1))]
        public MapNode TestMinus(MapNode node1, MapNode node2)
        {
            return node1 - node2;
        }

        [TestCase(new MapNode(1, 1), new MapNode(), Result=false)]
        [TestCase(new MapNode(), new MapNode(), Result=true)]
        public bool TestEqualsOperator(MapNode node1, MapNode node2)
        {
            return node1 == node2;
        }

        [TestCase(new MapNode(1, 1), new MapNode(), Result=true)]
        [TestCase(new MapNode(), new MapNode(), Result=false)]
        public bool TestNotEqualsOperator(MapNode node1, MapNode node2)
        {
            return node1 != node2;
        }

        [TestCase(new MapNode(2, 2), Result="2,2".GetHashCode())]
        [TestCase(new MapNode(), Result="0,0".GetHashCode())]
        [TestCase(new MapNode(int.MaxValue, int.MaxValue), Result=(int.MaxValue + "," + int.MaxValue).GetHashCode())]
        [TestCase(new MapNode(2, 2), Result=(new MapNode(2, 2)).GetHashCode())]
        public int TestGetHashCode(MapNode node)
        {
            return node.GetHashCode();
        }

        [TestCase(new MapNode(2, 2), Result="2,2")]
        [TestCase(new MapNode(), Result="0,0")]
        [TestCase(new MapNode(int.MaxValue, int.MaxValue), Result=(int.MaxValue + "," + int.MaxValue))]
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

        public static MapTestData() 
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

        [TestCase(MapTestData.Instance.TestMap1, 0, 1, Result=true)]
        [TestCase(MapTestData.Instance.TestMap2, 1, 1, Result=false)]
        public bool TestIndexer(Map map, int x, int y)
        {
            return map[x, y];
        }

        [TestCase(MapTestData.Instance.TestMap1, Result=5)]
        [TestCase(MapTestData.Instance.TestMap2, Result=0)]
        [TestCase(MapTestData.Instance.TestMap3, Result=0)]
        public double TestDistanceFromStartToEnd(Map map)
        {
            return map.DistanceFromStartToEnd;
        }

        [TestCase(MapTestData.Instance.TestMap1, new MapNode(), new MapNode(), Result=0)]
        [TestCase(MapTestData.Instance.TestMap1, new MapNode(), new MapNode(1, 0), Result=1)]
        [TestCase(MapTestData.Instance.TestMap1, new MapNode(), new MapNode(0, 1), Result=null)]
        [TestCase(MapTestData.Instance.TestMap1, new MapNode(), new MapNode(-2, -2), Result=null)]
        [TestCase(MapTestData.Instance.TestMap1, new MapNode(), new MapNode(2, 2), Result=5)]
        [TestCase(MapTestData.Instance.TestMap2, new MapNode(), new MapNode(2, 2), Result=5)]
        [TestCase(MapTestData.Instance.TestMap3, new MapNode(), new MapNode(2, 2), Result=null)]
        public double TestDistanceBetween(Map map, MapNode from, MapNode to)
        {
            return map.DistanceFromStartToEnd;
        }

        [TestCase(MapTestData.Instance.TestMap1, 0, 0, Result=Map.START_COLOUR)]
        [TestCase(MapTestData.Instance.TestMap1, 1, 0, Result=Map.TILE_COLOUR)]
        [TestCase(MapTestData.Instance.TestMap1, 2, 0, Result=Map.TILE_COLOUR)]
        [TestCase(MapTestData.Instance.TestMap1, 0, 1, Result=Map.WALL_COLOUR)]
        [TestCase(MapTestData.Instance.TestMap1, 1, 1, Result=Map.TILE_COLOUR)]
        [TestCase(MapTestData.Instance.TestMap1, 2, 1, Result=Map.WALL_COLOUR)]
        [TestCase(MapTestData.Instance.TestMap1, 0, 2, Result=Map.CHECKPOINT_COLOUR)]
        [TestCase(MapTestData.Instance.TestMap1, 1, 2, Result=Map.TILE_COLOUR)]
        [TestCase(MapTestData.Instance.TestMap1, 2, 2, Result=Map.END_COLOUR)]
        public Color TestImage(Map map, int x, int y)
        {
            return map.Image.GetPixel(x, y);
        }
    }
}
