using System.Collections.Concurrent;
using uzSurfaceMapper.Model;

namespace uzSurfaceMapper.Model
{
    public interface IPathNode
    {
        ConcurrentBag<int> Connections { get; }
        Point Position { get; }
        bool Invalid { get; }
    }
}