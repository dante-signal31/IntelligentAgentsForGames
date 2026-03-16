using System.Collections.Generic;
using Pathfinding;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Editor.Inspectors
{
[CustomEditor(typeof(MapGraphRegions))]
public class MapGraphRegionsEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        // Add some space before the button
        EditorGUILayout.Space(10);

        // Get the component reference
        MapGraphRegions mapGraphRegions = (MapGraphRegions)target;

        // Create and handle the reset button
        if (GUILayout.Button("Bake Regions", GUILayout.Height(30)))
        {
            mapGraphRegions.GenerateRegions();
            EditorUtility.SetDirty(target);
        }
    }
    
    private void OnSceneGUI()
    {
        var mapGraphRegions = (MapGraphRegions)target;
    
        if (mapGraphRegions == null) return;
        
        if (!mapGraphRegions.showGizmos) return;
        
        Vector2 cellSize = mapGraphRegions.mapGraph.CellSize;

        foreach (KeyValuePair<uint, uint> nodeIdToRegionId in 
                 mapGraphRegions.GraphRegionsResource.nodesIdToRegionsId)
        {
            IPositionNode node = 
                mapGraphRegions.mapGraph.GetNodeById(nodeIdToRegionId.Key);
            Vector2 position = node.Position;
            uint regionId = nodeIdToRegionId.Value;
            if (mapGraphRegions.regionColors.TryGetValue(regionId, out var regionColor))
            {
                regionColor.a = mapGraphRegions.gizmoAlpha;
                Vector2 halfSize = cellSize / 2;
                Rect rect = new Rect(position - halfSize, cellSize);
                Handles.DrawSolidRectangleWithOutline(
                    rect, regionColor, 
                    mapGraphRegions.mapGraph.GridColor);
            }
            else
            {   // In some editor context changes, region colors are missed and I need to
                // update them manually.
                mapGraphRegions.UpdateRegionsColors();
            }
        }

        foreach (RegionSeed seed in mapGraphRegions.seeds)
        {
            Handles.color = mapGraphRegions.gizmosColor;
            Handles.DrawSolidDisc(
                seed.position, 
                Vector3.forward, 
                mapGraphRegions.seedRadius);
        }
    }
}
}