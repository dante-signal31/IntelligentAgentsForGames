using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
/// <summary>
/// Represents a collection of positions used in a pathfinding system.
/// </summary>
public class PathData
{
    public bool loop;
    public readonly List<Vector2> positions = new();
    
    /// <summary>
    /// How many positions this path has.
    /// </summary>
    public int PathPositionsLength => positions.Count;

    /// <summary>
    /// Represents the total length of the path, calculated as the sum of distances
    /// between consecutive positions in the path.
    /// </summary>
    public float PathLength { get; private set; }

    /// <summary>
    /// Current index of the position we are going to.
    /// </summary>
    public int CurrentTargetPositionIndex { get; private set; }

    /// <summary>
    /// Position at the current position index.
    /// </summary>
    public Vector2 CurrentTargetPosition =>
        CurrentTargetPositionIndex >= positions.Count ? 
            Vector2.zero : 
            positions[CurrentTargetPositionIndex];

    /// <summary>
    /// Clears the current path data and loads a new set of positions into the path.
    /// </summary>
    /// <param name="path">
    /// The list of positions to be added to the path. Each position represents a point
    /// in the path.
    /// </param>
    public void LoadPathData(List<Vector2> path)
    {
        positions.Clear();
        positions.AddRange(path);
        PathLength = GetPathLength();
        CurrentTargetPositionIndex = 0;
    }

    /// <summary>
    /// Calculates the total length of the path by summing the distances between
    /// consecutive positions.
    /// </summary>
    /// <returns>
    /// The total length of the path. If the path contains fewer than two positions,
    /// the result is 0.
    /// </returns>
    private float GetPathLength()
    {
        if (positions.Count < 2) return 0;
        
        float distance = 0;
        for (int i = 1; i < positions.Count; i++)
        {
            distance += Vector2.Distance(positions[i-1], positions[i]);
        }
        
        return distance;
    }
    
    /// <summary>
    /// <p>Get the next position target in Path.</p>
    /// </summary>
    /// <returns>
    /// <p>Next position node if we are not at the end.</p>
    /// <p>If we are at the end and Loop is false, then the last target position is
    /// returned; whereas if the loop is true, then the index is reset to 0 and the
    /// first target position is returned.</p></returns>
    public Vector2 GetNextPositionTarget()
    {
        if (CurrentTargetPositionIndex == positions.Count - 1)
        {
            if (loop) CurrentTargetPositionIndex = 0;
        }
        else
        {
            CurrentTargetPositionIndex++;
        }
        return positions[CurrentTargetPositionIndex];
    }
}
}