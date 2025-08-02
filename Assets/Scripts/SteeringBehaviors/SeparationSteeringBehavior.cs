using System;
using System.Collections.Generic;
using UnityEngine;

namespace SteeringBehaviors
{
/// <summary>
/// <p>Monobehaviour to offer a separation steering behaviour.</p>
/// <p>Separation steering behaviour makes the agent go away from other GameObjects
/// marked as threat. It's similar to flee, but the threat is not a single GameObject,
/// but a group and the repulsion force is inversely proportional to the distance.</p>
/// </summary>
public class SeparationSteeringBehavior: SteeringBehavior
{
    public enum SeparationAlgorithms
    {
        Linear,
        InverseSquare
    }
    
    [Header("CONFIGURATION")]
    [Tooltip("List of agents to separate from.")]
    [SerializeField] private List<AgentMover> threats = new();
    [Tooltip("Below this threshold distance, separation will be applied.")]
    [SerializeField] private float separationThreshold = 100f;
    [Tooltip("Chosen algorithm to calculate separation acceleration.")]
    [SerializeField] private SeparationAlgorithms separationAlgorithm = 
        SeparationAlgorithms.Linear;
    [Tooltip("Coefficient for inverse square law separation algorithm.")]
    [SerializeField] private float decayCoefficient = 1f;
    
    [Header("DEBUG")]
    [Tooltip("Make visible velocity marker.")]
    [SerializeField] private bool velocityMarkerVisible;
    [Tooltip("Color used for debugging gizmos.")]
    [SerializeField] private Color markerColor;

    /// <summary>
    /// List of agents to separate from.
    /// </summary>
    public List<AgentMover> Threats { get => threats; set => threats = value; }

    /// <summary>
    /// Below this threshold distance, separation will be applied.
    /// </summary>
    public float SeparationThreshold
    {
        get => separationThreshold; 
        set => separationThreshold = value;
    }

    /// <summary>
    /// Chosen algorithm to calculate separation acceleration.
    /// </summary>
    public SeparationAlgorithms SeparationAlgorithm
    {
        get => separationAlgorithm; 
        set => separationAlgorithm = value;
    }

    /// <summary>
    /// Coefficient for inverse square law separation algorithm.
    /// </summary>
    public float DecayCoefficient
    {
        get => decayCoefficient; 
        set => decayCoefficient = value;
    }

    /// <summary>
    /// <p>Color used for debugging gizmos.</p>
    /// <p>Property read by editor handle script.</p>
    /// </summary>
    public Color MarkerColor => markerColor;

    private Vector2 _currentVelocity;
    private float GetLinearSeparationStrength(
        float maximumAcceleration,
        float currentDistance, 
        float threshold)
    {
        return maximumAcceleration * (threshold - currentDistance) / threshold;    
    }

    private float GetInverseSquareLawSeparationStrength(
        float maximumAcceleration,
        float currentDistance,
        float k)
    {
        float normalizedDistance = currentDistance / SeparationThreshold;
        return Mathf.Min(k / Mathf.Pow(normalizedDistance, 2f), maximumAcceleration);    
    }

    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        if (Threats == null || Threats.Count == 0) return SteeringOutput.Zero;

        Vector2 newVelocity = args.CurrentVelocity;
        Vector2 currentPosition = args.Position;

        // Traverse every target and sum up their respective repulsion forces.
        foreach (AgentMover target in Threats)
        {
            Vector2 toTarget = (Vector2) target.transform.position - currentPosition;
            float distanceToTarget = toTarget.magnitude;

            // If the agent is close enough to the target, apply a repulsion force.
            if (distanceToTarget < SeparationThreshold)
            {
                float strengthAcceleration = SeparationAlgorithm switch
                {
                    SeparationAlgorithms.Linear =>
                        GetLinearSeparationStrength(
                            args.MaximumAcceleration,
                            distanceToTarget,
                            SeparationThreshold),
                    SeparationAlgorithms.InverseSquare =>
                        GetInverseSquareLawSeparationStrength(
                            args.MaximumAcceleration,
                            distanceToTarget,
                            DecayCoefficient),
                    _ => throw new ArgumentOutOfRangeException(),
                };
                Vector2 toTargetDirection = toTarget.normalized;
                Vector2 repulsionDirection = -toTargetDirection;
                // Add current repulsion to previously added from other targets.
                newVelocity += repulsionDirection * 
                               (strengthAcceleration * args.DeltaTime);
            }
        }

        _currentVelocity = newVelocity;
        return new SteeringOutput(newVelocity, 0);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!velocityMarkerVisible) return;
    
        Gizmos.color = MarkerColor;
        Gizmos.DrawLine(
            transform.position, 
            (Vector2) transform.position + _currentVelocity);
    }
#endif
}
}
