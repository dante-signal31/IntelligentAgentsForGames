using System;
using PropertyAttribute;
using UnityEditor;
using UnityEngine.UIElements;

namespace Editor.PropertyDrawers
{
/// <summary>
/// Decorate an inspector field with a help bar with a given message as text.
/// The help bar is preceded by an icon depending on the message type.
/// </summary>
[CustomPropertyDrawer(typeof(HelpBarAttribute))]
public class HelpBarAttributeDrawer : DecoratorDrawer
{
    public override VisualElement CreatePropertyGUI()
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
        
        // Return container.
        return container;
    }
}
}
