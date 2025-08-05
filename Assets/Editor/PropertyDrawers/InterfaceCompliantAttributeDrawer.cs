using System;
using System.Collections.Generic;
using System.Linq;
using PropertyAttribute;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Editor.PropertyDrawers
{
/// <summary>
/// <p>Decorate an inspector field to show an alert message bar if the decorated
/// property does not comply with any of the given interfaces.</p>
/// </summary>
[CustomPropertyDrawer(typeof(InterfaceCompliantAttribute))]
public class InterfaceCompliantAttributeDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        // Get the object passed to the decorated field.
        Object checkedObject = property.objectReferenceValue;
        
        // Get the array with the interface types to check.
        var interfaceCompliantAttribute = (InterfaceCompliantAttribute) attribute;
        Type[] interfaceTypes = interfaceCompliantAttribute.InterfaceTypes;

        // Check if the object complies with any of the given interfaces. Any not
        // complying interface will be added to the list.
        List<Type> notFoundTypes = new();
        foreach (Type interfaceType in interfaceTypes)
        {
            if (checkedObject != null && 
                !interfaceType.IsInstanceOfType(checkedObject))
            {
                notFoundTypes.Add(interfaceType);
            }
            
        }
        
        // Create container element.
        var container = new VisualElement();

        if (notFoundTypes.Count > 0)
        {
            // Create a comma-separated string of not found interface names.
            string interfaceNames = string.Join(
                ", ", 
                notFoundTypes.Select(t => t.Name));

            // Get message text.
            string alertMessage = "Provided object does not comply with " +
                                  $"required following interfaces: {interfaceNames}";
            
            // Add help bubble with given message to container.
            HelpBox helpBox = new HelpBox(alertMessage, HelpBoxMessageType.Error);
            container.Add(helpBox);
        }
        
        // Now add decorated property to the container.
        PropertyField decoratedProperty = new PropertyField(property);
        container.Add(decoratedProperty);
        
        // Return container.
        return container;
    }
}
}