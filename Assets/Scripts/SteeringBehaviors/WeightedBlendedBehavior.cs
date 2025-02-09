using System;
using System.Collections.Generic;
using SteeringBehaviors;
using UnityEngine;

/// <summary>
/// This steering behavior takes a set of other steering behaviors and blend their outputs using
/// weights.
/// </summary>
public class WeightedBlendedBehavior : SteeringBehavior
{
    [Serializable]
    public struct WeightedBehavior
    {
        public SteeringBehavior steeringBehavior;
        public float weight;
    }

    private struct WeightedOutput
    {
        public SteeringOutput steeringOutput;
        public float weight;
        
        public WeightedOutput(SteeringOutput steeringOutput, float weight)
        {
            this.steeringOutput = steeringOutput;
            this.weight = weight;
        }
    }
    
    [Header("CONFIGURATION:")]
    [Tooltip("The set of behaviors to blend.")]
    [SerializeField] private WeightedBehavior[] weightedBehaviors;
    
    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        // We need two passes: one to get only active outputs and one to blend them just
        // taking in count weights for the active outputs.
        List<WeightedOutput> activeOutputs = new List<WeightedOutput>();
        float totalWeight = 0;
        // First pass: get only active outputs.
        foreach (var weightedBehavior in weightedBehaviors)
        {
            SteeringOutput output = weightedBehavior.steeringBehavior.GetSteering(args);
            if (output.Equals(SteeringOutput.Zero)) continue;
            activeOutputs.Add(new WeightedOutput(output, weightedBehavior.weight));
            totalWeight += weightedBehavior.weight;
        }
        // Second pass: blend them.
        SteeringOutput result = new SteeringOutput();
        foreach (WeightedOutput weightedOutput in activeOutputs)
        {
            float outputRelativeWeight = weightedOutput.weight / totalWeight;
            result += weightedOutput.steeringOutput * outputRelativeWeight;
        }
        
        return result;
    }
}
