using Groups;
using Tools;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


[CustomEditor(typeof(MeasureTape))]
// Leave this script under the default namespace. I don't know why, but Gizmos are not
// drawn if this script is under Editor namespace.
public class DrawMeasureTape : UnityEditor.Editor
{
    private SerializedProperty _localPositionA;
    private SerializedProperty _localPositionB;
    private SerializedProperty _color;
    private SerializedProperty _thickness;
    private SerializedProperty _endWidth;
    private SerializedProperty _endAlignment;
    private SerializedProperty _textSize;   
    private SerializedProperty _textDistance;
    
    private void OnEnable()
    {
        _localPositionA = serializedObject.FindProperty("localPositionA");
        _localPositionB = serializedObject.FindProperty("localPositionB");
        _color = serializedObject.FindProperty("color");
        _thickness = serializedObject.FindProperty("thickness");
        _endWidth = serializedObject.FindProperty("endWidth");
        _endAlignment = serializedObject.FindProperty("endAlignment");
        _textSize = serializedObject.FindProperty("textSize");
        _textDistance = serializedObject.FindProperty("textDistance");
    }
    
    public override VisualElement CreateInspectorGUI()
    {
        
        VisualElement root = new()
        {
            style =
            {
                // Add elements in descending column order.
                flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Column)
            }
        };

        MeasureTape tape = (MeasureTape)serializedObject.targetObject;
        
        // Just add those properties we want to be shown in its default view.
        var localPositionAField = new PropertyField(_localPositionA);
        root.Add(localPositionAField);
        var localPositionBField = new PropertyField(_localPositionB);
        root.Add(localPositionBField);
        
        // Create the panel where we will show global position fields.
        VisualElement globalPositionFields = new VisualElement();
        root.Add(globalPositionFields);
        
        // Populate the panel with global positions fields.
        UpdateGlobalPositionFields(globalPositionFields, tape);
        
        // When changed local position fields, or their handles, update global position
        // fields.
        localPositionAField.RegisterValueChangeCallback(_ => 
            UpdateGlobalPositionFields(globalPositionFields, tape));
        localPositionBField.RegisterValueChangeCallback(_ => 
            UpdateGlobalPositionFields(globalPositionFields, tape));
        
        // Add every other property we want to show in its default view.
        root.Add(new PropertyField(_color));
        root.Add(new PropertyField(_thickness));
        root.Add(new PropertyField(_endWidth));
        root.Add(new PropertyField(_endAlignment));
        root.Add(new PropertyField(_textSize));
        root.Add(new PropertyField(_textDistance));
        
        return root;
    }

    private void UpdateGlobalPositionFields(VisualElement panel, MeasureTape tape)
    {
        panel.Clear();
        
        panel.style.flexDirection = 
            new StyleEnum<FlexDirection>(FlexDirection.Column);
        
        var positionAField = new Vector3Field("Global Position A");
        positionAField.value = tape.PositionA;
        positionAField.SetEnabled(false);
        panel.Add(positionAField);
        
        var positionBField = new Vector3Field("Global Position B");
        positionBField.value = tape.PositionB;
        positionBField.SetEnabled(false);
        panel.Add(positionBField);
    }

    // Use DrawGizmo to draw the tape even when the tape is not selected.
    [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
    static void DrawTapeGizmos(MeasureTape tape, GizmoType gizmoType)
    {
        // Draw a line between handles.
        Handles.color = tape.color;
        Handles.DrawLine(tape.PositionA, tape.PositionB, tape.thickness);
        
        // Draw lines to highlight handles.
        Handles.DrawWireDisc(
            tape.PositionA,  
            Vector3.forward, 
            0.1f, 
            tape.thickness);
        Handles.DrawWireDisc(
            tape.PositionB, 
            Vector3.forward, 
            0.1f, 
            tape.thickness);
        
        float distance = Vector3.Distance(tape.PositionA, tape.PositionB);
        Vector3 direction = (tape.PositionB - tape.PositionA).normalized;
        Vector3 normalVector = new Vector3(-direction.y, direction.x, direction.z);
        
        // Draw a label to show the distance between handles.
        Vector3 middlePosition = (tape.PositionA + tape.PositionB) / 2;
        GUIStyle style = new GUIStyle();
        style.normal.textColor = tape.color;
        style.fontSize = tape.textSize;
        Vector3 labelPosition = middlePosition + normalVector * tape.textDistance;
        Handles.Label(labelPosition, distance.ToString("F2"), style);
        
        // Draw tape ends.
        Vector3 semiEnd = normalVector * tape.endWidth / 2;
        Handles.DrawLine(
            tape.PositionA - (semiEnd) + (semiEnd) * tape.endAlignment, 
            tape.PositionA + (semiEnd) + (semiEnd) * tape.endAlignment);
        Handles.DrawLine(
            tape.PositionB - (semiEnd) + (semiEnd) * tape.endAlignment,
            tape.PositionB + (semiEnd) + (semiEnd) * tape.endAlignment);
    }
    
    private void OnSceneGUI()
    {
        var tape = (MeasureTape)target;
        
        EditorGUI.BeginChangeCheck();
        
        // Place handles to locate ends.
        Vector3 positionAHandle = Handles.PositionHandle(
            tape.PositionA, 
            Quaternion.identity);
        Vector3 positionBHandle = Handles.PositionHandle(
            tape.PositionB, 
            Quaternion.identity);
        
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(tape, $"Changed tape ends.");
            tape.PositionA = positionAHandle;
            tape.PositionB = positionBHandle;
        }
    }
}
