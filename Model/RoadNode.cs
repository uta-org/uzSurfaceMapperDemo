﻿using System;
using Newtonsoft.Json;
using uzSurfaceMapper.Model.Enums;

namespace uzSurfaceMapper.Model
{
    [Serializable]
    public class RoadNode
    {
        public int Thickness { get; set; }
        [JsonIgnore] public InnerDirection FlowDirection { get; }
        [JsonIgnore] public InnerDirection CrossDirection { get; } // Perpendicular
        public Point Position { get; set; }

        public RoadNode()
        {
        }

        public RoadNode(int x, int y) : this(new Point(x, y))
        {
        }

        public RoadNode(Point position)
        {
            Position = position;
        }

        public RoadNode(int thickness, InnerDirection flowDirection, InnerDirection crossDirection, Point position)
        {
            Thickness = thickness;
            FlowDirection = flowDirection;
            CrossDirection = crossDirection;
            Position = position;
        }
    }

    //[Serializable]
    //public class RoadNode : IPathNode
    //{
    //    //[JsonConverter(typeof(PathNodeConverter))]
    //    // Remove this...
    //    public HashSet<int> Connections { get; set; } // TODO: IPathNode<T> + RoadNode : IPathNode<RoadNode> + Connections (ConcurrentBag<RoadNode>), but can't be serialized due to StackOverflow and OutOfMemory exceptions

    //    public Point Position { get; set; }
    //    public bool Invalid { get; set; } // Remove
    //    public int Thickness { get; set; } // Remove
    //    public List<int> ParentNodes { get; set; } // Remove

    //    public RoadNode()
    //    {
    //        //Connections = new List<RoadNode>();
    //    }

    //    public RoadNode(Point position, int thickness)
    //    //: this()
    //    {
    //        Position = position;
    //        Thickness = thickness;
    //    }

    //    public RoadNode(Point position, bool invalid, int thickness)
    //    //: this()
    //    {
    //        Position = position;
    //        Invalid = invalid;
    //        Thickness = thickness;
    //    }

    //    public RoadNode(int x, int y, int thickness)
    //        : this(new Point(x, y), thickness)
    //    {
    //    }

    //    public void SetThickness(int thickness)
    //    {
    //        // TODO: Call this when needed and thickness == -1
    //        Thickness = thickness;
    //    }

    //    public int GetKey()
    //    {
    //        return F.P(Position.x, Position.y, mapWidth, mapHeight);
    //    }

    //    public override string ToString()
    //    {
    //        return $"\n{{\n\t{Position} --> {GetKey()}\n\tThickness: {Thickness}\n\tParent Nodes: {ParentNodes?.Count}\n\tConnections: {Connections?.Count}\n\tInvalid?: {Invalid}}}\n";
    //    }
    //}
}