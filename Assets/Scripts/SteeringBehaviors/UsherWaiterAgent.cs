using PropertyAttribute;
using UnityEngine;

namespace SteeringBehaviors
{
public class UsherWaiterAgent: AgentMover
{
    [Header("USHER WAITER CONFIGURATION:")]
    [Tooltip(" Maximum distance that the following agent can lag behind usher.")]
    [SerializeField] public float maximumLaggingBehindDistance  = 0.5f;

    [Header("WIRING:")]
    [Tooltip("Steering behavior to move the formation. It must be ITargeter compliant")]
    [InterfaceCompliant(typeof(ITargeter))]
    [SerializeField] private SteeringBehavior targeter;
    
    // Following agent.
    public AgentMover FollowingAgent {get; set;}
    
    // Steering behavior to move the formation.
    private ITargeter _targeter;
    
    private float _originalMaximumSpeed;
    
    /// <summary>
    /// Distance between the members' average positions and formation usher.
    /// </summary>
    private float LaggingBehindDistance => Vector2.Distance(
        transform.position,
        FollowingAgent.transform.position);
    
    /// <summary>
    /// Whether usher is going away from the following agent. 
    /// </summary>
    private bool GoingAwayFromAveragePosition =>
        Vector2.Dot(
            transform.InverseTransformPoint(_targeter.Target.transform.position),
            transform.InverseTransformPoint(FollowingAgent.transform.position)) 
        < 0;

    protected override void Start()
    {
        base.Start();
        _targeter = (ITargeter) targeter;
        _originalMaximumSpeed = MaximumSpeed;
    }

    protected override void FixedUpdate()
    {
        if (GoingAwayFromAveragePosition)
        {
            // If we are leaving behind the following agent. We want to slow down so that
            // the following agent has time to catch the usher.
            MaximumSpeed = _originalMaximumSpeed * 
                           (1 - Mathf.Min(
                               LaggingBehindDistance, 
                               maximumLaggingBehindDistance) / 
                               maximumLaggingBehindDistance);
        }
        else
        {
            // We are going towards the following agent, so we can go at full speed
            // because we are meeting with it.
            MaximumSpeed = _originalMaximumSpeed;       
        }
        
        base.FixedUpdate();
    }
}
}