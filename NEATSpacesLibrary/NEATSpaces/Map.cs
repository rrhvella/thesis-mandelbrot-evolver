using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph;
using QuickGraph.Algorithms.ShortestPath;
using System.Drawing;
using System.Drawing.Imaging;

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
            if(obj.GetType() != this.GetType()) 
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
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            return X + "," + Y;
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
        private int width;
        private int height;
        private IEnumerable<MapNode> checkpoints;
        private int mandatoryCheckPointLevel;
        private MapNode end;
        private MapNode start;

        public Map(int width, int height, MapNode start, MapNode end, IEnumerable<MapNode> checkpoints, 
                                int mandatoryCheckPointLevel)
        {
            this.width = width;
            this.height = height;

            this.start = start;
            this.end = end;

            this.checkpoints = checkpoints;
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
                return collisionMap[i % width, i / width];
            }
            set
            {
                collisionMap[i % width, i / width] = value;
            }
        }

        public double DistanceFromStartToEnd 
        {
            get 
            {
                var distanceFromStartToEnd = DistanceBetween(start, end);

                if (distanceFromStartToEnd == null)
                {
                    return 0;
                }

                var membersOfE = (from checkpoint in checkpoints.AsParallel()
                                  where DistanceBetween(start, checkpoint) != null &&
                                      DistanceBetween(checkpoint, end) != null
                                  select checkpoint).Count();

                if (membersOfE < mandatoryCheckPointLevel)
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
                var result = new Bitmap(width, height);

                foreach(var x in Enumerable.Range(0, width)) 
                {
                    foreach(var y in Enumerable.Range(0, height)) 
                    {
                        result.SetPixel(x, y, (collisionMap[x, y]) ? WALL_COLOUR : TILE_COLOUR);
                    }
                }

                result.SetPixel(start.X, start.Y, START_COLOUR);
                result.SetPixel(end.X, end.Y, END_COLOUR);

                foreach (var node in checkpoints)
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
            var result = new Map(width, height, start, end, checkpoints, mandatoryCheckPointLevel);
            result.collisionMap = (bool[,])collisionMap.Clone();

            return result;
        }
    }
}
