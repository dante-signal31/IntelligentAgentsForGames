using UnityEngine;

namespace Pathfinding
{
/// <summary>
/// Basic API every navigation agent must implement.
/// </summary>
public abstract class NavigationAgent: MonoBehaviour
{
    /// <summary>
    /// Position to navigate to.
    /// </summary>
    public abstract Vector2 TargetPosition { get; set; }

    /// <summary>
    /// Agent radius.
    /// </summary>
    public abstract float Radius { get; set; }

    /// <summary>
    /// This agent is ready to navigate.
    /// </summary>
    public abstract bool IsReady { get; }

    /// <summary>
    /// Whether we can get a path to the target.
    /// </summary>
    public abstract bool IsTargetReachable { get; }

    /// <summary>
    /// <p>This agent has reached its target position.</p>
    /// <p>It may not always be possible to reach the target position. If target is not
    /// reachable, then path should get us to the nearest point to target.</p>
    /// </summary>
    public abstract bool IsTargetReached { get; }

    /// <summary>
    /// <p>Returns true if the navigation path's final position has been reached.</p>
    /// <p>If target is not rechable, then paths final position is the nearest point
    /// to target.</p>
    /// </summary>
    public abstract bool IsNavigationFinished { get; }

    /// <summary>
    /// List of path positions to target.
    /// </summary>
    public abstract Vector2[] PathToTarget { get; }

    /// <summary>
    /// <p>It is the last position in the path to target.<p>
    /// <p>If path is reachable, then this is the target position. If not, then it is
    /// the nearest reachable point to target.</p>
    /// </summary>
    public abstract Vector2 PathFinalPosition { get; }

    /// <summary>
    /// Remaining distance to reach the target, following current path.
    /// </summary>
    public abstract float DistanceToTarget();

    /// <summary>
    /// Next position to reach in the current path to target
    /// </summary>
    /// <returns>Next position in global space.</returns>
    public abstract Vector2 GetNextPathPosition();
}
}
