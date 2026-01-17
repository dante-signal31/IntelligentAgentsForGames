using System;
using System.Collections.Generic;
using Pathfinding;
using UnityEditor;
using UnityEngine;

namespace Editor.Inspectors
{
[CustomEditor(typeof(MapGraph))]
public partial class MapGraphEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        // Add some space before the button
        EditorGUILayout.Space(10);

        // Get the MapGraph component reference
        MapGraph mapGraph = (MapGraph)target;

        // Create and handle the reset button
        if (GUILayout.Button("Generate Graph", GUILayout.Height(30)))
        {
            mapGraph.GenerateGraph();
            EditorUtility.SetDirty(target);
        }
    }
    
    private void OnSceneGUI()
    {
        var graph = (MapGraph)target;
        
        var textStyle = new GUIStyle(EditorStyles.label)
        {
            normal = { textColor = graph.GridColor }
        };
       
        foreach (KeyValuePair<Vector2Int, PositionNode> nodeEntry in graph.Nodes)
        {
            Vector2 cellPosition = graph.NodeGlobalPosition(nodeEntry.Key);
            PositionNode node = nodeEntry.Value;
            if (node.connections == null) continue;
            foreach (Orientation orientation in Enum.GetValues(typeof(Orientation)))
            {
                if (node.connections.TryGetValue(orientation, out var connection))
                {
                    Vector2 otherNodeRelativePosition = 
                        graph.GetNeighborRelativeArrayPosition(orientation);
                    Vector2 otherNodePosition = cellPosition + 
                                                otherNodeRelativePosition * 
                                                graph.CellSize;
                    Handles.Label(
                        cellPosition + (otherNodePosition - cellPosition) / 2, 
                        connection.cost.ToString("G"),
                        textStyle);
                }
            }
        }
    }
}
}