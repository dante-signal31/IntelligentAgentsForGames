using UnityEngine;

namespace Pathfinding
{
public class SquareDistanceHeuristic: MonoBehaviour, IAStarHeuristic
{
    public float EstimateCostToTarget(Vector2 startPosition, Vector2 targetPosition)
    {
        return Vector2.SqrMagnitude(targetPosition - startPosition);
    }
}
}