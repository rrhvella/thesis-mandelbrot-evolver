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
    public struct MapNode 
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
                return MathExtensions.EuclideanDistance(X, Y);
            }
        }

        public double Magnitude
        {
            get
            {
                return Math.Abs(X) + Math.Abs(Y);
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

        private bool[] collisionMap;

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
                :this(new bool[width * height], width, start, end, checkpoints, mandatoryCheckPointLevel)
        {
        }

        public Map(bool[] collisionMap, int width, MapNode start, MapNode end, IEnumerable<MapNode> checkpoints, 
                                int mandatoryCheckPointLevel)
        {
            this.Width = width;
            this.Height = collisionMap.Length / width;

            this.StartNode = start;
            this.EndNode = end;

            this.Checkpoints = checkpoints;
            this.mandatoryCheckPointLevel = mandatoryCheckPointLevel;

            this.collisionMap = collisionMap;
        }

        public bool this[int x, int y]
        {
            get
            {
                return collisionMap[x + y * Width];
            }
            set
            {
                collisionMap[x + y * Width] = value;
            }
        }

        public bool this[int i]
        {
            get
            {
                return collisionMap[i];
            }
            set
            {
                collisionMap[i] = value;
            }
        }

        private class StartToEndRecord
        {
            public StartToEndRecord Parent;
            public bool IsEnd;
            public int Distance;

            private int checkpointMembershipCount;

            public int CheckpointMembershipCount
            {
                get
                {
                    return checkpointMembershipCount;
                }
            }

            public bool[] CheckpointMembership;
            public MapNode MapNode;

            public StartToEndRecord(int numberOfCheckpoints)
            {
                CheckpointMembership = new bool[numberOfCheckpoints];
            }        

            public void UpdateDistance()
            {
                Distance = Parent.Distance + 1;
            }

            public void UpdateCheckpoints(StartToEndRecord pathParent)
            {
                checkpointMembershipCount = 0;

                for (int i = 0; i < CheckpointMembership.Length; i++)
                {
                    if (pathParent.CheckpointMembership[i])
                    {
                        CheckpointMembership[i] = pathParent.CheckpointMembership[i];
                    }

                    if (CheckpointMembership[i])
                    {
                        checkpointMembershipCount++;
                    }
                }
            }
        }

        public double DistanceFromStartToEnd 
        {
            get 
            {
                var numberOfCheckpoints = Checkpoints.Count();

                //Initialise map.
                var record = new StartToEndRecord[Width, Height];

                //Initialise critical point.
                //Start record.
                var startRecord = new StartToEndRecord(numberOfCheckpoints);
                
                startRecord.MapNode = StartNode;
                startRecord.Distance = 0;
                startRecord.Parent = new StartToEndRecord(numberOfCheckpoints);

                record[StartNode.X, StartNode.Y] = startRecord;

                //End record.
                var endRecord = new StartToEndRecord(numberOfCheckpoints);

                endRecord.MapNode = EndNode;
                endRecord.IsEnd = true;

                record[EndNode.X, EndNode.Y] = endRecord;

                //Checkpoints.
                var checkpointIndex = 0;

                foreach (var checkpoint in Checkpoints)
                {
                    var checkpointRecord = new StartToEndRecord(numberOfCheckpoints);

                    checkpointRecord.MapNode = checkpoint;
                    record[checkpoint.X, checkpoint.Y] = checkpointRecord;

                    checkpointRecord.CheckpointMembership[checkpointIndex++] = true;
                }

                //Begin algorithm.
                var openQueue = new Queue<StartToEndRecord>();
                openQueue.Enqueue(startRecord);

                bool endFound = false;

                while (openQueue.Count > 0)
                {
                    var current = openQueue.Dequeue();

                    if (current.IsEnd)
                    {
                        endFound = true;
                        break;
                    }
                    
                    //Get children.
                    var possibleCandidates = GetChildren(current.MapNode);

                    foreach (var child in possibleCandidates)
                    {
                        StartToEndRecord childRecord = null;

                        if (record[child.X, child.Y] == null)
                        {
                            childRecord = new StartToEndRecord(Checkpoints.Count());
                            childRecord.MapNode = child;

                            record[child.X, child.Y] = childRecord;
                        }
                        else
                        {
                            childRecord = record[child.X, child.Y];
                        }

                        if (childRecord.Parent == null || childRecord.Parent.Distance > current.Distance)
                        {
                            if (childRecord.Parent == null)
                            {
                                openQueue.Enqueue(childRecord);
                            }

                            childRecord.Parent = current;
                            childRecord.UpdateDistance();
                        }

                        childRecord.UpdateCheckpoints(current);
                    }
                }

                if (endFound)
                {
                    if (endRecord.CheckpointMembershipCount >= mandatoryCheckPointLevel)
                    {
                        return endRecord.Distance + 2;
                    }
                    else
                    {
                        return 2;
                    }
                }
                else
                {
                    return 1;
                }
            }
        }

        private IEnumerable<MapNode> GetChildren(MapNode mapNode)
        {
            if (mapNode.X > 0 && !this[mapNode.X - 1, mapNode.Y])
            {
                yield return new MapNode(mapNode.X - 1, mapNode.Y);
            }

            if (mapNode.Y > 0 && !this[mapNode.X, mapNode.Y - 1])
            {
                yield return new MapNode(mapNode.X, mapNode.Y - 1);
            }

            if (mapNode.X < Width - 1 && !this[mapNode.X + 1, mapNode.Y])
            {
                yield return new MapNode(mapNode.X + 1, mapNode.Y);
            }

            if (mapNode.Y < Height - 1&& !this[mapNode.X, mapNode.Y + 1])
            {
                yield return new MapNode(mapNode.X, mapNode.Y + 1);
            }
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
                        result.SetPixel(x, y, (this[x, y]) ? WALL_COLOUR : TILE_COLOUR);
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
            result.collisionMap = (bool[])collisionMap.Clone();

            return result;
        }
    }
}
