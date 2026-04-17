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

    private int _currentTargetPositionIndex;
    /// <summary>
    /// Current index of the position we are going to.
    /// <remarks>
    /// If the given index is greater than the number of positions in the path, the
    /// modulus will be applied to the index to get the correct position index.
    /// </remarks>
    /// </summary>
    public int CurrentTargetPositionIndex
    {
        get => _currentTargetPositionIndex;
        set => _currentTargetPositionIndex = value != 0 ? value % positions.Count : 0;
    }

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
        AddPositionsToPath(path);
        CurrentTargetPositionIndex = 0;
    }

    /// <summary>
    /// Append given positions to the path.
    /// </summary>
    /// <param name="addedPositions">Positions to append.</param>
    public void AddPositionsToPath(List<Vector2> addedPositions)
    {
        positions.AddRange(addedPositions);
        PathLength = GetPathLength();
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
    public Vector2 GetNextTargetPosition()
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