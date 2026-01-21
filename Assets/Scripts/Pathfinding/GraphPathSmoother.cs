using System.Collections.Generic;
using PropertyAttribute;
using Tools;
using UnityEngine;

namespace Pathfinding
{
/// <summary>
/// The GraphPathSmoother class provides functionality for smoothing
/// paths within a graph structure.
/// </summary>
public class GraphPathSmoother : MonoBehaviour, IGraphPathFinder
{
    [Header("CONFIGURATION:")]
    [Tooltip("Graph modeling the environment.")]
    [SerializeField] private MapGraph graph;

    [Header("WIRING:")]
    [InterfaceCompliant(typeof(IGraphPathFinder))]
    [Tooltip("IGraphPathFinder component whose returning path needs to be smoothed.")]
    [SerializeField] private MonoBehaviour smoothedPathFinder;

    [Header("DEBUG:")]
    [SerializeField] private bool showGizmos;
    [SerializeField] private Color gizmosColor = Color.greenYellow;
    [SerializeField] public float positionGizmoRadius = 0.5f;

    public MapGraph Graph
    {
        get => graph;
        set
        {
            graph = value;
            if (smoothedGraphPathFinder != null) smoothedGraphPathFinder.Graph = value;
        }
    }

    private IGraphPathFinder smoothedGraphPathFinder;
    private CleanAreaChecker _cleanAreaChecker;
    private PathData _smoothedPathData = new();
    private PathData _rawPathData = new();

    private void Start()
    {
        smoothedGraphPathFinder = (IGraphPathFinder) smoothedPathFinder;
        smoothedGraphPathFinder.Graph = graph;
        _cleanAreaChecker = new CleanAreaChecker(
            (Mathf.Min(Graph.CellSize.x, Graph.CellSize.y)/2), 
            Graph.obstaclesLayers);
    }

    public PathData FindPath(Vector2 targetPosition)
    {
        _rawPathData = smoothedGraphPathFinder.FindPath(targetPosition);
        if (_rawPathData == null) return null;
        _smoothedPathData = SmoothPath(_rawPathData);
        return _smoothedPathData;
    }

    /// <summary>
    /// Smooths the provided path by reducing unnecessary waypoints while maintaining
    /// the path's functionality, ensuring a cleaner and more efficient route.
    /// </summary>
    /// <param name="rawPath">The original path containing a sequence of positions that
    /// may include redundant waypoints.</param>
    /// <returns>A new path with redundant waypoints removed, resulting in a more direct
    /// and optimized path.</returns>
    private PathData SmoothPath(PathData rawPath)
    {
        // With paths of length 2 or less, there's nothing to smooth.
        if (rawPath.PathPositionsLength <= 2) return rawPath;

        List<Vector2> smoothedPositions = new() { rawPath.positions[0] };
        int startIndex = 0;
    
        while (startIndex < rawPath.PathPositionsLength - 1)
        {
            bool cleanPathFound = false;
            // We try to get the farthest position from the current one that is clean.
            for (int testIndex = rawPath.PathPositionsLength - 1; 
                 testIndex > startIndex + 1; 
                 testIndex--)
            {
                // We do a ShapeCast instead of a RayCast because we want to avoid hitting
                // corners and partial obstacles.
                if (_cleanAreaChecker.IsCleanPath(
                        rawPath.positions[startIndex], 
                        rawPath.positions[testIndex]))
                {
                    smoothedPositions.Add(rawPath.positions[testIndex]);
                    startIndex = testIndex;
                    cleanPathFound = true;
                    break;
                }
            }
            
            // If we weren't able to skip any nodes, add the next one directly.
            if (!cleanPathFound)
            {
                startIndex++;
                smoothedPositions.Add(rawPath.positions[startIndex]);
            }
        }
        
        _smoothedPathData.LoadPathData(smoothedPositions);
        return _smoothedPathData;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
    
        if (_smoothedPathData.positions.Count < 1) return;

        Vector2 previousPosition = Vector2.zero;
        Gizmos.color = gizmosColor;
        // Draw path positions
        for (int i=0; i < _smoothedPathData.positions.Count; i++)
        {
            Gizmos.DrawWireSphere(_smoothedPathData.positions[i], positionGizmoRadius);
            if (i >= 1)
            {
                // Draw edges between positions.
                Gizmos.DrawLine(previousPosition, _smoothedPathData.positions[i]);
            }
            previousPosition = _smoothedPathData.positions[i];
        }
    }
#endif
}
}

