using System;
using System.Collections.Generic;
using Levels.Tiles;
using Tools;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Pathfinding
{
/// <summary>
/// <p>Represents a map-based graph system designed for pathfinding purposes. This class
/// is used to generate a graph structure based on a grid, which can be analyzed for
/// pathfinding operations. It supports dynamic configuration of map dimensions,
/// resolution, and collision layers.</p>
/// <p>Ideally, a graph does not need to have any spatial structure, but if you are
/// going to use the graph for pathfinding purposes, it is recommended to use a spatial
/// structure to improve performance. Otherwise, you will be forced to iterate over all
/// nodes in the graph every time you want to find the nearest node to a given position.
/// </p>
/// <remarks>For this component to work properly, it's transform must be placed at
/// global coordinates origin. Besides, the scene to map must spread from the global
/// coordinates origin <b>and only towards the positive side of the axis</b>.</remarks>
/// </summary>
[ExecuteAlways]
public class MapGraph : MonoBehaviour
{
    [Header("CONFIGURATION:")]
    [Tooltip("Size of the map to analyze in physical units.")]
    public Vector2Int mapSize = new(18, 10);
    [Tooltip("Dimensions for the resulting array of nodes.")]
    public Vector2Int cellResolution = new(18, 10);
    [Tooltip("Layers to consider as not walkable.")]
    public LayerMask obstaclesLayers;
    [Tooltip("Walkable tilemap")]
    public Tilemap walkableTilemap;
    
    /// <summary>
    /// MapGraph serialized backend.
    /// </summary>
    [SerializeField, HideInInspector] private MapGraphResource graphResource = new();
    
    [Header("DEBUG:")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color gridColor = Color.yellow;
    [SerializeField] private float nodeRadius = 0.1f;
    [SerializeField] private Color nodeColor = Color.orange;

    // Used to draw connection cost when debugging.
    public Color GridColor => gridColor;
    
    /// <summary>
    /// Represents the physical size of each cell in the grid based on the map dimensions
    /// and the resolution of the grid.
    /// </summary>
    /// <remarks>
    /// The cell size is calculated as the map's physical size divided by the resolution
    /// of the grid (number of cells in each dimension). It determines the scale of each
    /// grid cell in world units and is used for converting between global positions
    /// and grid array indices.
    /// </remarks>
    public Vector2 CellSize => mapSize / (Vector2) cellResolution;

    /// <summary>
    /// Calculates the global world position of a node based on its array position 
    /// within the grid.
    /// </summary>
    /// <param name="nodeArrayPosition">
    /// The position of the node within the grid as a Vector2Int.
    /// This represents the indices of the node in the grid.
    /// </param>
    /// <returns>
    /// A Vector2 representing the global world position of the node in physical space.
    /// </returns>
    public Vector2 NodeGlobalPosition(Vector2Int nodeArrayPosition) => 
        nodeArrayPosition * CellSize + CellSize / 2;


    /// <summary>
    /// Converts a global world position into its nearest corresponding array position
    /// within the grid.
    /// </summary>
    /// <param name="globalPosition"> The global position, represented as a Vector2.
    /// </param>
    /// <returns>
    /// A Vector2Int representing the position within the grid array. If the global
    /// position was from a node, then this corresponds to the node's indices in the grid.
    /// </returns>
    private Vector2Int GlobalToArrayPosition(Vector2 globalPosition)
    {
        Vector2 arrayPosition = globalPosition / CellSize;
        Vector2Int groundRoundedArrayPosition = new Vector2Int(
            Mathf.FloorToInt(arrayPosition.x),
            Mathf.FloorToInt(arrayPosition.y)
        );
        return groundRoundedArrayPosition;
    }

    /// <summary>
    /// Retrieves the array position of a node based on its unique identifier.
    /// </summary>
    /// <param name="nodeId">
    /// The unique identifier of the node, represented as an unsigned integer.
    /// This ID is used to map the node to its corresponding array position.
    /// </param>
    /// <returns>
    /// A Vector2Int representing the position of the node within the grid's array
    /// structure.
    /// </returns>
    private Vector2Int GetArrayPositionById(uint nodeId) => 
        graphResource.nodeArrayPositionsById[nodeId];

    /// <summary>
    /// Retrieves a node from the graph using its unique identifier.
    /// </summary>
    /// <param name="nodeId">
    /// The unique identifier of the node to retrieve.
    /// </param>
    /// <returns>
    /// The <see cref="PositionNode"/> associated with the specified ID.
    /// </returns>
    public PositionNode GetNodeById(uint nodeId) => 
        graphResource.nodes[GetArrayPositionById(nodeId)];
    
    /// <summary>
    /// Retrieves the GraphNode located at the given global world position.
    /// </summary>
    /// <param name="globalPosition">
    /// The global world position represented as a Vector2, specifying where to search
    /// for the node in the graph.
    /// </param>
    /// <returns>
    /// The GraphNode instance at the specified global position if it exists;
    /// otherwise, null.
    /// </returns>
    public PositionNode GetNodeAtPosition(Vector2 globalPosition)
    {
        Vector2Int arrayPosition = GlobalToArrayPosition(globalPosition);
        if (!graphResource.nodes.ContainsKey(arrayPosition)) return null;
        return graphResource.nodes[arrayPosition];
    }

    /// <summary>
    /// Just a shortcut to the graph nodes dictionary inside GraphResource.
    /// </summary>
    public Dictionary<Vector2Int, PositionNode> Nodes => graphResource.nodes;
    
    private CleanAreaChecker _cleanAreaChecker;

    /// <summary>
    /// Determines the relative array position of a neighboring node based on the
    /// specified orientation.
    /// </summary>
    /// <param name="orientation">
    /// The direction in which the neighboring node is located relative to the current
    /// node. Possible values are defined in the <see cref="Orientation"/> enum.
    /// </param>
    /// <returns>
    /// A Vector2Int representing the relative array position of the neighboring node
    /// based on the given orientation.
    /// </returns>
    public Vector2Int GetNeighborRelativeArrayPosition(Orientation orientation)
    {
        Vector2 relativePosition = Vector2.zero;
        switch (orientation)
        {
            case Orientation.North:
                relativePosition = Vector2.up;
                break;
            case Orientation.East:
                relativePosition = Vector2.right;
                break;
            case Orientation.South:
                relativePosition = Vector2.down;
                break;
            case Orientation.West:
                relativePosition = Vector2.left;
                break;
        }
        return Vector2Int.RoundToInt(relativePosition);
    }

    /// <summary>
    /// Generates a navigable graph representation of the map based on the configured
    /// resolution, size, and obstacle layers. This method analyzes the defined grid
    /// space, identifies valid nodes, and establishes connections between them to form
    /// a graph structure suitable for pathfinding algorithms.
    /// </summary>
    public void GenerateGraph()
    {
        ClearGraph();
        for (int x = 0; x < cellResolution.x; x++)
        {
            for (int y = 0; y < cellResolution.y; y++)
            {
                Vector2Int nodeArrayPosition = new(x, y);
                Vector2 nodeGlobalPosition = NodeGlobalPosition(nodeArrayPosition);
                
                // If there is any obstacle at that position, we don't create any node.
                if (!_cleanAreaChecker.IsCleanArea(nodeGlobalPosition))
                    continue;
                
                // If the position is clean, create a node.
                PositionNode node = new(nodeGlobalPosition);
                
                float nodeCost = GetPositionCost(nodeGlobalPosition);
                // Node cost is the cost to go through the node. So, its connection cost
                // is the half of the node cost. One half to enter the node and the other
                // half to exit it.
                float connectionCost = nodeCost / 2;
                
                // Populate new node's connections.
                foreach (Orientation orientation in Enum.GetValues(typeof(Orientation)))
                {
                    // If the newly created node is adjacent to an existing node, we
                    // create a connection between them.
                    Vector2Int neighborArrayPosition =
                        GetNeighborRelativeArrayPosition(orientation) + 
                        nodeArrayPosition;
                    if (!graphResource.nodes.ContainsKey(neighborArrayPosition)) 
                        continue;
                    // Get the cost to enter the neighbor node.
                    Vector2 neighborGlobalPosition = 
                        NodeGlobalPosition(neighborArrayPosition);
                    float neighbourCost = GetPositionCost(neighborGlobalPosition);
                    float neighborConnectionCost = neighbourCost / 2;
                    // Direct connection between this node and the neighbor.
                    PositionNode neighborNode = 
                        graphResource.nodes[neighborArrayPosition];
                    node.AddConnection(
                        neighborNode.Id,
                        // The half cost to leave the current node, and the half cost to
                        // enter the neighbor node.
                        connectionCost + neighborConnectionCost,
                        orientation);
                    // Conversely, as our connections are bidirectional, we must set up
                    // also the reciprocal connection from the neighbor to this node. 
                    Orientation reciprocalOrientation = Orientation.North;
                    switch (orientation)
                    {
                        case Orientation.North: 
                            reciprocalOrientation = Orientation.South; 
                            break;
                        case Orientation.East:
                            reciprocalOrientation = Orientation.West; 
                            break;
                        case Orientation.South:
                            reciprocalOrientation = Orientation.North;
                            break;
                        case Orientation.West:
                            reciprocalOrientation = Orientation.East;
                            break;
                    }
                    neighborNode.AddConnection(
                        node.Id, 
                        connectionCost + neighborConnectionCost, 
                        reciprocalOrientation);
                }
                // Once the node is created and configured, we add it to the graph.
                AddNodeToGraph(nodeArrayPosition, node);
            }
        }
#if UNITY_EDITOR
        // Mark the scene as dirty to serialize changes.
        EditorUtility.SetDirty(this);
#endif
    }

    private void OnEnable()
    {
        _cleanAreaChecker = new CleanAreaChecker(
            // If you make the radius exactly half the cell size, _cleanAreaChecker
            // will touch the adjacent wall. Thas why we subtract 0.1f.
            (Mathf.Min(CellSize.x, CellSize.y)/2)-0.1f, 
            obstaclesLayers);
    }

    private void OnDisable()
    {
        _cleanAreaChecker?.Dispose();
    }

    /// <summary>
    /// Clears the existing graph by removing all nodes and their associated data
    /// from the internal data structures.
    /// </summary>
    private void ClearGraph()
    {
        graphResource.nodes.Clear();
        graphResource.nodeArrayPositionsById.Clear();
    }

    /// <summary>
    /// Adds a new node to the graph at a specified array position and updates the
    /// internal mappings of node positions and IDs.
    /// </summary>
    /// <param name="nodeArrayPosition">
    /// The array position of the node within the grid, represented as a Vector2Int.
    /// This denotes the node's location in the grid structure.
    /// </param>
    /// <param name="node">
    /// The node to be added to the graph. It contains information about its unique ID
    /// and additional properties required for graph traversal.
    /// </param>
    private void AddNodeToGraph(Vector2Int nodeArrayPosition, PositionNode node)
    {
        graphResource.nodes.Add(nodeArrayPosition, node);
        graphResource.nodeArrayPositionsById.Add(node.Id, nodeArrayPosition);
    }

    /// <summary>
    /// Calculates the movement cost associated with a specific global position on the
    /// map. This cost represents the effort required to traverse the tile at the given
    /// position.
    /// </summary>
    /// <param name="neighborGlobalPosition">The global position of the tile
    /// to be evaluated.</param>
    /// <return>The movement cost as a float, derived from the custom data associated
    /// with the tile.</return>
    private float GetPositionCost(Vector2 neighborGlobalPosition)
    {
        Vector3Int cellPosition = walkableTilemap.WorldToCell(neighborGlobalPosition);
        CourtyardTile tile = walkableTilemap.GetTile<CourtyardTile>(cellPosition);
        return tile.Cost;
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        Vector2 cellSize = CellSize;

        // Draw the grid.
        // Draw vertical lines.
        Gizmos.color = gridColor;
        for (float x = 0; x < mapSize.x; x+=cellSize.x)
        {
            Gizmos.DrawLine(
                transform.TransformPoint(new Vector2(x, 0)), 
                transform.TransformPoint(new Vector2(x, mapSize.y)));
        }
        // Draw horizontal lines.
        for (float y = 0; y < mapSize.y; y += cellSize.y)
        {
            Gizmos.DrawLine(
                transform.TransformPoint(new Vector2(0, y)),
                transform.TransformPoint(new Vector2(mapSize.x, y)));
        }

        if (graphResource == null) return;
        if (graphResource.nodes == null) return;
        if (graphResource.nodes.Count == 0) return;
        
        // Draw nodes and their connections.
        Gizmos.color = nodeColor;
        foreach (KeyValuePair<Vector2Int, PositionNode> nodeEntry in graphResource.nodes)
        {
            Vector2 cellPosition = NodeGlobalPosition(nodeEntry.Key);
            PositionNode node = nodeEntry.Value;
            Gizmos.DrawWireSphere(cellPosition, nodeRadius);
            if (node.connections == null) continue;
            foreach (Orientation orientation in Enum.GetValues(typeof(Orientation)))
            {
                if (node.connections.ContainsKey(orientation))
                {
                    Vector2 otherNodeRelativePosition = 
                        GetNeighborRelativeArrayPosition(orientation);
                    Vector2 otherNodePosition = cellPosition + 
                                                otherNodeRelativePosition * CellSize;
                    Gizmos.DrawLine(cellPosition, otherNodePosition);
                }
            }
        }
    }
#endif
}
}

