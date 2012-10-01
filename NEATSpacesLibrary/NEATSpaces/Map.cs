using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph;
using QuickGraph.Algorithms.ShortestPath;
using System.Drawing;
using System.Drawing.Imaging;
using NEATSpacesLibrary.Extensions;

namespace NEATSpacesLibrary.NEATSpaces
{
    public struct MapNode : IComparable
    {
        public int X;
        public int Y;

        public MapNode(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public override bool Equals(object obj)
        {
            if(!(obj is MapNode))
            {
                return false;
            }

            var input = (MapNode)obj;

            return this == input;
        }

        public static MapNode operator -(MapNode node1, MapNode node2)
        {
            return new MapNode(node1.X - node2.X, node1.Y - node2.Y);
        }

        public static bool operator ==(MapNode node1, MapNode node2)
        {
            return node1.X == node2.X && node1.Y == node2.Y;
        }

        public static bool operator !=(MapNode node1, MapNode node2)
        {
            return !(node1 == node2);
        }


        public override int GetHashCode()
        {
            return X<<16>>16 | Y<<16;
        }

        public override string ToString()
        {
            return X + "," + Y;
        }

        public double EuclideanDistance
        {
            get
            {
                return MathExtensions.EuclideanDistance(new double[] { X, Y });
            }
        }

        public double Magnitude
        {
            get
            {
                return Math.Abs(X) + Math.Abs(Y);
            }
        }

        public int CompareTo(object obj)
        {
            if (this.Equals(obj))
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }
    }

    public class Map
    {
        public static readonly Color WALL_COLOUR = Color.FromArgb(255, 0, 0, 0);
        public static readonly Color TILE_COLOUR = Color.FromArgb(255, 255, 255, 255);
        public static readonly Color START_COLOUR = Color.FromArgb(255, 255, 0, 0);
        public static readonly Color END_COLOUR = Color.FromArgb(255, 0, 0, 255);
        public static readonly Color CHECKPOINT_COLOUR = Color.FromArgb(255, 0, 255, 0);

        private bool[,] collisionMap;
        private DelegateVertexAndEdgeListGraph<MapNode, SEquatableEdge<MapNode>> graph;

        public int Width
        {
            get;
            private set;
        }

        public int Height
        {
            get;
            private set;
        }

        public IEnumerable<MapNode> Checkpoints 
        { 
            get; 
            private set;
        }

        private int mandatoryCheckPointLevel;
        public MapNode EndNode
        {
            get;
            private set;
        }

        public MapNode StartNode
        {
            get;
            private set;
        }

        public Map(int width, int height, MapNode start, MapNode end, IEnumerable<MapNode> checkpoints, 
                                int mandatoryCheckPointLevel)
        {
            this.Width = width;
            this.Height = height;

            this.StartNode = start;
            this.EndNode = end;

            this.Checkpoints = checkpoints;
            this.mandatoryCheckPointLevel = mandatoryCheckPointLevel;

            this.collisionMap = new bool[width, height];
            var vertexList = from x in Enumerable.Range(0, width)
                             from y in Enumerable.Range(0, height)
                             select new MapNode(x, y);

            this.graph = new DelegateVertexAndEdgeListGraph<MapNode, SEquatableEdge<MapNode>>(
                                        vertexList,
                                        new TryFunc<MapNode, IEnumerable<SEquatableEdge<MapNode>>>(
                                                delegate(MapNode currentNode, out IEnumerable<SEquatableEdge<MapNode>> adjacentEdges)
                                                {
                                                    var neighbors = new List<MapNode>();

                                                    if (currentNode.X > 0)
                                                    {
                                                        neighbors.Add(new MapNode(currentNode.X - 1, currentNode.Y));
                                                    }

                                                    if(currentNode.X < width - 1) 
                                                    {
                                                        neighbors.Add(new MapNode(currentNode.X + 1, currentNode.Y));
                                                    }

                                                    if (currentNode.Y > 0)
                                                    {
                                                        neighbors.Add(new MapNode(currentNode.X, currentNode.Y - 1));
                                                    }

                                                    if(currentNode.Y < height - 1) 
                                                    {
                                                        neighbors.Add(new MapNode(currentNode.X, currentNode.Y + 1));
                                                    }

                                                    adjacentEdges = from adjacentNode in neighbors 
                                                                        where !collisionMap[adjacentNode.X, adjacentNode.Y]
                                                                        select new SEquatableEdge<MapNode>(currentNode, adjacentNode);

                                                    return neighbors.Count > 0;
                                                }
                                            ));

        }

        public bool this[int x, int y]
        {
            get
            {
                return collisionMap[x, y];
            }
            set
            {
                collisionMap[x, y] = value;
            }
        }

        public bool this[int i]
        {
            get
            {
                return collisionMap[i % Width, i / Width];
            }
            set
            {
                collisionMap[i % Width, i / Width] = value;
            }
        }

        public double DistanceFromStartToEnd 
        {
            get 
            {
                var distanceFromStartToEnd = DistanceBetween(StartNode, EndNode);

                if (distanceFromStartToEnd == null)
                {
                    return 0;
                }

                var missLimit = Checkpoints.Count() - mandatoryCheckPointLevel;
                var checkPointLevel = 0;
                var checkpointsIterator = Checkpoints.GetEnumerator();

                while (missLimit >= 0 && checkPointLevel < mandatoryCheckPointLevel)
                {
                    checkpointsIterator.MoveNext();
                    var checkpoint = checkpointsIterator.Current;

                    if (DistanceBetween(StartNode, checkpoint) != null && DistanceBetween(checkpoint, EndNode) != null)
                    {
                        checkPointLevel++;
                    }
                    else
                    {
                        missLimit--;
                    }
                }

                if (missLimit < 0)
                {
                    return 0;
                }

                return (double)distanceFromStartToEnd;
            }
        }

        public double? DistanceBetween(MapNode from, MapNode to)
        {
            var algorithm = new AStarShortestPathAlgorithm<MapNode, SEquatableEdge<MapNode>>(this.graph, 
                new Func<SEquatableEdge<MapNode>,double>(
                    delegate(SEquatableEdge<MapNode> edge) 
                    {
                        return (edge.Target - edge.Source).Magnitude;
                    }
                ), new Func<MapNode,double>(
                    delegate(MapNode currentNode) {
                        return (to - currentNode).Magnitude;
                    }
                ));

            algorithm.FinishVertex += new VertexAction<MapNode>(delegate(MapNode currentNode)
            {
                if (currentNode == to)
                {
                    algorithm.Abort();
                }
            });

            algorithm.Compute(from);

            return (algorithm.Distances.ContainsKey(to) && algorithm.Distances[to] < double.MaxValue) ? 
                        (double?)algorithm.Distances[to] : null;
        }

        public Bitmap Image
        {
            get 
            {
                var result = new Bitmap(Width, Height);

                foreach(var x in Enumerable.Range(0, Width)) 
                {
                    foreach(var y in Enumerable.Range(0, Height)) 
                    {
                        result.SetPixel(x, y, (collisionMap[x, y]) ? WALL_COLOUR : TILE_COLOUR);
                    }
                }

                result.SetPixel(StartNode.X, StartNode.Y, START_COLOUR);
                result.SetPixel(EndNode.X, EndNode.Y, END_COLOUR);

                foreach (var node in Checkpoints)
                {
                    result.SetPixel(node.X, node.Y, CHECKPOINT_COLOUR);
                }

                return result;
            }
        }

        public int Length
        {
            get
            {
                return collisionMap.Length;
            }
        }

        public Map Copy()
        {
            var result = new Map(Width, Height, StartNode, EndNode, Checkpoints, mandatoryCheckPointLevel);
            result.collisionMap = (bool[,])collisionMap.Clone();

            return result;
        }
    }
}
