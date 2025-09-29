using System;
using System.Collections.Generic;
using System.Linq;
using PropertyAttribute;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
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
        // Get the array with the interface types to check.
        var interfaceCompliantAttribute = (InterfaceCompliantAttribute) attribute;
        Type[] interfaceTypes = interfaceCompliantAttribute.InterfaceTypes;
        
        // Create container element.
        var container = new VisualElement();

        AddErrorBoxIfNeeded(interfaceTypes, property, container);
        
        // Get decorated property.
        PropertyField decoratedProperty = new PropertyField(property);
        
        // Make container check property to refresh inspector if the value given to the
        // property changes.
        //
        // My first approach was to use decoratedProperty.RegisterValueChangCallback()
        // but it was not working. I was having some kind of recursive issue, likely 
        // related to this reported bug:
        // https://discussions.unity.com/t/the-registervaluechanged-problem/1556730
        //
        // That's why I've followed the workaround proposed in that thread, and I've used 
        // TrackPropertyValue() instead.
        container.TrackPropertyValue(property, _ =>
        {
            AddErrorBoxIfNeeded(interfaceTypes, property, container);
            container.Add(decoratedProperty);
        });
        
        // Now add decorated property to the container.
        container.Add(decoratedProperty);
        
        // Return container.
        return container;
    }

    /// <summary>
    /// Adds an error box to the container if the inspected object does not comply with
    /// any of the specified interface types.
    /// </summary>
    /// <param name="interfaceTypes">An array of interface types to check compliance
    /// against.</param>
    /// <param name="property">The serialized property associated with the inspected
    /// object to be validated for interface compliance.</param>
    /// <param name="container">The container UI element to which the error box will
    /// be added if non-compliance is detected.</param>
    private void AddErrorBoxIfNeeded(Type[] interfaceTypes, SerializedProperty property,
        VisualElement container)
    {
        // Get the object passed to the decorated field.
        Object checkedObject = property.objectReferenceValue;

        // Check if the provided object complies with any of the given interfaces. Any not
        // complying interface will be returned in the list.
        List<Type> notFoundTypes = GetNotComplyingInterfaces(
            interfaceTypes,
            checkedObject);

        // If any not complying interface was found, add an error box to the container.
        HelpBox errorBox = GenerateErrorBox(notFoundTypes);
        
        // Clear existing content
        container.Clear();

        // Add an error box if needed.
        if (errorBox != null) container.Add(errorBox);
    }

    private HelpBox GenerateErrorBox(List<Type> notFoundTypes)
    {
        if (notFoundTypes.Count == 0) return null;
        
        // Create a comma-separated string of not found interface names.
        string interfaceNames = string.Join(
            ", ", 
            notFoundTypes.Select(t => t.Name));

        // Get message text.
        string alertMessage = "Provided object does not comply with " +
                              $"required following interfaces: {interfaceNames}";
        
        // Build a help box and return it.
        return new HelpBox(alertMessage, HelpBoxMessageType.Error);
    }

    /// <summary>
    /// Identifies the interfaces that are not implemented by a given object.
    /// </summary>
    /// <param name="interfaceTypes">An array of interface types to check compliance
    /// against.</param>
    /// <param name="checkedObject">The object to be checked for interface
    /// compliance.</param>
    /// <returns>A list of interface types that are not implemented by the
    /// given object. If provided object complies with every interface, then returned
    /// list will have zero elements.</returns>
    private List<Type> GetNotComplyingInterfaces(Type[] interfaceTypes,
        Object checkedObject)
    {
        List<Type> notFoundTypes = new();

        foreach (Type interfaceType in interfaceTypes)
        {
            if (checkedObject != null && 
                !interfaceType.IsInstanceOfType(checkedObject))
            {
                notFoundTypes.Add(interfaceType);
            }
        }
        
        return notFoundTypes;
    }
    
    // IMGUI fallback for older inspectors or when UI Toolkit is not used.
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Retrieve attribute and interface list
        var interfaceCompliantAttribute = (InterfaceCompliantAttribute)attribute;
        Type[] interfaceTypes = interfaceCompliantAttribute.InterfaceTypes;

        // Reserve rects
        // First draw the property field
        Rect propertyRect = position;

        // Compute potential help box height if needed
        List<Type> notFoundTypes = GetNotComplyingInterfaces(interfaceTypes, property.objectReferenceValue);
        string interfaceNames = string.Join(", ", notFoundTypes.Select(t => t.Name));
        string alertMessage = notFoundTypes.Count == 0
            ? null
            : $"Provided object does not comply with required following interfaces: {interfaceNames}";

        // If there is a message, draw it above the property
        if (!string.IsNullOrEmpty(alertMessage))
        {
            // Calculate height for HelpBox
            float helpHeight = EditorStyles.helpBox.CalcHeight(new GUIContent(alertMessage), position.width);
            Rect helpRect = new Rect(position.x, position.y, position.width, helpHeight);
            EditorGUI.HelpBox(helpRect, alertMessage, MessageType.Error);

            // Move property rect below the help box with a small padding
            const float padding = 2f;
            propertyRect = new Rect(position.x, helpRect.yMax + padding, position.width,
                EditorGUI.GetPropertyHeight(property, label, true));
        }

        EditorGUI.PropertyField(propertyRect, property, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Base height for the property itself
        float height = EditorGUI.GetPropertyHeight(property, label, true);

        // Add help box height if there's an error
        var interfaceCompliantAttribute = (InterfaceCompliantAttribute)attribute;
        Type[] interfaceTypes = interfaceCompliantAttribute.InterfaceTypes;
        List<Type> notFoundTypes = GetNotComplyingInterfaces(interfaceTypes, property.objectReferenceValue);

        if (notFoundTypes.Count > 0)
        {
            string interfaceNames = string.Join(", ", notFoundTypes.Select(t => t.Name));
            string alertMessage =
                $"Provided object does not comply with required following interfaces: {interfaceNames}";
            float helpHeight = EditorStyles.helpBox.CalcHeight(new GUIContent(alertMessage), EditorGUIUtility.currentViewWidth);
            const float padding = 2f;
            height += helpHeight + padding;
        }

        return height;
    }
}
}