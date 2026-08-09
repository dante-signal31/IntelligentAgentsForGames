using System.Collections.Generic;
using Pathfinding;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor
{
[CustomEditor(typeof(PositionalWebGraph))]
public class DrawPositionalWebGraph : UnityEditor.Editor
{
    private readonly List<Vector2> _nodeHandles = new();
    private SerializedProperty _nodesProperty;
    private ListView _nodesListView;
        
    private void OnSceneGUI()
    {
        // Handles management.
        var graph = (PositionalWebGraph) target;
     
        var textStyle = new GUIStyle(EditorStyles.label)
        {
            normal = { textColor = graph.gizmosColor }
        };
        
        Handles.color = graph.gizmosColor;

        EditorGUI.BeginChangeCheck();
        
        _nodeHandles.Clear();
        foreach (var node in graph.Nodes)
        {
            // Draw handle to place the node visually.
            _nodeHandles.Add(Handles.PositionHandle(
                node.Position, 
                Quaternion.identity));   
            
            // Highlight the handle with a circle.
            Handles.DrawWireDisc(
                node.Position,
                Vector3.forward,
                graph.gizmoRadius);
            
            // Show the id number.
            if (graph.showNodesId)
            {
                Vector2 textPosition = node.Position + graph.gizmoTextOffset;
                Handles.Label(textPosition, $"{node.Id}", textStyle);
            }
        }
        
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(graph, "Position node changed location.");
            
            // Update the node positions with the changes in the handles.
            for (int i = 0; i < _nodeHandles.Count; i++)
            {
                graph.Nodes[i].Position = _nodeHandles[i];
            }
            
            // Force inspector update.
            EditorUtility.SetDirty(graph);
        }
        
        foreach (PositionNode positionNode in graph.Nodes)
        {
            foreach (KeyValuePair<uint, GraphConnection> uintToConnection in 
                     positionNode.Connections)
            {
                GraphConnection connection = uintToConnection.Value;
                PositionNode startPositionNode = 
                    (PositionNode) graph.GetNodeById(connection.startNodeId);
                PositionNode endPositionNode = 
                    (PositionNode) graph.GetNodeById(connection.endNodeId);
                
                if (startPositionNode == null || endPositionNode == null) continue;
                
                // Show a label with the connection cost.
                Vector2 direction = endPositionNode.Position - startPositionNode.Position;
                Vector2 arrowPosition = startPositionNode.Position + 
                                        direction.normalized * 
                                        (direction.magnitude * graph.arrowOffset);
                Vector2 textPosition = arrowPosition + graph.gizmoTextOffset;
                Handles.Label(
                    textPosition, 
                    connection.cost.ToString("F2"),
                    textStyle);
            }
        }
    }
    
    private void OnEnable()
    {
        _nodesProperty = serializedObject.FindProperty("nodes");
    }
    
    public override VisualElement CreateInspectorGUI()
    {
        // Do not use base.CreateInspectorGUI() to draw the default inspector. It can use
        // IMGUI, which is not supported in the UI Toolkit. If you want to force
        // UI-Toolkit, use InspectorElement.FillDefaultInspector().
        VisualElement root = new();

        InspectorElement.FillDefaultInspector(root, serializedObject, this);
        
        // Create new ListView and bind it to the nodes field.
        _nodesListView = new ListView();
        _nodesListView.bindingPath = _nodesProperty.propertyPath;
        _nodesListView.showBorder = true;
        _nodesListView.showFoldoutHeader = true;
        _nodesListView.headerTitle = "NODES:";
        _nodesListView.showAddRemoveFooter = true;
        _nodesListView.reorderable = false;
        _nodesListView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
        _nodesListView.itemsRemoved += OnNodeRemoved;
        root.Add(_nodesListView);

        // Show a warning box to not push the auto-generate button if you want to
        // keep any hand-made connection.
        HelpBox warningBox = new(
            "Be aware that the button below will regenerate every connection in " +
            "the graph using a line-of-sight check. So, any previous connection will " +
            "be lost.", 
            HelpBoxMessageType.Warning);
        root.Add(warningBox);

        Button bakeButton = new(() =>
        {
            // Update graph.
            var graph = (PositionalWebGraph) target;
            Undo.RecordObject(graph, "Bake graph connections");
            graph.GenerateConnections();
            
            // Force inspector update.
            serializedObject.Update();
            _nodesListView.Rebuild();
            EditorUtility.SetDirty(graph);
        });
        bakeButton.text = "Bake Connections";
        root.Add(bakeButton);
        
        return root;
    }

    private void OnNodeRemoved(IEnumerable<int> removedIndices)
    {
        // Read-only access to original object.
        var graph = (PositionalWebGraph)target;
        
        // Look for any connection pointing to the removed node.
        //
        // int = node Index, List<uint> = connection IDs to remove in that node.
        Dictionary <int, List<uint>> connectionIndicesToRemove = new();
        foreach (int removedIndex in removedIndices)
        {
            if (removedIndex >= 0 && removedIndex < graph.Nodes.Count)
            {
                uint removedID = graph.Nodes[removedIndex].Id;
                // Do not remove any connection while searching, or you will get alerts
                // of being traversing modified collections.
                for (int nodeIndex = 0; nodeIndex < graph.Nodes.Count; nodeIndex++)
                {
                    foreach (uint connectionKey in 
                             graph.Nodes[nodeIndex].Connections.Keys)
                    {
                        if (graph.Nodes[nodeIndex].Connections[connectionKey]
                                .endNodeId != removedID) continue;
                        if (!connectionIndicesToRemove.ContainsKey(nodeIndex))
                            connectionIndicesToRemove[nodeIndex] = new();
                        connectionIndicesToRemove[nodeIndex].Add(connectionKey);
                    }
                }
            }
        }
        
        // Remove any connection pointing to removed node.
        foreach (KeyValuePair<int, List<uint>> indexToIDsToRemove in connectionIndicesToRemove)
        {
            int nodeIndex = indexToIDsToRemove.Key;
            List<uint> idsToRemove = indexToIDsToRemove.Value;
            PositionNode node = graph.Nodes[nodeIndex];
            foreach (uint id in idsToRemove)
            {
                node.Connections.Remove(id);
            }
            _nodesProperty.GetArrayElementAtIndex(nodeIndex).boxedValue = node;
        }
        
        // Apply changes to the original object.
        serializedObject.ApplyModifiedProperties();
    }
}
}