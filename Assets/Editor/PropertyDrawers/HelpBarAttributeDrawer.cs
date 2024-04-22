using System;
using PropertyAttribute;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Editor.PropertyDrawers
{
    /// <summary>
    /// Decorate an inspector field with a help bar with given message as text.
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
    }
}