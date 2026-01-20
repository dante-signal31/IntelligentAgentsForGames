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
        int endIndex = 2;
    
        do
        {
            // We do a ShapeCast instead of a RayCast because we want to avoid hitting
            // corners and partial obstacles.
            if (_cleanAreaChecker.IsCleanPath(
                    rawPath.positions[startIndex],
                    rawPath.positions[endIndex]))
            {
                // If there was a clear path to the end of the path, then add that end to
                // the smoothed path before leaving the loop. That will complete the
                // smoothed path.
                if (endIndex >= rawPath.PathPositionsLength - 1) 
                    smoothedPositions.Add(rawPath.positions[endIndex]);
                endIndex++;
                // If there was a clear path from the starIndex position to the endIndex
                // position, and the endIndex was not the end of the path, then we can
                // omit the positions between them from the smoothed path.
                continue;
            }
            if (endIndex == rawPath.PathPositionsLength - 1)
            {
                // If we were at the end of the path, then add the last position to the
                // smoothed path and smooth no more.
                smoothedPositions.Add(rawPath.positions[endIndex]);
                break;
            }
            // Otherwise, add the previous position to the occluded one to the smoothed
            // path because it was the last we could get directly.
            smoothedPositions.Add(rawPath.positions[endIndex-1]);
            // Now we will ray trace from that position to find out if we can omit any of
            // the remaining positions.
            startIndex = endIndex - 1;
            endIndex++;
        } while (endIndex < rawPath.PathPositionsLength);
    
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

