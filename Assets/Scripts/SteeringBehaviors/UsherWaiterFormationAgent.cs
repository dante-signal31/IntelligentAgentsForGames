
using Groups;
using PropertyAttribute;
using UnityEngine;

namespace SteeringBehaviors
{
/// <summary>
/// <p>This agent is the invisible leader of a formation. It decides where to move the
/// formation and its members follow him.</p>
/// <p>This agent is different from the UsherFormationAgent in that it slows down when
/// the members are lagging behind the ushers.</p>
/// </summary>
public class UsherWaiterFormationAgent : UsherFormationAgent
{
    [Header("USHER WAITER CONFIGURATION:")]
    [Tooltip("Maximum distance in pixels that the members average position can lag " +
             "behind ushers formation.")]
    [SerializeField] private float maximumLaggingBehindDistance = 5.0f;
    
    [Header("USHER WAITER WIRING:")]
    [Tooltip("Formation that generates and holds members.")]
    [InterfaceCompliant(typeof(IFormation))]
    [SerializeField] private MonoBehaviour formation;
    [Tooltip("Steering behavior to move the formation. It should be iTargeter compliant.")]
    [InterfaceCompliant(typeof(ITargeter))]
    [SerializeField] private MonoBehaviour targeter;
    
    private IFormation _formation;
    private ITargeter _targeter;
    private float _originalMaximumSpeed;
    // Our formation origin is not centered at the average position, so we
    // must compensate for that difference.
    private float _originalAveragePositionDistance;
    
    private Vector2 FormationAveragePosition
    {
        get
        {
            Vector2 averagePosition = Vector2.zero;
            if (_formation == null || _formation.Members.Count == 0) 
                return averagePosition;
            foreach (GameObject member in _formation.Members)
            {
                averagePosition += (Vector2) member.transform.position;
            }
            return averagePosition / _formation.Members.Count;
        }
    }
    
    /// <summary>
    /// Distance between the members' average positions and formation usher.
    /// </summary>
    private float LaggingBehindDistance => Vector2.Distance(
        transform.position,
        FormationAveragePosition) - _originalAveragePositionDistance;
    
    /// <summary>
    /// Whether usher is going away from the formation members. 
    /// </summary>
    private bool GoingAwayFromAveragePosition =>
        Vector2.Dot(
            transform.InverseTransformPoint(_targeter.Target.transform.position),
            transform.InverseTransformPoint(FormationAveragePosition)) < 0;

    protected override void Start()
    {
        base.Start();
        _formation = (IFormation) formation;
        _targeter = (ITargeter) targeter;
        _originalMaximumSpeed = MaximumSpeed;
        _originalAveragePositionDistance = Vector2.Distance(
            transform.position,
            FormationAveragePosition);
    }

    protected override void FixedUpdate()
    {
        if (formation == null) return;
        
        if (GoingAwayFromAveragePosition)
        {
            // If we are leaving behind the average position, then that means some members
            // are lagging behind. We want to slow down so that members have time to catch
            // the formation.
            MaximumSpeed = _originalMaximumSpeed * 
                           (1 - Mathf.Min(
                               LaggingBehindDistance, 
                               maximumLaggingBehindDistance) / 
                               maximumLaggingBehindDistance);
        }
        else
        {
            // We are going towards the average position, so we can go at full speed
            // because we are meeting with those members that are lagging behind.
            MaximumSpeed = _originalMaximumSpeed;       
        }
        
        base.FixedUpdate();
    }
}
}

