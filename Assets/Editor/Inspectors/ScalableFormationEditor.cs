using System;
using Groups;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Editor.Inspectors
{
/// <summary>
/// Custom inspector editor to only show those properties you need to edit for every
/// distribution type.
/// </summary>
[CustomEditor(typeof(ScalableFormation))]
public class ScalableFormationEditor : UnityEditor.Editor
{
    private SerializedProperty _memberPrefab;
    private SerializedProperty _memberRadius;
    private SerializedProperty _minimumDistanceBetweenMembers;
    private SerializedProperty _distributionType;
    private SerializedProperty _formationDimensions;
    private SerializedProperty _density;
    private SerializedProperty _quantity;
    private SerializedProperty _showGizmos;
    private SerializedProperty _gizmosColor;
    private SerializedProperty _originGizmoRadius;
    private SerializedProperty _rightHinge;
    private SerializedProperty _leftHinge;

    private void OnEnable()
    {
        _memberPrefab = serializedObject.FindProperty("memberPrefab");
        _memberRadius = serializedObject.FindProperty("memberRadius");
        _minimumDistanceBetweenMembers = 
            serializedObject.FindProperty("minimumDistanceBetweenMembers");
        _distributionType = serializedObject.FindProperty("distributionType");
        _formationDimensions = serializedObject.FindProperty("formationDimensions");
        _density = serializedObject.FindProperty("density");
        _quantity = serializedObject.FindProperty("quantity");
        _showGizmos = serializedObject.FindProperty("showGizmos");
        _gizmosColor = serializedObject.FindProperty("gizmosColor");
        _originGizmoRadius = serializedObject.FindProperty("originGizmoRadius");
        _rightHinge = serializedObject.FindProperty("rightHinge");
        _leftHinge = serializedObject.FindProperty("leftHinge");
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

        // Just add those properties we want to be shown in its default view.
        root.Add(new PropertyField(_memberPrefab));
        root.Add(new PropertyField(_memberRadius));
        root.Add(new PropertyField(_minimumDistanceBetweenMembers));
        
        // We want to decide depending on the distribution type which fields we
        // want to show. So we create a specific control.
        var distributionTypeField = new EnumField(
            "Distribution Type", 
            // EnumField just creates a generic. It needs an initial value to know
            // which enum type to show.
            (ScalableFormation.DistributionType)_distributionType.enumValueIndex);
        distributionTypeField.BindProperty(_distributionType);
        root.Add(distributionTypeField);

        // Now the dynamic panel.
        VisualElement distributionFields = new VisualElement();
        root.Add(distributionFields);
        UpdateDistributionFields(
            distributionFields,
            (ScalableFormation.DistributionType)distributionTypeField.value);
        
        // Every time the distribution type changes, we need to update the fields.
        distributionTypeField.RegisterValueChangedCallback(evt => 
        {
            distributionFields.Clear();
            var newValue = (ScalableFormation.DistributionType) evt.newValue;
            UpdateDistributionFields(distributionFields, newValue);
        });
        
        
        // Add every other property we want to show in its default view.
        root.Add(new PropertyField(_showGizmos));
        root.Add(new PropertyField(_gizmosColor));
        root.Add(new PropertyField(_originGizmoRadius));
        root.Add(new PropertyField(_rightHinge));
        root.Add(new PropertyField(_leftHinge));
        
        return root;
    }

    private void UpdateDistributionFields(
        VisualElement panel,
        ScalableFormation.DistributionType type)
    {
        panel.style.flexDirection = 
            new StyleEnum<FlexDirection>(FlexDirection.Column);
        
        Vector2Field densityField = new Vector2Field("Density");
        densityField.BindProperty(_density);
        Vector2Field dimensionsField = new Vector2Field("Dimensions");
        dimensionsField.BindProperty(_formationDimensions);
        IntegerField quantityField = new IntegerField("Quantity");
        quantityField.BindProperty(_quantity);
        
        switch (type)
        {
            case ScalableFormation.DistributionType.DensityAndDimensionsDefined:
                
                panel.Add(densityField);
                panel.Add(dimensionsField);
                break;
            case ScalableFormation.DistributionType.DensityAndQuantityDefined:
                panel.Add(densityField);
                panel.Add(quantityField);
                break;
            case ScalableFormation.DistributionType.QuantityAndDimensionsDefined:
                panel.Add(quantityField);
                panel.Add(dimensionsField);
                break;
        }
    }
}
}