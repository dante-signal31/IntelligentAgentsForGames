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
    [Tooltip("Whether the path should loop back to the beginning once the end is " +
             "reached.")]
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
    public int PathLength => Data.PathPositionsLength;

    /// <summary>
    /// Current index of the position we are going to.
    /// </summary>
    public int CurrentTargetPositionIndex => Data.CurrentTargetPositionIndex;
    
    /// <summary>
    /// Position at the current position index.
    /// </summary>
    public Vector2 CurrentTargetPosition => Data.CurrentTargetPosition;
    
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
    public Vector2 GetNextPositionTarget() => Data.GetNextPositionTarget();

    /// <summary>
    /// Encapsulates path-related data used for pathfinding, including positions,
    /// looping configuration, and current target position tracking.
    /// </summary>
    public PathData Data { get; private set; }= new();

    private void Awake()
    {
        // Leave any internal initialization here to let external path users make their
        // initial path configuration at the Start phase.
        Data.loop = loop;
        Data.LoadPathData(positions);
    }

    public void UpdatePathData(PathData newData)
    {
        Data = newData;
        positions.Clear();
        positions.AddRange(Data.positions);
    }
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        Data.loop = loop;
        Data.LoadPathData(positions);
    }

    private void OnDrawGizmos()
    {
        if (!ShowGizmos) return;
        
        if (Data.PathPositionsLength < 1) return;

        Vector2 previousPosition = Vector2.zero;
        Gizmos.color = GizmosColor;
        // Draw path positions
        for (int i=0; i < Data.PathPositionsLength; i++)
        {
            Gizmos.DrawWireSphere(Data.positions[i], positionGizmoRadius);
            if (i >= 1)
            {
                // Draw edges between positions.
                Gizmos.DrawLine(previousPosition, Data.positions[i]);
            }
            previousPosition = Data.positions[i];
        }
    }
#endif
}
}

