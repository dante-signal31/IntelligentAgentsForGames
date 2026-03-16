using System.Collections.Generic;
using Pathfinding;
using UnityEditor;
using UnityEngine;

namespace Editor.Inspectors
{
[CustomEditor(typeof(RegionGraph))]
public class RegionGraphEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        // Add some space before the button
        EditorGUILayout.Space(10);

        // Get the component reference
        RegionGraph regionGraph = (RegionGraph)target;

        // Create and handle the reset button
        if (GUILayout.Button("Bake Region Graph", GUILayout.Height(30)))
        {
            regionGraph.GenerateRegionGraph();
            EditorUtility.SetDirty(target);
        }
    }
    
    private void OnSceneGUI()
    {
        var regionGraph = (RegionGraph)target;
    
        if (regionGraph == null) return;
        
        if (!regionGraph.showGizmos) return;
        
        var textStyle = new GUIStyle(EditorStyles.label)
        {
            normal = { textColor = regionGraph.nodeColor }
        };
        
        // Draw region center positions.
        foreach (KeyValuePair<uint, RegionNode> regionIdToRegionNode in 
                 regionGraph.regionGraphResource.regionIdToRegionNode)
        {
            RegionNode regionNode = regionIdToRegionNode.Value;
            Handles.color = regionGraph.nodeColor;
            Handles.DrawSolidDisc(
                regionNode.Position,
                Vector3.forward,
                regionGraph.nodeRadius);
            // Draw region number on the region center.
            Handles.Label(
                regionNode.Position + regionGraph.gizmoTextOffset, 
                regionNode.Id.ToString(),
                textStyle);
            
            // Draw connections between regions.
            Handles.color = regionGraph.gridColor;
            foreach (KeyValuePair<uint, GraphConnection> regionNodeConnection in 
                     regionNode.Connections)
            {
                GraphConnection connection = regionNodeConnection.Value;
                RegionNode endRegionNode = 
                    regionGraph.regionGraphResource.regionIdToRegionNode[connection.endNodeId];
                Handles.DrawLine(regionNode.Position, endRegionNode.Position);
                // Draw connection cost.
                Handles.Label(
                    regionNode.Position + 
                    (endRegionNode.Position - regionNode.Position) / 2 + 
                    regionGraph.gizmoTextOffset, 
                    connection.cost.ToString("F2"),
                    textStyle);
            }
        }
    }
}
}