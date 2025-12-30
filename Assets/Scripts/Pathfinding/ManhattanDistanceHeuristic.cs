using UnityEngine;

namespace Pathfinding
{
public class ManhattanDistanceHeuristic: MonoBehaviour, IAStarHeuristic
{
    public float EstimateCostToTarget(Vector2 startPosition, Vector2 targetPosition)
    {
        float distanceX = Mathf.Abs(targetPosition.x - startPosition.x);
        float distanceY = Mathf.Abs(targetPosition.y - startPosition.y);
        float manhattanDistance = distanceX + distanceY;
        return manhattanDistance;
    }
}
}