using System.Collections.Generic;
using SteeringBehaviors;
using UnityEngine;


namespace Pathfinding
{
/// <summary>
/// Component to show and manipulate a path in the editor.
/// </summary>
public class Path : MonoBehaviour, IGizmos
{
    [Header("PATH CONFIGURATION:")] 
    [SerializeField] public bool loop;
    [Tooltip("Target global positions of the path")] 
    [SerializeField] public List<Vector2> positions = new();
    
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
    public int PathLength => _pathData.PathLength;

    /// <summary>
    /// Current index of the position we are going to.
    /// </summary>
    public int CurrentTargetPositionIndex => _pathData.CurrentTargetPositionIndex;
    
    /// <summary>
    /// Position at the current position index.
    /// </summary>
    public Vector2 CurrentTargetPosition => _pathData.CurrentTargetPosition;
    
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
    public Vector2 GetNextPositionTarget() => _pathData.GetNextPositionTarget();
    
    private PathData _pathData = new();

    private void Start()
    {
        _pathData.loop = loop;
        _pathData.LoadPathData(positions);
    }

    public void UpdatePathData(PathData newData)
    {
        _pathData = newData;
        positions.Clear();
        positions.AddRange(_pathData.positions);
    }
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        _pathData.loop = loop;
        _pathData.LoadPathData(positions);
    }

    private void OnDrawGizmos()
    {
        if (!ShowGizmos) return;
        
        if (positions.Count < 1) return;

        Vector2 previousPosition = Vector2.zero;
        Gizmos.color = GizmosColor;
        // Draw path positions
        for (int i=0; i < positions.Count; i++)
        {
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

