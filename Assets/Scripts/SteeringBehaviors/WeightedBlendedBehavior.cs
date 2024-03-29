using System;
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
    
    [Header("CONFIGURATION:")]
    [Tooltip("The set of behaviors to blend.")]
    [SerializeField] private WeightedBehavior[] weightedBehaviors;
    
    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        SteeringOutput result = new SteeringOutput();
        foreach (var weightedBehavior in weightedBehaviors)
        {
            result += weightedBehavior.steeringBehavior.GetSteering(args) * weightedBehavior.weight;
        }
        return result;
    }
}
