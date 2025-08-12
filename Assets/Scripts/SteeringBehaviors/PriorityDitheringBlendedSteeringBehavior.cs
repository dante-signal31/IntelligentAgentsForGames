using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SteeringBehaviors
{
/// <summary>
/// This steering behavior takes a set of other steering behaviors and blends their
/// outputs using probabilities.
/// </summary>
public class PriorityDitheringBlendedSteeringBehavior : SteeringBehavior, IGizmos
{
    /// <summary>
    /// Represents a random steering behavior resource that specifies a
    /// particular steering behavior, its associated probability, and a color for
    /// debugging purposes. This class is used in configuring prioritized dithering
    /// blended steering behaviors in game AI.
    /// </summary>
    [Serializable]
    public class RandomBehavior
    {
        public SteeringBehavior steeringBehavior;
        public float probability;
        public Color debugColor;
    }

    [Header("CONFIGURATION:")]
    [Tooltip("The set of behaviors to blend.")]
    [SerializeField] private RandomBehavior[] randomBehaviors;

    [Header("DEBUG:")]
    [Tooltip("Show gizmos.")]
    [SerializeField] private bool showGizmos;
    [Tooltip("Colors for this object's gizmos.")]
    [SerializeField] private Color gizmosColor;

    /// <summary>
    /// Show gizmos.
    /// </summary>
    public bool ShowGizmos
    {
        get => showGizmos;
        set => showGizmos = value;
    }

    /// <summary>
    /// Colors for this object's gizmos.
    /// </summary>
    public Color GizmosColor
    {
        get => gizmosColor;
        set => gizmosColor = value;
    }

    private SteeringOutput _currentSteering;

    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        foreach (var randomBehavior in randomBehaviors)
        {
            if (Random.value > randomBehavior.probability) continue;
            SteeringOutput output = randomBehavior.steeringBehavior.GetSteering(args);
            if (output.Equals(SteeringOutput.Zero)) continue;
            _currentSteering = output;
            return output;
        }
        return SteeringOutput.Zero;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showGizmos || _currentSteering == null) return;

        Gizmos.color = gizmosColor;
    
        Gizmos.DrawLine(
            transform.position, 
            transform.position + (Vector3) _currentSteering.Linear);
    }
#endif
}
}

