using System;
using UnityEngine;

/// <summary>
/// <p>Monobehaviour to offer an Interpose steering behaviour.</p>
/// <p>Interpose make an agent to place itself between two other agents.</p>
/// <p>It's an usual protection behavior. E.g. a bodyguard.</p>
/// </summary>
[RequireComponent(typeof(SeekSteeringBehavior))]
public class InterposeSteeringBehavior: SteeringBehavior
{
    [Header("CONFIGURATION:")] 
    [SerializeField] private AgentMover agentA;
    [SerializeField] private AgentMover agentB;
    [Tooltip("Distance at which we give our goal as reached and we stop our agent.")]
    [SerializeField] private float arrivalDistance;

    [Header("DEBUG")]
    [Tooltip("Make visible position marker.")]
    [SerializeField] private bool predictedPositionMarkerVisible;

    public AgentMover AgentA
    {
        get => agentA;
        set => agentA = value;
    }
    
    public AgentMover AgentB
    {
        get => agentB;
        set => agentB = value;
    }
    
    public float ArrivalDistance
    {
        get => arrivalDistance;
        set => arrivalDistance = value;
    }
    
    private SeekSteeringBehavior _seekSteeringBehavior;
    private GameObject _predictedPositionMarker;
    private Vector2 _previousPositionAgentA;
    private Vector2 _previousPositionAgentB;
    private Color _agentAColor;
    private Color _agentBColor;

    private void Awake()
    {
        // Create an invisible object as marker to place it at target predicted future
        // position. That marker will be used by seek steering behaviour as target.
        _predictedPositionMarker = new GameObject();
        _predictedPositionMarker.transform.position = GetMidPoint(
            agentA.transform.position, 
            agentB.transform.position);
        // Configure seek steering behaviour to go to that marker.
        _seekSteeringBehavior = GetComponent<SeekSteeringBehavior>();
        _seekSteeringBehavior.Target = _predictedPositionMarker;
        _seekSteeringBehavior.ArrivalDistance = arrivalDistance;
        // Configure our gizmos.
        _agentAColor = agentA.GetComponent<AgentColor>().Color;
        _agentBColor = agentB.GetComponent<AgentColor>().Color;
    }

    private void OnDestroy()
    {
        Destroy(_predictedPositionMarker);
    }

    /// <summary>
    /// Get midway point between two positions.
    /// </summary>
    /// <param name="position1">First position.</param>
    /// <param name="position2">Second position.</param>
    /// <returns>Midpoint position.</returns>
    public static Vector2 GetMidPoint(Vector2 position1, Vector2 position2)
    {
        Vector2 vectorBetweeenPositions = position2 - position1;
        return position1 + vectorBetweeenPositions / 2;
    }

    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        Vector2 currentPosition = args.CurrentAgent.transform.position;
        float maximumSpeed = args.MaximumSpeed;

        if ((Vector2)AgentA.transform.position != _previousPositionAgentA ||
            (Vector2)AgentB.transform.position != _previousPositionAgentB)
        {
            Vector2 midPoint = GetMidPoint(
                AgentA.transform.position, 
                AgentB.transform.position);
        
            // If target agents where static, how much time we'd need to get to midPoint?
            float TimeToReachMidPoint = (midPoint - currentPosition).magnitude / 
                                        maximumSpeed;
        
            // But actually agents won't be static, so while we move to midPoint,
            // they will move too. So, we must figure out where target agents are going
            // to be after TimeToReachMidPoint has passed. To get that we'll assume both
            // target agents are going to continue on a straight trajectory (so, no
            // velocity change), so we'll extrapolate their future position using
            // their current velocity.
            Vector2 futurePositionOfAgentA = (Vector2) AgentA.transform.position + 
                                             AgentA.Velocity * TimeToReachMidPoint;
            Vector2 futurePositionOfAgentB = (Vector2) AgentB.transform.position + 
                                             AgentB.Velocity * TimeToReachMidPoint;
            
            // Now we have the future position of target agents, we can get the estimated
            // future midpoint position.
            Vector2 futureMidPoint = GetMidPoint(
                futurePositionOfAgentA, 
                futurePositionOfAgentB);
        
            // So, to not been left behind, we must go to the future midpoint.
            _predictedPositionMarker.transform.position = futureMidPoint;

            _previousPositionAgentA = AgentA.transform.position;
            _previousPositionAgentB = AgentB.transform.position;
        }
        
        return _seekSteeringBehavior.GetSteering(args);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_predictedPositionMarker == null || 
            !predictedPositionMarkerVisible) return;

        Gizmos.color = _agentAColor;
        Gizmos.DrawLine(
            AgentA.transform.position, 
            _predictedPositionMarker.transform.position);
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(_predictedPositionMarker.transform.position, 0.1f);
        Gizmos.color = _agentBColor;
        Gizmos.DrawLine(
            AgentB.transform.position,
            _predictedPositionMarker.transform.position
        );
    }
#endif
}
