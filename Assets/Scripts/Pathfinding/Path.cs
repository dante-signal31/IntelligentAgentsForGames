using System.Collections.Generic;
using SteeringBehaviors;
using UnityEngine;


namespace Pathfinding
{
public class Path : MonoBehaviour, IGizmos
{
    [Header("PATH CONFIGURATION:")] 
    [SerializeField] public bool loop;
    [Tooltip("Target global positions of the path")] 
    [SerializeField] public List<Vector2> positions;
    
    [Header("DEBUG:")]
    [SerializeField] private bool showGizmos;
    [SerializeField] private Color gizmosColor;
    [SerializeField] public float positionGizmoRadius = 0.5f;
    [SerializeField] public Vector2 gizmoTextOffset = new(0.1f, 0.1f);
    
    public bool ShowGizmos
    {
        get => showGizmos;
        set => showGizmos = value;
    }

    public Color GizmosColor
    {
        get => gizmosColor;
        set => gizmosColor = value;
    }
    
    /// <summary>
    /// How many positions this path has.
    /// </summary>
    public int PathLength => positions.Count;

    /// <summary>
    /// Current index of the position we are going to.
    /// </summary>
    public int CurrentTargetPositionIndex { get; private set; }
    
    /// <summary>
    /// Position at the current position index.
    /// </summary>
    public Vector2 CurrentTargetPosition => positions[CurrentTargetPositionIndex];
    
    /// <summary>
    /// Where to place strings with the number of positions.
    /// </summary>
    public Vector2 GizmoTextPosition => 
        new Vector2(positionGizmoRadius, positionGizmoRadius) + gizmoTextOffset;

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
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!ShowGizmos) return;
        
        if (positions.Count < 1) return;

        Vector2 previousPosition = Vector2.zero;
        Gizmos.color = GizmosColor;
        // Draw path positions
        for (int i=0; i < positions.Count; i++)
        {
            // Vector2 gizmoBorder = new Vector2(positionGizmoRadius, positionGizmoRadius);
            // Vector2 textPosition = Positions.Offsets[i] - gizmoBorder - gizmoTextOffset;
            // DrawString(ThemeDB.FallbackFont, textPosition, $"{Name}-{i}");
            Gizmos.DrawWireSphere(positions[i], positionGizmoRadius);
            if (i >= 1)
            {
                // Draw edges between positions.
                Gizmos.DrawLine(previousPosition, positions[i]);
            }
            previousPosition = positions[i];
        }
    }
#endif
}
}

