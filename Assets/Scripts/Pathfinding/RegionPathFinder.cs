using System.Collections.Generic;
using System.ComponentModel.Composition;
using PropertyAttribute;
using UnityEngine;

namespace Pathfinding
{
/// <summary>
/// A class to compute paths through a set of interconnected regions.
/// The RegionPathFinder utilizes region-based pathfinding strategies, making it
/// suitable for large graph traversals where higher-level abstractions of the graph
/// are necessary for efficiency.
/// </summary>
public class RegionPathFinder : MonoBehaviour, IPathFinder
{
    [Header("CONFIGURATION:")]
    [Tooltip("Graph modeling the regions to traverse.")]
    [SerializeField] public RegionGraph regionGraph;

    [Export("WIRING:")]
    [Tooltip("Dijkstra path finder used for first mile.")]
    [SerializeField] private DijkstraGraphPathFinder firstMilePathFinder;
    [Tooltip("Region path finder used for region-level pathfinding.")]
    [InterfaceCompliant(typeof(IPathFinder))]
    [SerializeField] private MonoBehaviour regionLevelPathFinder;
    [Tooltip("Path finder used for last mile. It can be either Dijkstra or A*.")]
    [InterfaceCompliant(typeof(IPathFinder))]
    [SerializeField] private MonoBehaviour lastMilePathFinder;

    private MapGraph MapGraph => regionGraph.graphRegions.mapGraph;
    private IPathFinder _regionLevelPathFinder;
    private IPathFinder _lastMilePathFinder;

    public IPositionGraph Graph
    {
        get=> regionGraph;
        set
        {
            regionGraph = (RegionGraph) value;
        }
    }

    public void Start()
    {
        // At "first mile" we use Dijkstra to get to the nearest boundary node of the
        // next region.
        if (firstMilePathFinder != null) firstMilePathFinder.Graph = MapGraph;
    
        // At region level you can use both Dijkstra and A*.
        if (regionLevelPathFinder != null)
        {
            _regionLevelPathFinder = (IPathFinder) regionLevelPathFinder;
            _regionLevelPathFinder.Graph = regionGraph;
        }
    
        // At region level you can use both Dijkstra and A*.
        if (lastMilePathFinder != null)
        {
            _lastMilePathFinder = (IPathFinder) lastMilePathFinder;
            _lastMilePathFinder.Graph = MapGraph;
        }
    }

    /// <summary>
    /// Get a path from the current position to the target position, traversing every
    /// intermediate region.
    /// </summary>
    /// <param name="targetPosition">End position.</param>
    /// <param name="fromPosition">Starting position.</param>
    /// <returns>A path to get to the targetPosition from fromPosition</returns>
    public PathData FindPath(Vector2 targetPosition, Vector2 fromPosition=default)
    {
        PathData totalPathData = new();
    
        PositionNode targetNode = 
            (PositionNode) MapGraph.GetNodeAtPosition(targetPosition);
        uint targetRegionId = regionGraph.graphRegions.GetRegionByNodeId(targetNode.Id);
        RegionNode targetRegion = regionGraph.GetRegionNodeById(targetRegionId);
    
        PositionNode initialNode = fromPosition==default?
            (PositionNode) MapGraph.GetNodeAtPosition(transform.position):
            (PositionNode) MapGraph.GetNodeAtPosition(fromPosition);
        uint initialRegionId = regionGraph.graphRegions.GetRegionByNodeId(initialNode.Id);
        RegionNode initialRegion = regionGraph.GetRegionNodeById(initialRegionId);
    
        // Get the path in regionGraph space.
        PathData regionPathData = _regionLevelPathFinder.FindPath(
            targetRegion.Position, 
            initialRegion.Position);
    
        // Now get the sequence of regions to traverse.
        uint[] regionIdsSequence = new uint[regionPathData.PathPositionsLength+1];
        regionIdsSequence[0] = initialRegionId;
        uint index = 1;
        foreach (Vector2 regionPathPosition in regionPathData.positions)
        {
            regionIdsSequence[index] = 
                regionGraph.graphRegions.GetRegionByPosition(regionPathPosition);
            index++;
        }
    
        // "First mile". Get the path to the nearest boundary node of the next region.
        uint currentRegionIndex = 0;
        uint currentRegionId = regionIdsSequence[currentRegionIndex];
        uint nextRegionId = regionIdsSequence[currentRegionIndex + 1];
        RegionNode nextRegion = regionGraph.GetRegionNodeById(nextRegionId);
        List<uint> nextRegionBoundaryNodes = nextRegion.boundaryNodes[initialRegionId];
        firstMilePathFinder.CalculateCosts(
            initialNode, 
            // End condition: exit when you find the first boundary node of the next
            // region.
            () => nextRegionBoundaryNodes.Contains(
                firstMilePathFinder.CurrentNodeRecord.node.Id));
        PositionNode nearestNextRegionBoundaryNode = null;
        foreach (uint nextRegionBoundaryNode in nextRegionBoundaryNodes)
        {
            PositionNode candidateNextRegionBoundaryNode = 
                (PositionNode) MapGraph.GetNodeById(nextRegionBoundaryNode);
            // There should be only une candidate node at the closed list, the nearest
            // one.
            if (firstMilePathFinder.closedDict.ContainsKey(
                    candidateNextRegionBoundaryNode))
            {
                nearestNextRegionBoundaryNode = candidateNextRegionBoundaryNode;
                break;
            }
        }
        PathData pathDataToNextRegion = firstMilePathFinder.BuildPath(
            firstMilePathFinder.closedDict, 
            initialNode, 
            nearestNextRegionBoundaryNode);
        totalPathData.AddPositionsToPath(pathDataToNextRegion.positions);
    
        // Once you are in intermediate regions, you can use static routing embed in 
        // regionGraph to get to the final region.
        currentRegionId = nextRegionId;
        currentRegionIndex++;
        while (currentRegionId != targetRegionId)
        {
            // The last end position is now our starting position.
            PositionNode startNode = nearestNextRegionBoundaryNode;
            // Prepare to travel to the next region.
            nextRegionId = regionIdsSequence[currentRegionIndex + 1];
            Path traversalPath = regionGraph.GetShortestPathToRegion(
                startNode, 
                nextRegionId);
            Vector2 endPosition = traversalPath.positions[^1];
            nearestNextRegionBoundaryNode = 
                (PositionNode) MapGraph.GetNodeAtPosition(endPosition);
            // Append the path to traverse this region to the global path.
            totalPathData.AddPositionsToPath(traversalPath.positions);
            // Go to the next region.
            currentRegionId = nextRegionId;
            currentRegionIndex++;
        }
    
        // "Last-mile": we reached the target region, so we use A* to go from the
        // boundary node to the target one.
        //
        // Again, the last end position is now our starting position.
        PositionNode finalBoundaryNode = nearestNextRegionBoundaryNode;
        PathData pathToTarget = _lastMilePathFinder.FindPath(
            targetNode.Position,
            finalBoundaryNode.Position);
        totalPathData.AddPositionsToPath(pathToTarget.positions);
    
        return totalPathData;
    }
}
}

