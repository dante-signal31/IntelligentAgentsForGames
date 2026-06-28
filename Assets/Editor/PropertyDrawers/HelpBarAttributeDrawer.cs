using System;
using PropertyAttribute;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.PropertyDrawers
{
/// <summary>
/// Decorate an inspector field with a help bar with a given message as text.
/// The help bar is preceded by an icon depending on the message type.
/// </summary>
[CustomPropertyDrawer(typeof(HelpBar))]
public class HelpBarAttributeDrawer : DecoratorDrawer
{
    public override VisualElement CreatePropertyGUI()
    {  
        // Create container element.
        var container = new VisualElement();
        
        // Get message text.
        string message = ((HelpBar)attribute).message;
        MessageType messageType = ((HelpBar)attribute).messageType switch
        {
            MessageTypes.MessageType.Info => MessageType.Info,
            MessageTypes.MessageType.Warning => MessageType.Warning,
            MessageTypes.MessageType.Error => MessageType.Error,
            _ => throw new ArgumentOutOfRangeException()
        };
        
        // Add help bubble with given message to container.
        HelpBox helpBox = new HelpBox(message, (HelpBoxMessageType)messageType);
        container.Add(helpBox);
        
        // Return container.
        return container;
    }
    
    // IMGUI fallback for older Inspectors or when UI Toolkit is not used
    public override void OnGUI(
        Rect position)
    {
        string message = ((HelpBar)attribute).message;
        MessageType messageType = ((HelpBar)attribute).messageType switch
        {
            MessageTypes.MessageType.Info => MessageType.Info,
            MessageTypes.MessageType.Warning => MessageType.Warning,
            MessageTypes.MessageType.Error => MessageType.Error,
            _ => MessageType.None
        };

        // Calculate rects
        float helpHeight = EditorStyles.helpBox.CalcHeight(
            new GUIContent(message), 
            position.width);
        Rect helpRect = new Rect(position.x, position.y, position.width, helpHeight);

        // Draw help box and property field
        EditorGUI.HelpBox(helpRect, message, messageType);
    }
    
    public override float GetHeight()
    {
        string message = ((HelpBar)attribute).message;
        float helpHeight = EditorStyles.helpBox.CalcHeight(
            new GUIContent(message), 
            EditorGUIUtility.currentViewWidth);
        const float helpBoxPadding = 2f;
        return helpHeight + helpBoxPadding;
    }
}
}
