using SteeringBehaviors;
using Tools;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Editor.Inspectors
{
[CustomEditor(typeof(AgentMover))]
public class AgentMoverEditor : UnityEditor.Editor
{
    private SerializedProperty _maximumSpeed;
    private SerializedProperty _stopSpeed;
    private SerializedProperty _maximumRotationalSpeed;
    private SerializedProperty _stopRotationalThreshold;
    private SerializedProperty _maximumAcceleration;
    private SerializedProperty _maximumDeceleration;
    private SerializedProperty _autoSmooth;
    private SerializedProperty _smoothingMethod;
    private SerializedProperty _autoSmoothSamples;
    private SerializedProperty _smoothingCurve;
    private SerializedProperty _exponentialConvergenceRate;

    private void OnEnable()
    {
        _maximumSpeed = serializedObject.FindProperty("maximumSpeed");
        _stopSpeed = serializedObject.FindProperty("stopSpeed");
        _maximumRotationalSpeed = serializedObject.FindProperty("maximumRotationalSpeed");
        _stopRotationalThreshold = 
            serializedObject.FindProperty("stopRotationalThreshold");
        _maximumAcceleration = serializedObject.FindProperty("maximumAcceleration");
        _maximumDeceleration = serializedObject.FindProperty("maximumDeceleration");
        _autoSmooth = serializedObject.FindProperty("autoSmooth");
        _smoothingMethod = serializedObject.FindProperty("smoothingMethod");
        _autoSmoothSamples = serializedObject.FindProperty("autoSmoothSamples");
        _smoothingCurve = serializedObject.FindProperty("smoothingCurve");
        _exponentialConvergenceRate = 
            serializedObject.FindProperty("exponentialConvergenceRate");
    }

    public override VisualElement CreateInspectorGUI()
    {
        VisualElement container = new();

        // Create fields for serialized properties.
        PropertyField maximumSpeedField = new PropertyField(_maximumSpeed);
        PropertyField stopSpeedField = new PropertyField(_stopSpeed);
        PropertyField maximumRotationalSpeedField = 
            new PropertyField(_maximumRotationalSpeed);
        PropertyField stopRotationalThresholdField = 
            new PropertyField(_stopRotationalThreshold);
        PropertyField maximumAccelerationField = new PropertyField(_maximumAcceleration);
        PropertyField maximumDecelerationField = new PropertyField(_maximumDeceleration);
        
        // Add fields to main container.
        container.Add(maximumSpeedField);
        container.Add(stopSpeedField);
        container.Add(maximumRotationalSpeedField);
        container.Add(stopRotationalThresholdField);
        container.Add(maximumAccelerationField);
        container.Add(maximumDecelerationField);
        
        // Auto smooth toggle makes auto smoothing panel appear.  
        Toggle autoSmoothField = new(_autoSmooth.displayName);
        autoSmoothField.BindProperty(_autoSmooth);
        
        // Add auto smooth toggle to main panel.
        container.Add(autoSmoothField);

        // Auto smooth panel. It is going to contain the smoothing method dropdown
        // and the auto-smooth options panel for the selected smoothing method.
        VisualElement autoSmoothContainer = new VisualElement();
        autoSmoothContainer.style.flexDirection = FlexDirection.Column;
        container.Add(autoSmoothContainer);

        // Smoothing method dropdown. Its change makes the autoSmoothWeightedOptions
        // panel, or autoSmoothExponentialOptions panel, appear.
        var smoothingMethodField = new EnumField(
            _smoothingMethod.displayName,
            (SmoothingMethods) _smoothingMethod.enumValueIndex);
        smoothingMethodField.BindProperty(_smoothingMethod);

        // The panel that contains the auto smooth options for the selected method.
        VisualElement autoSmoothOptionsContainer = new VisualElement();
        autoSmoothOptionsContainer.style.flexDirection = FlexDirection.Column;

        // The subpanel that contains the auto smooth options for the Weighted Moving
        // Average method.
        VisualElement autoSmoothWeightedOptions = new VisualElement();
        PropertyField autoSmoothSamplesField = new PropertyField(_autoSmoothSamples);
        PropertyField smoothingCurveField = new PropertyField(_smoothingCurve);
        autoSmoothWeightedOptions.Add(autoSmoothSamplesField);
        autoSmoothWeightedOptions.Add(smoothingCurveField);

        // The subpanel that contains the auto smooth options for the Exponential method.
        VisualElement autoSmoothExponentialOptions = new VisualElement();
        PropertyField exponentialConvergenceRateField =
            new PropertyField(_exponentialConvergenceRate);
        autoSmoothExponentialOptions.Add(exponentialConvergenceRateField);
        
        // Populate auto smooth panel.
        autoSmoothContainer.Add(smoothingMethodField);
        autoSmoothContainer.Add(autoSmoothOptionsContainer);
        autoSmoothOptionsContainer.Add(autoSmoothWeightedOptions);
        autoSmoothOptionsContainer.Add(autoSmoothExponentialOptions);

        // The inner method that refreshes the auto smooth options panel.
        void RefreshAutoSmoothOptions(SmoothingMethods smoothingMethod)
        {
            if (!_autoSmooth.boolValue)
            {
                // When display is None, the panel is not visible and no empty space is
                // allocated for it.
                autoSmoothContainer.style.display = DisplayStyle.None;
                autoSmoothWeightedOptions.style.display = DisplayStyle.None;
                autoSmoothExponentialOptions.style.display = DisplayStyle.None;
                return;
            }

            autoSmoothContainer.style.display = DisplayStyle.Flex;
            autoSmoothOptionsContainer.style.display = DisplayStyle.Flex;

            switch (smoothingMethod)
            {
                case SmoothingMethods.WeightedMovingAverage:
                    autoSmoothWeightedOptions.style.display = DisplayStyle.Flex;
                    autoSmoothExponentialOptions.style.display = DisplayStyle.None;
                    break;
                case SmoothingMethods.Exponential:
                    autoSmoothWeightedOptions.style.display = DisplayStyle.None;
                    autoSmoothExponentialOptions.style.display = DisplayStyle.Flex;
                    break;
                default:
                    autoSmoothWeightedOptions.style.display = DisplayStyle.None;
                    autoSmoothExponentialOptions.style.display = DisplayStyle.None;
                    break;
            }
        }
        
        // Initialize auto smooth options panel.
        RefreshAutoSmoothOptions((SmoothingMethods) _smoothingMethod.enumValueIndex);
        
        // Register callbacks for auto smooth toggle and smoothing method dropdown.
        autoSmoothField.RegisterValueChangedCallback(evt =>
        {
            RefreshAutoSmoothOptions((SmoothingMethods) _smoothingMethod.enumValueIndex);
        });

        smoothingMethodField.RegisterValueChangedCallback(evt =>
        {
            RefreshAutoSmoothOptions((SmoothingMethods) evt.newValue);
        });
        
        return container;
    }
}
}
