using System.Collections.Generic;
using SteeringBehaviors;
using UnityEngine;
using UnityEngine.Events;


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
    [SerializeField] public UnityEvent pathUpdated = new();
    
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
    public int PathLength => _data.PathPositionsLength;

    /// <summary>
    /// Current index of the position we are going to.
    /// </summary>
    public int CurrentTargetPositionIndex
    {
        get => _data.CurrentTargetPositionIndex;
        set => _data.CurrentTargetPositionIndex = value;
    }
    
    /// <summary>
    /// Position at the current position index.
    /// </summary>
    public Vector2 CurrentTargetPosition => _data.CurrentTargetPosition;
    
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
    public Vector2 GetNextPositionTarget() => _data.GetNextTargetPosition();

    /// <summary>
    /// Encapsulates path-related data used for pathfinding, including positions,
    /// looping configuration, and current target position tracking.
    /// </summary>
    private PathData _data = new();

    private void Awake()
    {
        // Leave any internal initialization here to let external path users make their
        // initial path configuration at the Start phase.
        _data.loop = loop;
        _data.LoadPathData(positions);
    }

    /// <summary>
    /// Update the path data with new positions.
    /// </summary>
    /// <param name="newData">New positions in a PathData instance.</param>
    public void UpdatePathData(PathData newData)
    {
        _data = newData;
        positions.Clear();
        positions.AddRange(_data.positions);
        pathUpdated?.Invoke();
    }

    /// <summary>
    /// Update the path data with new positions.
    /// </summary>
    /// <param name="newPositions">New positions in a list.</param>
    public void UpdatePathData(List<Vector2> newPositions)
    {
        _data.LoadPathData(newPositions);
        positions.Clear();
        positions.AddRange(_data.positions);
        pathUpdated?.Invoke();
    }
    
    /// <summary>
    /// Adds given path positions to the end of this path.
    /// </summary>
    /// <param name="path">Path to append</param>
    public void AppendPath(Path path)
    {
        positions.AddRange(path.positions);
        _data.AddPositionsToPath(path.positions);
        pathUpdated?.Invoke();
    }

    /// <summary>
    /// Retrieves the nearest position in the path to the specified point, along with
    /// its index.
    /// </summary>
    /// <param name="position">The reference position from which the nearest path
    /// position is calculated.</param>
    /// <returns>
    /// A tuple where the first element is the nearest position in the path and the
    /// second element is the index of this position in the path.
    /// </returns>
    public (Vector2, uint) GetNearestPosition(Vector2 position)
    {
        uint index = 0;
        float minDistance = int.MaxValue;
        Vector2 nearestPosition = Vector2.zero;
        uint nearestIndex = 0;
        foreach (Vector2 dataPosition in _data.positions)
        {
            float distance = Vector2.Distance(position, dataPosition);
            if (distance >= minDistance) continue;
            minDistance = distance;
            nearestPosition = dataPosition;
            nearestIndex = index++;
        }
        return (nearestPosition, nearestIndex);
    }
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        _data.loop = loop;
        _data.LoadPathData(positions);
    }

    private void OnDrawGizmos()
    {
        if (!ShowGizmos) return;
        
        if (_data.PathPositionsLength < 1) return;

        Vector2 previousPosition = Vector2.zero;
        Gizmos.color = GizmosColor;
        // Draw path positions
        for (int i=0; i < _data.PathPositionsLength; i++)
        {
            Gizmos.DrawWireSphere(_data.positions[i], positionGizmoRadius);
            if (i >= 1)
            {
                // Draw edges between positions.
                Gizmos.DrawLine(previousPosition, _data.positions[i]);
            }
            previousPosition = _data.positions[i];
        }
    }
#endif
}
}

