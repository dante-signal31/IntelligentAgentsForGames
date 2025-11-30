
using System;
using Groups;
using UnityEngine;

namespace Pathfinding
{
public class Path : GroupPattern
{
    [Header("PATH CONFIGURATION:")] 
    [SerializeField] public bool loop;
    
    /// <summary>
    /// Target positions of the path.
    /// </summary>
    public Vector2[] TargetPositions => Positions.Offsets;
    
    /// <summary>
    /// How many positions this path has.
    /// </summary>
    public int PathLength => Positions.Offsets.Length;

    /// <summary>
    /// Current index of the position we are going to.
    /// </summary>
    public int CurrentTargetPositionIndex { get; private set; }
    
    /// <summary>
    /// Position at the current position index.
    /// </summary>
    public Vector2 CurrentTargetPosition => Positions.Offsets[CurrentTargetPositionIndex];

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
        if (CurrentTargetPositionIndex == Positions.Offsets.Length - 1)
        {
            if (loop) CurrentTargetPositionIndex = 0;
        }
        else
        {
            CurrentTargetPositionIndex++;
        }
        return Positions.Offsets[CurrentTargetPositionIndex];
    }

    private void Start()
    {
        positionGizmoRadius = 10;
        gizmoTextOffset = new(10, 10);
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!ShowGizmos) return;
        
        if (Positions.Offsets.Length < 1) return;

        Vector2 previousPosition = Vector2.zero;
        Gizmos.color = GizmosColor;
        // Draw path positions
        for (int i=0; i < Positions.Offsets.Length; i++)
        {
            // Vector2 gizmoBorder = new Vector2(positionGizmoRadius, positionGizmoRadius);
            // Vector2 textPosition = Positions.Offsets[i] - gizmoBorder - gizmoTextOffset;
            // DrawString(ThemeDB.FallbackFont, textPosition, $"{Name}-{i}");
            Gizmos.DrawWireSphere(Positions.Offsets[i], positionGizmoRadius);
            if (i >= 1)
            {
                // Draw edges between positions.
                Gizmos.DrawLine(previousPosition, Positions.Offsets[i]);
            }
            previousPosition = Positions.Offsets[i];
        }
    }
#endif
}
}

