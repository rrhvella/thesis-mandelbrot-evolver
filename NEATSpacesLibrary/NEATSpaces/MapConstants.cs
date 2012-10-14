using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NEATSpacesLibrary.NEATSpaces
{
    public class MapConstants
    {
        public const int MANDATORY_CHECKPOINT_LEVEL = 3;

        public static readonly MapNode[] CHECKPOINTS = new[] { new MapNode(6, 24), new MapNode(12, 18), 
                                                        new MapNode(18, 12), new MapNode(24, 6) };
        public static readonly MapNode START_NODE = new MapNode(0, 0);
        public static readonly MapNode END_NODE = new MapNode(29, 29);

        public const int MAP_SIZE = 30;

        public static Map CreateMap()
        {
            return new Map(MAP_SIZE, MAP_SIZE, START_NODE, END_NODE, CHECKPOINTS, MANDATORY_CHECKPOINT_LEVEL);
        }
    }
}
