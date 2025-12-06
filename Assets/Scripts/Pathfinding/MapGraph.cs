
using System;
using System.Collections.Generic;
using OdinSerializer;
using Tools;
using UnityEngine;

namespace Pathfinding
{
/// <summary>
/// Represents a map-based graph system designed for pathfinding purposes. This class is
/// used to generate a graph structure based on a grid, which can be analyzed for
/// pathfinding operations. It supports dynamic configuration of map dimensions,
/// resolution, and collision layers.
/// </summary>
[ExecuteAlways]
public class MapGraph : SerializedMonoBehaviour
{
    [Header("CONFIGURATION:")]
    [Tooltip("Size of the map to analyze in physical units.")]
    public Vector2Int mapSize = new(18, 10);
    [Tooltip("Dimensions for the resulting array of nodes.")]
    public Vector2Int cellResolution = new(18, 10);
    [Tooltip("Layers to consider as not walkable.")]
    public LayerMask obstaclesLayers;

    [NonSerialized, OdinSerialize] private MapGraphResource _graphResource;
    
    [Header("DEBUG:")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color gridColor = Color.yellow;
    [SerializeField] private float nodeRadius = 0.1f;
    [SerializeField] private Color nodeColor = Color.orange;

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
    private Vector2 CellSize => mapSize / (Vector2) cellResolution;

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
    private Vector2 NodeGlobalPosition(Vector2Int nodeArrayPosition) => 
        nodeArrayPosition * CellSize + CellSize / 2;


    /// <summary>
    /// Converts a global world position of a node into its corresponding array position
    /// within the grid.
    /// </summary>
    /// <param name="nodeGlobalPosition">
    /// The global position of the node in world space, represented as a Vector2.
    /// This is the physical location of the node.
    /// </param>
    /// <returns>
    /// A Vector2Int representing the position of the node within the grid array.
    /// This corresponds to the node's indices in the grid.
    /// </returns>
    private Vector2Int NodeArrayPosition(Vector2 nodeGlobalPosition) =>
        Vector2Int.RoundToInt((nodeGlobalPosition - CellSize / 2) / CellSize);
    
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
    private Vector2Int GetNeighborRelativeArrayPosition(Orientation orientation)
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
        _graphResource.nodes.Clear();
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
                GraphNode node = new()
                {
                    position = nodeGlobalPosition
                };
                foreach (Orientation orientation in Enum.GetValues(typeof(Orientation)))
                {
                    // If the newly created node is adjacent to an existing node, we
                    // create an edge between them.
                    Vector2Int neighborArrayPosition =
                        GetNeighborRelativeArrayPosition(orientation) + 
                        nodeArrayPosition;
                    if (!_graphResource.nodes.ContainsKey(neighborArrayPosition)) 
                        continue;
                    node.AddEdge(
                        _graphResource.nodes[neighborArrayPosition], 
                        1, 
                        orientation);
                }
                // Once the node is created and configures, we add it to the graph.
                _graphResource.nodes.Add(nodeArrayPosition, node);
            }
        }
    }

    private void Awake()
    {
        _cleanAreaChecker = new CleanAreaChecker(
            // If you make the radius exactly half the cell size, _cleanAreaChecker
            // will touch the adjacent wall. Thas why we subtract 0.1f.
            (Mathf.Min(CellSize.x, CellSize.y)/2)-0.1f, 
            obstaclesLayers);
        if (_graphResource == null) _graphResource = new();
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

        if (_graphResource.nodes.Count == 0) return;
        
        // Draw nodes and their edges.
        Gizmos.color = nodeColor;
        foreach (KeyValuePair<Vector2Int, GraphNode> nodeEntry in _graphResource.nodes)
        {
            Vector2 cellPosition = NodeGlobalPosition(nodeEntry.Key);
            GraphNode node = nodeEntry.Value;
            Gizmos.DrawWireSphere(cellPosition, nodeRadius);
            foreach (Orientation orientation in Enum.GetValues(typeof(Orientation)))
            {
                if (node.edges.ContainsKey(orientation))
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

