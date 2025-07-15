using System;
using System.Collections.Generic;
using UnityEngine;

namespace SteeringBehaviors
{
/// <summary>
/// This steering behavior takes a set of other steering behaviors and blend their outputs
/// using weights.
/// </summary>
public class WeightBlendedSteeringBehavior : SteeringBehavior, IGizmos
{
    [Serializable] 
    public struct WeightedBehavior
    {
        public SteeringBehavior steeringBehavior;
        public float weight;
        public Color debugColor;
    }

    private struct WeightedOutput
    {
        public readonly SteeringOutput SteeringOutput;
        public readonly float Weight;
        public readonly Color DebugColor;
    
        public WeightedOutput(SteeringOutput steeringOutput, float weight, Color color)
        {
            this.SteeringOutput = steeringOutput;
            this.Weight = weight;
            DebugColor = color;
        }
    }

    [Header("CONFIGURATION:")]
    [Tooltip("The set of behaviors to blend.")]
    [SerializeField] private WeightedBehavior[] weightedBehaviors;
    
    [Header("DEBUG:")]
    [Tooltip("Whether to show gizmos.")]
    [SerializeField] private bool showGizmos;
    [Tooltip("Colors for this object's gizmos.")]
    [SerializeField] private Color gizmosColor;
    
    public bool ShowGizmos
    {
        get => showGizmos;
        set => showGizmos = value;
    }

    public Color GizmosColor
    {
        get => gizmosColor;
        set => gizmosColor = value;
    }

    private readonly List<WeightedOutput> _activeOutputs = new();
    
    private float _totalWeight;
    
    private SteeringOutput _currentSteering;
    
    
    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        // We need two passes: one to get the active outputs
        // and one to blend them taking their relative weights for the active outputs.
        _activeOutputs.Clear();
        _totalWeight = 0.0f;
        
        // First pass: take in count only active outputs.
        foreach (var weightedBehavior in weightedBehaviors)
        {
            SteeringOutput output = weightedBehavior.steeringBehavior.GetSteering(args);
            if (output.Equals(SteeringOutput.Zero)) continue;
            _activeOutputs.Add(
                new WeightedOutput(
                    output, 
                    weightedBehavior.weight, 
                    weightedBehavior.debugColor));
            _totalWeight += weightedBehavior.weight;
        }
    
        // Second pass: blend them.
        _currentSteering = new SteeringOutput();
        foreach (WeightedOutput weightedOutput in _activeOutputs)
        {
            float outputRelativeWeight = weightedOutput.Weight / _totalWeight;
            _currentSteering += weightedOutput.SteeringOutput * outputRelativeWeight;
        }
        return _currentSteering;
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!ShowGizmos || _currentSteering == null) return;
        
        Gizmos.color = gizmosColor;
        
        // Draw first partial steerings.
        foreach (WeightedOutput weightedOutput in _activeOutputs)
        {
            float outputRelativeWeight = weightedOutput.Weight / _totalWeight;
            Gizmos.color = weightedOutput.DebugColor;
            Gizmos.DrawLine(
                transform.position,
                transform.position +
                (Vector3) weightedOutput.SteeringOutput.Linear * outputRelativeWeight);
        }
        
        // Next draw total steering.
        Gizmos.color = GizmosColor;
        Gizmos.DrawLine(
            transform.position,
            transform.position + (Vector3) _currentSteering.Linear); 
    }
#endif
}
}
