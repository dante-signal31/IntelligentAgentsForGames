using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace SteeringBehaviors
{
/// <summary>
/// <p> Monobehaviour to offer a cohesion steering behaviour. </p>
/// <p> Cohesion steering behaviour makes the agent to place himself in the center of a
/// group of other agents. </p>
/// </summary>
public class CohesionSteeringBehavior: SteeringBehavior
{
    [Header("CONFIGURATION:")]
    [Tooltip("List of agents to align with averaging their positions.")]
    [SerializeField] private List<GameObject> targets = new List<GameObject>();
    [Tooltip("Distance at which we give our goal as reached and we stop our agent.")]
    [SerializeField] private float arrivalDistance = 30f;
    
    [FormerlySerializedAs("_seekSteeringBehavior")]
    [Header("WIRING:")]
    [Tooltip("Steering to actually move the agent.")]
    [SerializeField] private SeekSteeringBehavior seekSteeringBehavior;

    [Header("DEBUG")]
    [Tooltip("Make position gizmos visible.")]
    [SerializeField] private bool positionGizmoVisible;
    [Tooltip("Color for position gizmo.")]
    [SerializeField] private Color positionGizmosColor;
    [Tooltip("Radius for the position marker gizmo.")]
    [SerializeField] private float positionGizmoRadius;

    /// <summary>
    /// List of agents to align with averaging their orientations.
    /// </summary>
    public List<GameObject> Targets { get => targets; set => targets = value; }

    /// <summary>
    /// Distance at which we give our goal as reached and we stop our agent.
    /// </summary>
    public float ArrivalDistance
    {
        get => arrivalDistance;
        set
        {
            arrivalDistance = value;
            if (seekSteeringBehavior != null)
                seekSteeringBehavior.ArrivalDistance = value;
        }
    }

    /// <summary>
    /// <p>Average orientation counting every agent's targets.</p>
    /// </summary>
    public Vector2 AveragePosition{ get; private set; }

    private GameObject _positionMarker;

    private void Awake()
    {
        _positionMarker = new GameObject("OrientationMarker");
        seekSteeringBehavior.Target = _positionMarker;
        seekSteeringBehavior.ArrivalDistance = ArrivalDistance;
    }

    private void OnDestroy()
    {
        Destroy(_positionMarker);
    }

    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        if (Targets == null || 
            Targets.Count == 0 || 
            seekSteeringBehavior == null) 
            return SteeringOutput.Zero;

        // Let's average position counting every agent's targets. 
        Vector2 positionSum = new();
        foreach (GameObject target in Targets)
        {
            positionSum += (Vector2) target.transform.position;
        }
        AveragePosition = (positionSum / Targets.Count);
    
        // Place our position marker in the average position to make
        // _seekSteeringBehavior makes us get there.
        _positionMarker.transform.position = AveragePosition;
    
        return seekSteeringBehavior.GetSteering(args);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!positionGizmoVisible || 
            _positionMarker == null || 
            Targets == null) return;

        // Draw relationship between targets and center of mass.
        foreach (GameObject target in Targets)
        {
            Gizmos.color = target.GetComponent<AgentColor>().Color;
            Gizmos.DrawLine(
                target.transform.position, 
                _positionMarker.transform.position);
        }
    
        // Draw center of mass.
        Gizmos.color = positionGizmosColor;
        Gizmos.DrawWireSphere(_positionMarker.transform.position, positionGizmoRadius);
    
        // Draw heading from current agent to center of mass.
        Gizmos.color = positionGizmosColor;
        Gizmos.DrawLine(
            transform.position, 
            _positionMarker.transform.position);
    }
#endif
}
}
