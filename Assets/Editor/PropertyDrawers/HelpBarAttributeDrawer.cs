using System;
using PropertyAttribute;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.PropertyDrawers
{
/// <summary>
/// Decorate an inspector field with a help bar with a given message as text.
/// The help bar is preceded by an icon depending on the message type.
/// </summary>
[CustomPropertyDrawer(typeof(HelpBarAttribute))]
public class HelpBarAttributeDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {  
        // Create container element.
        var container = new VisualElement();
        
        // Get message text.
        string message = ((HelpBarAttribute)attribute).Message;
        MessageType messageType = ((HelpBarAttribute)attribute).MessageType switch
        {
            MessageTypes.MessageType.Info => MessageType.Info,
            MessageTypes.MessageType.Warning => MessageType.Warning,
            MessageTypes.MessageType.Error => MessageType.Error,
            _ => throw new ArgumentOutOfRangeException()
        };
        
        // Add help bubble with given message to container.
        HelpBox helpBox = new HelpBox(message, (HelpBoxMessageType)messageType);
        container.Add(helpBox);
        
        // Now add decorated property to container.
        PropertyField decoratedProperty = new PropertyField(property);
        container.Add(decoratedProperty);
        
        // Return container.
        return container;
    }
    
    // IMGUI fallback for older Inspectors or when UI Toolkit is not used
    public override void OnGUI(
        Rect position, 
        SerializedProperty property, 
        GUIContent label)
    {
        string message = ((HelpBarAttribute)attribute).Message;
        MessageType messageType = ((HelpBarAttribute)attribute).MessageType switch
        {
            MessageTypes.MessageType.Info => MessageType.Info,
            MessageTypes.MessageType.Warning => MessageType.Warning,
            MessageTypes.MessageType.Error => MessageType.Error,
            _ => MessageType.None
        };

        // Calculate rects
        const float helpBoxPadding = 2f;
        float helpHeight = EditorStyles.helpBox.CalcHeight(
            new GUIContent(message), 
            position.width);
        Rect helpRect = new Rect(position.x, position.y, position.width, helpHeight);
        Rect fieldRect = 
            new Rect(
                position.x, 
                position.y + helpHeight + helpBoxPadding, 
                position.width, 
                EditorGUIUtility.singleLineHeight);

        // Draw help box and property field
        EditorGUI.HelpBox(helpRect, message, messageType);
        EditorGUI.PropertyField(fieldRect, property, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        string message = ((HelpBarAttribute)attribute).Message;
        float helpHeight = EditorStyles.helpBox.CalcHeight(
            new GUIContent(message), 
            EditorGUIUtility.currentViewWidth);
        const float helpBoxPadding = 2f;
        // Include full height of the property (handles children if expanded)
        float fieldHeight = EditorGUI.GetPropertyHeight(property, label, true);
        return helpHeight + helpBoxPadding + fieldHeight;
    }
}
}