using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pathfinding
{
    /// <summary>
    /// Implements a depth-first search (DFS) pathfinding algorithm for navigating a
    /// graph structure.
    /// </summary>
    /// <remarks>
    /// Depth-first search does NOT guarantee to find the shortest path. Actually, when
    /// it finds a path, it's likely to be suboptimal.
    /// </remarks>
    public class DepthFirstPathFinder : NotInformedPathFinder<NodeRecordStack> { }
}