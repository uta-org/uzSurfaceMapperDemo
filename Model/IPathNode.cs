using System.Collections.Generic;

namespace uzSurfaceMapper.Model
{
    public interface IPathNode
    {
        HashSet<int> Connections { get; }
        Point Position { get; }
        bool Invalid { get; }
    }
}