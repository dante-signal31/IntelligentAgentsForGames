using UnityEngine;

namespace Pathfinding
{
/// <summary>
/// Interface for graph-bases pathfinding algorithms.
/// </summary>
public interface IGraphPathFinder: IPathFinder
{
    /// <summary>
    /// Graph modeling the environment.
    /// </summary>
    public MapGraph Graph { get; set; }
}
}