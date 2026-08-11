using System.Collections.Generic;
using System.Linq;
using Pathfinding;
using Tools;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Editor.PropertyDrawers
{ 
[CustomPropertyDrawer(typeof(PositionNode))]
public class PositionNodeDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        SerializedProperty currentProperty = property;
        List<uint> connectionIds = 
            ((PositionNode) property.boxedValue).ConnectionIds.ToList();
        
        VisualElement container = new VisualElement();
        
        uint currentId = property.FindPropertyRelative("id").uintValue;
        
        // Show ID label.
        Label label = new("Id");
        label.text = currentId.ToString();
        container.Add(label);
        
        // Show position.
        SerializedProperty positionProperty = 
            property.FindPropertyRelative("position");
        PropertyField positionField = new(positionProperty);
        container.Add(positionField);
        
        // Show the connection list.
        //
        // Make the list foldable and give it a title.
        Foldout connectionsLabel = new()
        {
            text = "Connections:",
            value = true
        };
        // Now create the list.
        ListView connectionList = new()
        {
            itemsSource = connectionIds,
            // Show add and remove buttons under the list.
            showAddRemoveFooter = true,
            // Make the list resize automatically to the height of its content.
            virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
            makeItem = CreateConnectionElement,
            bindItem = 
                (element, index) => 
                    BindToConnection(element, index, currentProperty, connectionIds),
        };
        // Set the list's height to the number of connections.
        // UpdateListViewHeight(connectionList, connectionIds.Count);
        // Set up the list's callbacks. You must pass state variables as parameters. You
        // cannot rely on global variables because Unity reuses the same
        // PositionNodeDrawer instance for all properties of the PositionNode type.
        connectionList.itemsAdded += 
            addedIndices => 
                OnListItemsAdded(
                    addedIndices, 
                    currentProperty, 
                    connectionIds);
        connectionList.itemsRemoved += 
            removedIndices => OnListItemsRemoved(
                currentProperty, 
                removedIndices, 
                connectionIds);
        
        // Populate container hierarchy.
        connectionsLabel.Add(connectionList);
        container.Add(connectionsLabel);
        
        return container;
    }

    /// <summary>
    /// Creates a new visual element representing a connection within the connection list.
    /// </summary>
    /// <returns>
    /// A VisualElement displaying connection details, including the connection's ID,
    /// end node ID, and cost, with appropriate layout formatting.
    /// </returns>
    private VisualElement CreateConnectionElement()
    {
        VisualElement row = new VisualElement();
        row.style.flexDirection = FlexDirection.Column;

        Label connectionIdLabel = new Label();
        
        LongField endNodeField = new LongField("End Node ID: ");
        FloatField costField = new FloatField("Cost: ");
        
        // Indent both fields to the right a bit to highlight them.
        endNodeField.style.marginLeft = 16f;
        costField.style.marginLeft = 16f;
        
        row.Add(connectionIdLabel);
        row.Add(endNodeField);
        row.Add(costField);

        return row;
    }

    /// <summary>
    /// Binds the given VisualElement to a connection entry within a PositionNode,
    /// allowing the UI to display and interact with connection data.
    /// </summary>
    /// <param name="element">
    /// The VisualElement to which the connection data will be bound.
    /// </param>
    /// <param name="index">
    /// Index of the connection in the connection list.
    /// </param>
    /// <param name="currentProperty">
    /// SerializedProperty representing the PositionNode that contains the
    /// connection list.
    /// </param>
    /// <param name="connectionIds">
    /// List of connection IDs available in the PositionNode.
    /// </param>
    private void BindToConnection(
        VisualElement element, 
        int index, 
        SerializedProperty currentProperty,
        List<uint> connectionIds)
    {
        // Original object read access.
        PositionNode positionNode = (PositionNode) currentProperty.boxedValue;
        uint connectionId = connectionIds[index];
        GraphConnection connection = positionNode.Connections[connectionId];
        
        // Original object connections write access. Remember you have to access to its
        // serialized version to be able to modify it.
        SerializedProperty connectionsProperty =
            currentProperty.FindPropertyRelative("connections");
        SerializedProperty keyDataProperty =
            connectionsProperty.FindPropertyRelative("keyData");
        SerializedProperty valueDataProperty =
            connectionsProperty.FindPropertyRelative("valueData");
        
        // Set label to connection ID.
        Label connectionIdLabel = element.Q<Label>();
        connectionIdLabel.text = connectionId.ToString();
        
        // Set fields to connection values. This is an initial assignment, so
        // you don't want to assign the values directly to the fields because that
        // would trigger a change notification (and their corresponding callbacks).
        // Instead, use the SetValueWithoutNotify method to avoid that.
        LongField endNodeField = element.Q<LongField>();
        endNodeField.SetValueWithoutNotify(connection.endNodeId);
        FloatField costField = element.Q<FloatField>();
        costField.SetValueWithoutNotify(connection.cost);
        
        endNodeField.RegisterValueChangedCallback(evt =>
        {
            // Get the index of the connection to modify.
            int serializedIndex = FindConnectionSerializedIndex(
                keyDataProperty,
                connectionId
            );
            if (serializedIndex == -1) return;
            // Get the connection to change. 
            GraphConnection connectionToChange = 
                (GraphConnection) valueDataProperty
                    .GetArrayElementAtIndex(serializedIndex).boxedValue;
            // Modify connection.
            connectionToChange.endNodeId = (uint)evt.newValue;
            // Apply the changes to the original object.
            valueDataProperty
                .GetArrayElementAtIndex(serializedIndex).boxedValue = connectionToChange;
            currentProperty.serializedObject.ApplyModifiedProperties();
        });
        costField.RegisterValueChangedCallback(evt =>
        {
            // Get the index of the connection to modify.
            int serializedIndex = FindConnectionSerializedIndex(
                keyDataProperty,
                connectionId
            );
            if (serializedIndex == -1) return;
            // Get the connection to change. 
            GraphConnection connectionToChange = 
                (GraphConnection) valueDataProperty
                    .GetArrayElementAtIndex(serializedIndex).boxedValue;
            // Modify connection.
            connectionToChange.cost = evt.newValue;
            // Apply the changes to the original object.
            valueDataProperty
                .GetArrayElementAtIndex(serializedIndex).boxedValue = connectionToChange;
            currentProperty.serializedObject.ApplyModifiedProperties();
        });
    }

    /// <summary>
    /// Handles the addition of new items to the connection list in the property drawer.
    /// </summary>
    /// <param name="addedIndices">
    /// The indices of the newly added items in the list.
    /// </param>
    /// <param name="currentProperty">
    /// The serialized property representing the object associated with the list.
    /// </param>
    /// <param name="connectionsIds">
    /// The list of connection IDs corresponding to the existing connections.
    /// </param>
    private void OnListItemsAdded(
        IEnumerable<int> addedIndices, 
        SerializedProperty currentProperty,
        List<uint> connectionsIds)
    {
        // Add new connection entries to the list. Usually, if you use the inspector list
        // "+" button, the indices will have only one element. You'll only have more than
        // that if you add elements to the list using some sort of drag-and-drop.
        foreach (int index in addedIndices)
        {
            // Original object read access.
            PositionNode positionNode = (PositionNode) currentProperty.boxedValue;
            
            // New connection to add to the list.
            uint newConnectionId = positionNode.GetNextAvailableConnectionId();
            connectionsIds[index] = newConnectionId;
            GraphConnection newConnection = new GraphConnection(
                startNodeId: positionNode.Id,
                endNodeId: 0,
                cost: 1f
            );
            
            // Append the new connection to the original list.
            positionNode.Connections.Add(newConnectionId, newConnection);
            
            // Overwrite original object with the one with the updated list.
            currentProperty.boxedValue = positionNode;
            
            // Apply the changes to the original object.
            currentProperty.serializedObject.ApplyModifiedProperties();
        }
    }

    /// <summary>
    /// Handles the removal of items from the connection list and updates
    /// the associated serialized property and underlying data structure.
    /// </summary>
    /// <param name="currentProperty">
    /// The serialized property representing the PositionNode object being modified.
    /// </param>
    /// <param name="removedIndices">
    /// A collection of indices corresponding to the items removed from the connection
    /// list.
    /// </param>
    /// <param name="connectionsIds">
    /// The list of connection IDs currently associated with the PositionNode object.
    /// </param>
    private void OnListItemsRemoved(
        SerializedProperty currentProperty,
        IEnumerable<int> removedIndices,
        List<uint> connectionsIds)
    {
        // Make sure we are working with the most updated version of the original object.
        currentProperty.serializedObject.Update();
        
        // Original object connections write access. 
        SerializedProperty connectionsProperty =
            currentProperty.FindPropertyRelative("connections");
        
        // Get original connection list.
        CustomUnityDictionaries.UintGraphConnectionDictionary connections =
            (CustomUnityDictionaries.UintGraphConnectionDictionary) 
            connectionsProperty.boxedValue;
        
        // Remove connection.
        foreach (var removedIndex in removedIndices)
        {
            connections.Remove(connectionsIds[removedIndex]);
        }
        
        // Overwrite connection list.
        connectionsProperty.boxedValue = connections;
        
        // Apply changes to the original object.
        currentProperty.serializedObject.ApplyModifiedProperties();
    }


    /// <summary>
    /// Finds the serialized index of a connection ID within a serialized property array.
    /// </summary>
    /// <param name="keyDataProperty">
    /// The serialized property representing the array to search within.
    /// </param>
    /// <param name="connectionId">
    /// The connection ID to search for within the array.
    /// </param>
    /// <returns>
    /// The index of the connection ID in the array if found, otherwise -1.
    /// </returns>
    private static int FindConnectionSerializedIndex(
        SerializedProperty keyDataProperty,
        uint connectionId)
    {
        for (int i = 0; i < keyDataProperty.arraySize; i++)
        {
            SerializedProperty keyProperty =
                keyDataProperty.GetArrayElementAtIndex(i);

            if (keyProperty.uintValue == connectionId)
                return i;
        }

        return -1;
    }
}
}