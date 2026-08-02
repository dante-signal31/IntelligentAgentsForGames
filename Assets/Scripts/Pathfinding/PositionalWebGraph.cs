
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
public class PositionalWebGraph : MonoBehaviour, IPositionGraph
{
    [Header("CONFIGURATION:")]
    [SerializeField] private PositionNode[] nodes;
    [SerializeField] LayerMask obstaclesLayers;
    [SerializeField] private float lineOfSightRange;
    
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

    public void GenerateConnections()
    {
        throw new System.NotImplementedException();
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

