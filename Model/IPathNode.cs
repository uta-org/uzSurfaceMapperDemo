using System.Collections.Generic;

namespace uzSurfaceMapper.Model
{
    public interface IPathNode
    {
        List<int> Connections { get; }
        Point Position { get; }
        bool Invalid { get; }
    }
}