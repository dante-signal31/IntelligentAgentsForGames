using System.Collections.Generic;
using Sensors;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;

namespace Pathfinding
{
/// <summary>
/// Represents a graph structure designed for pathfinding and navigation,
/// where each node has an associated position in a 2D space.
/// </summary>
/// <remarks>
/// This class is implemented as a MonoBehaviour, enabling it to be used
/// within Unity's scene-based environment. The <c>PositionalWebGraph</c> allows
/// for defining nodes, rendering debug visualizations such as gizmos, and generating
/// connections between nodes for pathfinding purposes.
/// </remarks>
public class PositionalWebGraph : MonoBehaviour, IPositionGraph
{
    [Header("CONFIGURATION:")]
    [Tooltip("Layers to consider as not walkable.")]
    [SerializeField] LayerMask obstaclesLayers;
    [Tooltip("Maximum distance between two nodes for line-of-sight checks.")]
    [SerializeField] private float lineOfSightRange;
    
    [Header("WIRING:")]
    [Tooltip("Ray sensor used for line-of-sight checks.")]
    [SerializeField] private RaySensor raySensor;
    
    [Header("DEBUG:")]
    [Tooltip("Whether to show gizmos.")]
    [SerializeField] public bool showGizmos = true;
    [Tooltip("Color to show gizmos.")]
    [SerializeField] public Color gizmosColor = Color.yellow;
    [Tooltip("Whether to show node IDs.")]
    [SerializeField] public bool showNodesId = true;
    [Tooltip("Radius for the gizmos that mark the nodes.")]
    [SerializeField] public float gizmoRadius = 0.1f;
    [Tooltip("Offset por the text to show connection cost.")]
    [SerializeField] public Vector2 gizmoTextOffset = new(0.2f, 0.2f);
    [Tooltip("Over one position to draw the arrow head for connection direction.")]
    [SerializeField] public float arrowOffset = 0.75f;
    [Tooltip("Length of the arrow wings.")]
    [SerializeField] public float arrowLength = 0.5f;
    [Tooltip("Angle in degrees of the arrow wings. [0, 90)")]
    [Range(0f, 90f)]
    [SerializeField] public float arrowApertureDegrees = 30f;
    
    [Space]
    // Do not show. We will use our custom drawer instead.
    [SerializeField, HideInInspector] private PositionNode[] nodes;

    public IReadOnlyList<PositionNode> Nodes => nodes;


    /// <summary>
    /// Retrieves the node corresponding to the specified unique identifier.
    /// </summary>
    /// <param name="nodeId">The unique identifier of the node to retrieve.</param>
    /// <returns>
    /// The <see cref="IPositionNode"/> that matches the specified identifier,
    /// or <c>null</c> if no such node exists in the graph.
    /// </returns>
    public IPositionNode GetNodeById(uint nodeId)
    {
        foreach (PositionNode node in nodes)
        {
            if (node.Id == nodeId) return node;
        }
        
        return null;
    }

    /// <summary>
    /// Retrieves the nearest node to the specified position that has a clear
    /// line-of-sight.
    /// </summary>
    /// <param name="position">The position in the map for which the nearest node is
    /// being queried.</param>
    /// <returns>
    /// The nearest <see cref="IPositionNode"/> to the specified position that satisfies
    /// the conditions of proximity and unobstructed line-of-sight, or <c>null</c> if
    /// no such node is found.
    /// </returns>
    public IPositionNode GetNodeAtPosition(Vector2 position)
    {
        PositionNode nearestNode = null;
        float minDistance = float.MaxValue;
        
        foreach (PositionNode node in nodes)
        {
            // If farther than the current nearest node, skip it.
            float currentDistance = Vector2.Distance(node.Position, position);
            if (currentDistance > minDistance) continue;
            
            // If there is an obstacle in the way, skip it.
            raySensor.GlobalStartPosition = node.Position;
            raySensor.GlobalEndPosition = position;
            raySensor.UpdateRay();
            if (raySensor.AnyObjectDetected) continue;
            
            // Otherwise, this is the nearest node so far.
            minDistance = currentDistance;
            nearestNode = node;
        }

        return nearestNode;
    }

    /// <summary>
    /// Generates connections between nodes using line-of-sight checks.
    /// This method is used to dynamically rebuild the graph, ensuring nodes are
    /// connected based on their current positions and environment.
    /// </summary>
    /// <remarks>
    /// <ul>
    /// <li> Existing connections in all nodes will be cleared before generating new
    /// ones.</li>
    /// <li> Nodes too far apart (beyond the configured line-of-sight range) are excluded
    /// from connection attempts.</li>
    /// <li> Connections are only established if there are no obstacles blocking the
    /// direct line-of-sight between two nodes.</li>
    /// <li> The connections generated are bidirectional, with symmetric costs.</li>
    /// </ul>
    /// </remarks>
    public void GenerateConnections()
    {
        raySensor.SensorLayerMask = obstaclesLayers;
        
        foreach (PositionNode node in nodes)
        {
            node.Connections.Clear();
            
            uint counter = 0;
            // Center ray sensor on the current node.
            raySensor.GlobalStartPosition = node.Position;
            foreach (PositionNode otherNode in nodes)
            {
                // Don't check against yourself.
                if (node == otherNode) continue;
                
                // Don't check nodes that are too far away.
                float distance = Vector2.Distance(node.Position, otherNode.Position);
                if (distance > lineOfSightRange) continue;
                
                // Point ray sensor to the other node.
                raySensor.GlobalEndPosition = otherNode.Position;
                raySensor.UpdateRay();
                if (!raySensor.AnyObjectDetected)
                {
                    // If the ray sensor does not detect any obstacle, then create a
                    // connection between the two nodes.
                    node.AddConnection(otherNode.Id, distance, counter);
                    counter++;
                }
            }
        }
    }

    private void Awake()
    {
        raySensor.SensorLayerMask = obstaclesLayers;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (nodes == null) return;

        HashSet<uint> ids = new();
        foreach (PositionNode node in nodes)
        {
            if (ids.Contains(node.Id) || node.Id == 0) 
                node.RegenerateId();
            ids.Add(node.Id);
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        Gizmos.color = gizmosColor;

        foreach (PositionNode node in nodes)
        {
            foreach (KeyValuePair<uint, GraphConnection> uintToGraphConnection in 
                     node.Connections)
            {
                GraphConnection connection = uintToGraphConnection.Value;
                PositionNode startPositionNode = 
                    (PositionNode) GetNodeById(connection.startNodeId);
                PositionNode endPositionNode = 
                    (PositionNode) GetNodeById(connection.endNodeId);
                
                if (startPositionNode == null || endPositionNode == null) continue;
                
                // Draw a line between the two nodes.
                Gizmos.DrawLine(
                    startPositionNode.Position, 
                    endPositionNode.Position);
                
                // Draw direction arrow.
                //
                // Get the arrow point position.
                Vector2 direction = endPositionNode.Position - startPositionNode.Position;
                Vector2 arrowPosition = startPositionNode.Position + 
                                         direction.normalized * 
                                         direction.magnitude * arrowOffset;
                
                // Get the arrow wings.
                Vector2 inverseDirection = -direction.normalized;
                Vector2 arrowLine1 =
                    Quaternion.Euler(0f, 0f, arrowApertureDegrees) *
                    inverseDirection *
                    arrowLength;
                Vector2 arrowLine2 =
                    Quaternion.Euler(0f, 0f, -arrowApertureDegrees) *
                    inverseDirection *
                    arrowLength;

                // Draw arrow head.
                Gizmos.DrawLine(arrowPosition, arrowPosition + arrowLine1);
                Gizmos.DrawLine(arrowPosition, arrowPosition + arrowLine2);
            }
        }
    }
#endif
}
}

