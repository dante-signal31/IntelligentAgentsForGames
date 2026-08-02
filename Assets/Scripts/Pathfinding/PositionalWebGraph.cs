
using System;
using System.Collections.Generic;
using System.Numerics;
using Sensors;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;

namespace Pathfinding
{ 
[ExecuteAlways]
public class PositionalWebGraph : MonoBehaviour, IPositionGraph
{
    [Header("CONFIGURATION:")]
    [SerializeField] private PositionNode[] nodes;
    [SerializeField] LayerMask obstaclesLayers;
    [SerializeField] private float lineOfSightRange;
    
    [Header("WIRING:")]
    [SerializeField] private RaySensor raySensor;
    
    [Header("DEBUG:")]
    [SerializeField] public bool showGizmos = true;
    [SerializeField] public Color gizmosColor = Color.yellow;
    [SerializeField] public bool showNodesId = true;
    [SerializeField] public float gizmoRadius = 0.1f;
    [SerializeField] public Vector2 gizmoTextOffset = new(0.2f, 0.2f);
    [SerializeField] public float arrowOffset = 0.75f;
    [SerializeField] public float arrowLength = 0.5f;
    [SerializeField] public float arrowApertureDegrees = 30f;

    public IReadOnlyList<PositionNode> Nodes => nodes;
    
    public IPositionNode GetNodeById(uint nodeId)
    {
        foreach (PositionNode node in nodes)
        {
            if (node.Id == nodeId) return node;
        }
        
        return null;
    }

    public IPositionNode GetNodeAtPosition(Vector2 position)
    {
        throw new System.NotImplementedException();
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
        foreach (GraphNode node in nodes)
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

