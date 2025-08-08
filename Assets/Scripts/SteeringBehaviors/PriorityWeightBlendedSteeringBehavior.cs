using System.Collections.Generic;
using UnityEngine;

namespace SteeringBehaviors
{
    /// <summary>
    /// <p> This steering behavior takes its children WeightBlendedSteeringBehavior nodes
    /// by order and gets the steering of the first node that returns non-zero
    /// steering.</p>
    /// </summary>
public class PriorityWeightBlendedSteeringBehavior: SteeringBehavior, IGizmos
{
    [Header("DEBUG:")]
    [Tooltip("Show gizmos.")]
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

    private readonly List<WeightBlendedSteeringBehavior> _weightBlendedSteeringBehaviors 
        = new();
    private SteeringOutput _currentSteering;

    private void Awake()
    {
        GetWeightBlendedSteeringBehaviors();
    }

    private void GetWeightBlendedSteeringBehaviors()
    {
        GetComponentsInChildren(_weightBlendedSteeringBehaviors);
    }

    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        _currentSteering = SteeringOutput.Zero;
        
        foreach (WeightBlendedSteeringBehavior weightBlendedSteeringBehavior in
                 _weightBlendedSteeringBehaviors)
        {
            SteeringOutput steeringOutput = 
                weightBlendedSteeringBehavior.GetSteering(args);
            if (steeringOutput.Equals(SteeringOutput.Zero)) continue;
            _currentSteering = steeringOutput;
            break;
        }
        
        return _currentSteering;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!ShowGizmos || _currentSteering == null) return;

        Gizmos.color = gizmosColor;
        Gizmos.DrawLine(
            transform.position, 
            transform.position + (Vector3) _currentSteering.Linear);
    }
#endif
    
}
}