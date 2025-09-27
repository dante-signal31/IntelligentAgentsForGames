using UnityEditor;
using UnityEngine;

namespace Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(Groups.OffsetList))]
    public class OffsetListDrawer : PropertyDrawer
    {
        public override void OnGUI(
            Rect position, 
            SerializedProperty property, 
            GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            // Draw the object field
            position.height = EditorGUIUtility.singleLineHeight;
            property.objectReferenceValue = EditorGUI.ObjectField(
                position, 
                label, 
                property.objectReferenceValue, 
                typeof(Groups.OffsetList), false);

            // If we have a valid reference, draw the array
            if (property.objectReferenceValue != null)
            {
                SerializedObject offsetList = 
                    new SerializedObject(property.objectReferenceValue);
                SerializedProperty offsetsArray = offsetList.FindProperty("offsets");

                position.y += 
                    EditorGUIUtility.singleLineHeight + 
                    EditorGUIUtility.standardVerticalSpacing;
                position.height = EditorGUI.GetPropertyHeight(offsetsArray);
                
                EditorGUI.indentLevel++;
                EditorGUI.PropertyField(
                    position, 
                    offsetsArray, 
                    GUIContent.none,
                    true);
                EditorGUI.indentLevel--;
                
                if (offsetList.hasModifiedProperties)
                {
                    offsetList.ApplyModifiedProperties();
                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(
            SerializedProperty property, 
            GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;
            
            if (property.objectReferenceValue != null)
            {
                SerializedObject offsetList = 
                    new SerializedObject(property.objectReferenceValue);
                SerializedProperty offsetsArray = offsetList.FindProperty("offsets");
                height += EditorGUIUtility.standardVerticalSpacing + 
                         EditorGUI.GetPropertyHeight(offsetsArray);
            }
        
            return height;
        }
    }
}