using System.Collections.Generic;
using Tools;
using UnityEngine;

namespace Pathfinding
{
/// <summary>
/// Graph whose nodes are regions. Those regions comprise groups of position nodes. So
/// this is a secondary graph structure over the map graph that is used to represent
/// the map.
/// </summary>
public class RegionGraph : MonoBehaviour, IPositionGraph
{
    [Header("WIRING:")] 
    [Tooltip("MapGraphRegions this component is going to abstract into a graph.")]
    [SerializeField] public MapGraphRegions graphRegions;
    [Tooltip("RegionGraph serialized backend.")] 
    [SerializeField, HideInInspector] 
    public RegionGraphResource regionGraphResource = new();
    [Tooltip("Component to find the shortest path between an origin boundary node " +
             "and the nearest node of a target neighbor region.")]
    [SerializeField] private DijkstraGraphPathFinder dijkstraPathFinder;
    
    [Header("DEBUG:")] 
    [SerializeField] public bool showGizmos;
    [SerializeField] public Color gridColor = Color.yellow;
    [SerializeField] public float nodeRadius = 0.1f;
    [SerializeField] public Color nodeColor = Color.orange;
    [SerializeField] public Vector2 gizmoTextOffset = new(10,10);
    
    /// <summary>
    /// Heuristic cost to traverse a region.
    /// </summary>
    private readonly Dictionary<uint, float> _regionTraversalCosts = new();
    
    /// <summary>
    /// The nodes our dijkstra pathfinder must reach to end the necessary calculation.
    /// </summary>
    private readonly HashSet<IPositionNode> _targetNodes = new();
    
    /// <summary>
    /// Retrieves the region ID for a given position in the map graph.
    /// </summary>
    /// <param name="position">The global position for which the corresponding region
    /// ID is to be determined.</param>
    /// <returns>The ID of the region that contains the specified position.</returns>
    public uint GetRegionIdByPosition(Vector2 position)
    {
        IPositionNode positionNode = graphRegions.mapGraph.GetNodeAtPosition(position);
        return graphRegions.GetRegionByNodeId(positionNode.Id);   
    }

    /// <summary>
    /// Retrieves the region node associated with the specified region ID.
    /// </summary>
    /// <param name="regionId">The unique identifier of the region whose corresponding
    /// RegionNode is to be retrieved.</param>
    /// <returns>The RegionNode object representing the region with the given ID.</returns>
    public RegionNode GetRegionNodeById(uint regionId)
    {
        return regionGraphResource.regionIdToRegionNode[regionId];
    }
    
    /// <summary>
    /// Returns the shortest path from a given node to a region.
    /// </summary>
    /// <param name="startNode">Starting node.</param>
    /// <param name="regionId">Target region.</param>
    /// <returns>A path to get the nearest node of the target region.</returns>
    public Path GetShortestPathToRegion(PositionNode startNode, uint regionId)
    {
        long nodeToRegion = RegionGraphResource.GetFromNodeToRegionKey(
            startNode.Id, 
            regionId);
        InterRegionPath interRegionPath =
            regionGraphResource.fromNodeToRegionPaths[nodeToRegion];
        List<Vector2> pathPositions = interRegionPath.pathPositions;
        Path returnedPath = new GameObject().AddComponent<Path>();
        returnedPath.UpdatePathData(pathPositions);
        return returnedPath;
    }
    
    /// <summary>
    /// Get the RegionNode related to a given region seed.
    /// </summary>
    /// <param name="nodeId">Region ID</param>
    /// <returns>The PositionNode where the given region seed is placed.</returns>
    public IPositionNode GetNodeById(uint nodeId)
    {
        RegionNode regionNode = regionGraphResource.regionIdToRegionNode[nodeId];
        return regionNode;
    }


    /// <summary>
    /// Retrieves a RegionNode located at the specified position in the graph.
    /// </summary>
    /// <param name="position">The position in the graph where the node is to be
    /// located.</param>
    /// <returns>The node located at the specified position, or the nearest node if
    /// one does not exist at the exact position. Null if nothing is found.</returns>
    public IPositionNode GetNodeAtPosition(Vector2 position)
    {
        RegionNode regionNode;
        regionNode = regionGraphResource.positionToRegionNode.ContainsKey(position) ? 
            regionGraphResource.positionToRegionNode[position]: 
            GetNodeAtNearestPosition(position);
        return regionNode;
    }

    /// <summary>
    /// Retrieves the region node located at the nearest position to the specified
    /// global position.
    /// </summary>
    /// <param name="globalPosition">The global position for which the nearest region
    /// node is determined.</param>
    /// <returns>The region node closest to the specified position.</returns>
    private RegionNode GetNodeAtNearestPosition(Vector2 globalPosition)
    {
        Vector2 nearestPosition = FindNearestPosition(globalPosition);
        RegionNode nearestNode = 
            regionGraphResource.positionToRegionNode[nearestPosition];
        return nearestNode;
    }

    /// <summary>
    /// Finds the nearest position in the graph to the specified target position.
    /// </summary>
    /// <param name="targetPosition">The position for which to find the nearest graph
    /// position.</param>
    /// <returns>The nearest position in the graph to the target position.</returns>
    private Vector2 FindNearestPosition(Vector2 targetPosition)
    {
        Vector2 nearestPosition = Vector2.zero;
        float minDistance = float.MaxValue;

        foreach (Vector2 key in regionGraphResource.positionToRegionNode.Keys)
        {
            float distance = Vector2.Distance(targetPosition, key);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestPosition = key;
            }
        }
        return nearestPosition;
    }
    
    public void GenerateRegionGraph()
    {
        if (dijkstraPathFinder.Graph == null) 
            dijkstraPathFinder.Graph = graphRegions.mapGraph;
        GenerateRegionNodes();
        CalculateRegionTraversalPaths();
        CalculateRegionTraversalCosts();
        EstablishRegionConnections();
    }

    /// <summary>
    /// Computes the traversal costs for each region based on inter-region paths.
    /// </summary>
    /// <remarks>
    /// This method calculates the average traversal cost for every region by aggregating
    /// the costs of paths crossing each region and determining the average cost per path.
    /// The result is stored in a dictionary where each region ID is associated with its
    /// average traversal cost.
    /// </remarks>
    private void CalculateRegionTraversalCosts()
    {
        // <CrossedRegion, (TotalCost, PathsCrossingAmount)>
        Dictionary<uint, (float, uint)> regionIdToGlobalCostAndPathCount = new();
        
        // Sum cost of every path crossing a region.
        foreach (KeyValuePair<
                     long, 
                     InterRegionPath> fromNodeToRegionPath 
                 in regionGraphResource.fromNodeToRegionPaths)
        {
            long nodeToRegionKey = fromNodeToRegionPath.Key;
            RegionGraphResource.SplitKey(
                nodeToRegionKey, 
                out uint fromNodeId, 
                out uint _);
            InterRegionPath interRegionPath = fromNodeToRegionPath.Value;
            uint crossedRegionId =
                graphRegions.GetRegionByNodeId(fromNodeId);
            if (!regionIdToGlobalCostAndPathCount.ContainsKey(crossedRegionId))
                regionIdToGlobalCostAndPathCount[crossedRegionId] = (0, 0);
            (float totalCost, uint pathCrossingAmount) =
                regionIdToGlobalCostAndPathCount[crossedRegionId];
            totalCost += interRegionPath.cost;
            pathCrossingAmount++;
            regionIdToGlobalCostAndPathCount[crossedRegionId] = 
                (totalCost, pathCrossingAmount);
        }

        // Calculate the average cost of every region.
        foreach (KeyValuePair<uint, (float, uint)> regionToCostAndCount in 
                 regionIdToGlobalCostAndPathCount)
        {
            uint regionId = regionToCostAndCount.Key;
            (float totalCost, uint pathCrossingAmount) = regionToCostAndCount.Value;
            _regionTraversalCosts[regionId] = totalCost / pathCrossingAmount;
        }
    }
    
    /// <summary>
    /// Ends Dijkstra calculation when every target node is reached.
    /// </summary>
    /// <returns>Returns true only when every target node has been reached;
    /// otherwise returns false.</returns>
    private bool EndCondition()
    {
        if (!_targetNodes.Contains(dijkstraPathFinder.currentNodeRecord.node)) 
            return false;
        _targetNodes.Remove(dijkstraPathFinder.currentNodeRecord.node);
        return _targetNodes.Count == 0;
    }

    /// <summary>
    /// Calculates the traversal paths between regions in the region graph.
    /// For each region, determines the shortest paths from its boundary nodes
    /// to the nearest boundary nodes of neighboring regions.
    /// </summary>
    private void CalculateRegionTraversalPaths()
    {
        // Region traversal cost is a heuristic that estimates the cost to completely
        // go through a region. So, the connection between two regions is estimated by
        // added the half of the region traversal cost of its region ends.
        regionGraphResource.fromNodeToRegionPaths.Clear();
        foreach (KeyValuePair<uint, RegionNode> regionIdToRegionNode in 
                 regionGraphResource.regionIdToRegionNode)
        {
            uint regionId = regionIdToRegionNode.Key;
            RegionNode regionNode = regionIdToRegionNode.Value;
            HashSet<uint> neighborRegionIds = new(regionNode.boundaryNodes.Keys);
            
            // For every boundary node get a path to the nearest boundary node of 
            // every neighbor region.
            foreach (KeyValuePair<uint, UintList> neighborRegionToBoundaryNodes in 
                     regionNode.boundaryNodes)
            {
                // Boundary nodes are going to be our starting points for the paths.
                List<uint> boundaryNodesTowardsNeighborRegion = 
                    neighborRegionToBoundaryNodes.Value.items;

                // Now calculate the shortest path from every boundary node of the current
                // region towards the nearest boundary node of every neighbor region.
                foreach (uint boundaryNodeId in boundaryNodesTowardsNeighborRegion)
                {
                    try
                    {
                        // Start node.
                        PositionNode boundaryNode = (PositionNode) 
                            graphRegions.mapGraph.GetNodeById(boundaryNodeId);
                        
                        // Gather possible targets. They are the boundary nodes in
                        // the neighbor regions.
                        _targetNodes.Clear();
                        foreach (uint neighborRegionId in neighborRegionIds)
                        {
                            // Get nodes from the neighbor region that are boundary to
                            // the current region.
                            List<uint> neighborBoundaryNodesIds = regionGraphResource
                                .regionIdToRegionNode[neighborRegionId]
                                .boundaryNodes[regionId].items;
                            foreach (uint neighborBoundaryNodeId in neighborBoundaryNodesIds)
                            {
                                PositionNode neighborBoundaryNode = (PositionNode)
                                    graphRegions.mapGraph.GetNodeById(
                                        neighborBoundaryNodeId);
                                _targetNodes.Add(neighborBoundaryNode);
                            }
                        }

                        // Map from the boundary node to every target node.
                        dijkstraPathFinder.CalculateCosts(boundaryNode, EndCondition);
                        
                        // Select the best paths to every neighbor region from the current
                        // boundary node.
                        foreach (uint neighborRegionId in neighborRegionIds)
                        {
                            // Get nodes from the neighbor that are boundary to the current
                            // region.
                            List<uint> neighborBoundaryNodesIds = regionGraphResource
                                .regionIdToRegionNode[neighborRegionId]
                                .boundaryNodes[regionId].items;
                            
                            long nodeToRegionKey = 
                                RegionGraphResource.GetFromNodeToRegionKey(
                                    boundaryNodeId, 
                                    neighborRegionId);
                            
                            // For every neighbor region, we keep only the path to its
                            // boundary node, which is nearest to the current boundary
                            // node we are using as the starting node.
                            foreach (uint targetNodeId in neighborBoundaryNodesIds)
                            {
                                PositionNode targetNode = (PositionNode)
                                    graphRegions.mapGraph.GetNodeById(targetNodeId);
                                if (!regionGraphResource.fromNodeToRegionPaths.ContainsKey(
                                        nodeToRegionKey) ||
                                    regionGraphResource.fromNodeToRegionPaths[nodeToRegionKey]
                                        .cost > dijkstraPathFinder.closedDict[targetNode]
                                        .costSoFar)
                                {
                                    regionGraphResource.fromNodeToRegionPaths[nodeToRegionKey] =
                                        new InterRegionPath()
                                        {
                                            cost = dijkstraPathFinder.closedDict[targetNode]
                                                .costSoFar,
                                            pathPositions = dijkstraPathFinder
                                                .BuildPath(dijkstraPathFinder.closedDict,
                                                    boundaryNode, targetNode).positions
                                        };
                                }
                            }
                        }
                    }
                    catch (KeyNotFoundException)
                    {
                        Debug.LogError($"Boundary node not found " +
                                       $"({boundaryNodeId}) when calculating " +
                                       $"paths for region {regionId}.");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Establishes connections between region nodes in the region graph by analyzing
    /// shared boundary nodes and calculating traversal costs. This ensures that each
    /// region node is properly connected to its neighboring region nodes, enabling
    /// efficient pathfinding across regions.
    /// </summary>
    private void EstablishRegionConnections()
    {
        // Once that region nodes are created, establish connections between them.
        foreach (KeyValuePair<uint, RegionNode> regionIdToRegionNode in 
                 regionGraphResource.regionIdToRegionNode)
        {
            uint regionId = regionIdToRegionNode.Key;
            RegionNode regionNode = regionIdToRegionNode.Value;
            // Create a connection with every region this one has boundary nodes with.
            foreach (KeyValuePair<uint, UintList> neighborRegionIdBoundaryNodes in 
                     regionNode.boundaryNodes)
            {
                uint neighborRegionId = neighborRegionIdBoundaryNodes.Key;
                RegionNode neighborRegionNode = 
                    regionGraphResource.regionIdToRegionNode[neighborRegionId];
                GraphConnection connection = new(
                        regionNode.Id,
                        neighborRegionNode.Id,
                        (_regionTraversalCosts[neighborRegionId] + 
                         _regionTraversalCosts[regionId]) / 2
                    );
                regionNode.Connections[neighborRegionId] = connection;
            }
        }
    }

    /// <summary>
    /// Generates and initializes region nodes that represent individual regions
    /// within the region graph. Each region node is created based on the regions
    /// defined in the MapGraphRegions and is associated with attributes such as
    /// position and connectivity.
    /// </summary>
    private void GenerateRegionNodes()
    {
        regionGraphResource.regionIdToRegionNode.Clear();
        regionGraphResource.positionToRegionNode.Clear();
        
        // Traverse every region to generate their region nodes.
        foreach (KeyValuePair<uint, HashSet<uint>> regionIdToNodesIdsByRegion in 
                 graphRegions.nodesByRegion)
        {
            uint regionId = regionIdToNodesIdsByRegion.Key;
            // Create a new region node to represent that region in the graph.
            RegionNode regionNode = new()
            {
                Id = regionId,
                Position = graphRegions.GetRegionCenter(regionId),
            };
            // Traverse every node in the region.
            HashSet<uint> nodeIdsInRegion = regionIdToNodesIdsByRegion.Value;
            foreach (uint nodeId in nodeIdsInRegion)
            {
                PositionNode node = 
                    (PositionNode) graphRegions.mapGraph.GetNodeById(nodeId);
                // Check if the node has connections to other regions.
                foreach (KeyValuePair<Orientation, GraphConnection> graphConnection in 
                         node.GetConnections())
                {
                    GraphConnection connection = graphConnection.Value;
                    uint otherNodeRegionId = 
                        graphRegions.GetRegionByNodeId(connection.endNodeId);
                    // If the node is connected to another region, add it to the region's
                    // boundary nodes.
                    if (otherNodeRegionId != regionId)
                    {
                        if (!regionNode.boundaryNodes.ContainsKey(otherNodeRegionId))
                            regionNode.boundaryNodes[otherNodeRegionId] = new();
                        regionNode.boundaryNodes[otherNodeRegionId].items.Add(nodeId);
                    }
                }
            }
            regionGraphResource.regionIdToRegionNode[regionId] = regionNode;
            regionGraphResource.positionToRegionNode[regionNode.Position] = regionNode;
        }
    }

}
}

