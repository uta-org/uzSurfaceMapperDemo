using System;
using System.Collections.Concurrent;
using uzSurfaceMapper.Model;
using uzSurfaceMapper.Extensions;
using static uzSurfaceMapper.Core.Generators.MapGenerator;

namespace uzSurfaceMapper.Model
{
    [Serializable]
    public class RoadNode : IPathNode
    {
        //[JsonConverter(typeof(RoadNodeConverter))]
        public ConcurrentBag<int> Connections { get; set; } // TODO: IPathNode<T> + RoadNode : IPathNode<RoadNode> + Connections (ConcurrentBag<RoadNode>), but can't be serialized due to StackOverflow and OutOfMemory exceptions

        public Point Position { get; set; }
        public bool Invalid { get; set; }
        public int Thickness { get; set; }
        public ConcurrentBag<int> ParentNodes { get; set; }

        public RoadNode()
        {
            //Connections = new List<RoadNode>();
        }

        public RoadNode(Point position, int thickness)
        //: this()
        {
            Position = position;
            Thickness = thickness;
        }

        public RoadNode(Point position, bool invalid, int thickness)
        //: this()
        {
            Position = position;
            Invalid = invalid;
            Thickness = thickness;
        }

        public RoadNode(int x, int y, int thickness)
            : this(new Point(x, y), thickness)
        {
        }

        public void SetThickness(int thickness)
        {
            // TODO: Call this when needed and thickness == -1
            Thickness = thickness;
        }

        public int GetKey()
        {
            return F.P(Position.x, Position.y, mapWidth, mapHeight);
        }
    }
}