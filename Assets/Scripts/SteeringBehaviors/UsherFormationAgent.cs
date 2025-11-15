using UnityEngine;

namespace SteeringBehaviors
{
/// <summary>
/// This agent is the invisible leader of a formation. It decides where to move the
/// formation and its members follow him.
/// </summary>
public class UsherFormationAgent: AgentMover, IGizmos
{
    /// <summary>
    /// Type of movements the formation can perform.
    /// </summary>
    private enum MovementNeeded
    {
        Stop,
        TurnLeft,
        GoStraight,
        TurnRight,
    }
    
    [Header("USHER FORMATION CONFIGURATION:")]
    // Vanilla formation movement algorithm turns it taking its origin as axis. The
    // problem is that real formations don't turn that way because one wing would advance
    // but the other would go backwards, which is awkward or even impossible for a real
    // formation of soldiers. Instead of that, a realistic turn would use formation
    // corners as its turning axis (hinge).
    [Tooltip("Whether this formation should turn around its center or one of it front " +
             "corners.")]
    [SerializeField] private bool realisticTurns;

    [Tooltip("Maximum deviation over one to target direction before switching to " +
             "straight movement.")]
    [Range(0.0f, 1.0f)]
    [SerializeField] private float deviationToleranceBeforeTurn = 0.1f;
    
    [Header("USHER FORMATION WIRING:")]
    [SerializeField] private HingeJoint2D rightHinge;
    [SerializeField] private HingeJoint2D leftHinge;
    
    [Header("DEBUG:")]
    [SerializeField] private bool showGizmos;
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
    
    /// <summary>
    /// Whether this formation should turn around its center or one of its front
    /// corners.
    /// </summary>
    public bool RealisticTurns
    {
        get => realisticTurns;
        set
        {
            realisticTurns = value;
            if (!realisticTurns && _engagedHinge) DisengageHinge();
        }
    }

    /// <summary>
    /// Maximum deviation over one to target direction before switching to
    /// straight movement.
    /// </summary>
    public float DeviationToleranceBeforeTurn
    {
        get => deviationToleranceBeforeTurn;
        set => deviationToleranceBeforeTurn = value;
    }

    private Vector2 _rightHingeRelativePosition;
    private Vector2 _leftHingeRelativePosition;
    private HingeJoint2D _engagedHinge;
    private float _hingeSpeed;
    
    private bool _showGizmos;
    private Color _gizmosColor;
    
    /// <summary>
    /// Whether the agent is currently executing a left turn.
    /// </summary>
    /// <remarks>
    /// This property evaluates to true when the hinge agent engaged for steering
    /// matches the left hinge of the formation. 
    /// </remarks>
    private bool IsTurningLeft => _engagedHinge == leftHinge;
    
    /// <summary>
    /// Whether the agent is currently executing a right turn.
    /// </summary>
    /// <remarks>
    /// This property evaluates to true when the hinge agent engaged for steering
    /// matches the right hinge of the formation. 
    /// </remarks>
    private bool IsTurningRight => _engagedHinge == rightHinge;


    /// <summary>
    /// Indicates whether the agent is currently executing any type of turn.
    /// </summary>
    /// <remarks>
    /// This property evaluates to true if the agent's engaged hinge is steering
    /// either to the left or to the right. 
    /// </remarks>
    private bool IsTurning => IsTurningLeft || IsTurningRight;
    
    protected override void Start()
    {
        base.Start();
        
        // Limit rotational speed if needed to comply with the maximum speed of formation
        // members.
        float turnRadius = Vector2.Distance(rightHinge.anchor, leftHinge.anchor);
        // Tangential speed es equal to the turn radius times the maximum rotational
        // speed (in radians per second).
        float externalAgentTangentialSpeed =
            Mathf.Deg2Rad * MaximumRotationalSpeed * turnRadius;
        // If maximum rotational speed makes the most external agent (from the turn hinge)
        // go faster than its maximum speed, then we must limit the maximum rotational
        // speed for the hinges.
        if (externalAgentTangentialSpeed > MaximumSpeed)
        {
            float maximumRotationalDegPossible = Mathf.Rad2Deg * 
                                                 (MaximumSpeed / turnRadius);
            _hingeSpeed = maximumRotationalDegPossible;
        }
        else
        {
            _hingeSpeed = MaximumRotationalSpeed;
        }
        rightHinge.useMotor = false;
        leftHinge.useMotor = false;
    }

    /// <summary>
    /// Set hinge rotational speed.
    /// </summary>
    /// <param name="hinge">Hinge to set.</param>
    /// <param name="speed">New rotational speed.</param>
    private void SetHingeRotationalSpeed(HingeJoint2D hinge, float speed)
    {
        // You cannot set the motor speed directly. You must set the motor and then
        // reassign that motor.
        // Got from:
        // https://discussions.unity.com/t/hinge-joint-2d-change-motor-speed/627212/4
        JointMotor2D motor = hinge.motor;
        motor.motorSpeed = speed;
        hinge.motor = motor;
    }
    
    protected override void FixedUpdate()
    {
        if (!RealisticTurns)
        {
            base.FixedUpdate();
        }
        else
        {
            UpdateSteeringBehaviorArgs(Time.fixedDeltaTime);
            
            SteeringOutput steeringOutput = SteeringBehavior.GetSteering(behaviorArgs);
            
            switch (GetMovementNeeded(steeringOutput.Linear))
            {
                case MovementNeeded.Stop:
                    rigidBody.linearVelocity = Vector2.zero;
                    break;
                case MovementNeeded.TurnLeft:
                    TurnLeft();
                    break;
                case MovementNeeded.GoStraight:
                    GoStraight(steeringOutput.Linear);
                    break;
                case MovementNeeded.TurnRight:
                    TurnRight();
                    break;
            }
        }
    }
    
    /// <summary>
    /// Determines the type of movement adjustment needed based on the provided direction.
    /// </summary>
    /// <param name="direction">The direction vector representing the desired movement of
    /// the agent.</param>
    /// <returns>A <see cref="MovementNeeded"/> value indicating whether the agent should
    /// turn left, go straight, or turn right.</returns>
    private MovementNeeded GetMovementNeeded(Vector2 direction)
    {
        if (direction.magnitude < StopSpeed) return MovementNeeded.Stop;
        if (Vector2.Dot(Forward, direction.normalized) > 
            1 - DeviationToleranceBeforeTurn) return MovementNeeded.GoStraight;
        if (Vector3.Cross(direction, Forward).z < 0) 
            return MovementNeeded.TurnLeft;
        return MovementNeeded.TurnRight;
    }
    
    /// <summary>
    /// Adjusts the agent's velocity to move in a straight direction based on the provided
    /// vector.
    /// </summary>
    /// <param name="direction">The direction vector representing the straight movement to
    /// be applied to the agent's velocity.</param>
    private void GoStraight(Vector2 direction)
    {
        if (IsTurning)
        {
            DisengageHinge();
        }
        // After turns, some angular velocity may remain. We must remove it.
        rigidBody.angularVelocity = 0;
        rigidBody.linearVelocity = direction;
    }


    /// <summary>
    /// Rotate the agent clockwise based on the specified direction vector, using
    /// right hinge as axis.
    /// </summary>
    private void TurnRight()
    {
        Turn(MovementNeeded.TurnRight);
    }

    /// <summary>
    /// Rotate the agent counterclockwise based on the specified direction vector, using
    /// left hinge as axis.
    /// </summary>
    private void TurnLeft()
    {
        Turn(MovementNeeded.TurnLeft);
    }

    private void Turn(MovementNeeded movementNeeded)
    {
        bool justStartedTurning = 
            (movementNeeded == MovementNeeded.TurnRight && !IsTurningRight) ||
            (movementNeeded == MovementNeeded.TurnLeft && !IsTurningLeft);
        
        if (justStartedTurning)
        { // We just started turning from the other direction or from straight movement.
            if (IsTurning)
            { // If we were already turning to the other direction, stop it.
                DisengageHinge();
            }
            // Setup hinge to turn to the new direction.
            EngageHinge(movementNeeded == MovementNeeded.TurnRight ? 
                rightHinge : 
                leftHinge);
        }
    }

    /// <summary>
    /// Prepare the hinge to start a formation turning.
    /// </summary>
    /// <param name="hinge">Hinge to set up.</param>
    private void EngageHinge(HingeJoint2D hinge)
    {
        _engagedHinge = hinge;
        // Hinge anchor position in global space need to be recalculated everytime the
        // hinge is activated. A trick to force that recalculation is to put
        // autoConfigureConnectedAnchor to false and afterward to true.
        // Source:
        // https://discussions.unity.com/t/how-can-i-make-a-moving-hinge-joint/627080/6
        _engagedHinge.autoConfigureConnectedAnchor = true;
        _engagedHinge.useConnectedAnchor = true;
        SetHingeRotationalSpeed(
            hinge, 
            _engagedHinge == rightHinge? _hingeSpeed: -_hingeSpeed);
        _engagedHinge.useMotor = true;
    }
    
    /// <summary>
    /// Disable the hinge as the turn ends.
    /// </summary>
    private void DisengageHinge()
    {
        SetHingeRotationalSpeed(_engagedHinge, 0);
        _engagedHinge.autoConfigureConnectedAnchor = false;
        _engagedHinge.useConnectedAnchor = false;
        _engagedHinge.useMotor = false;
        _engagedHinge = null;
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
        ITargeter targeter = SteeringBehavior as ITargeter;
        if (targeter == null) return;

        // Draw a line to target.
        Gizmos.color = gizmosColor;
        Gizmos.DrawLine(transform.position, targeter.Target.transform.position);
        Gizmos.DrawWireSphere(
            targeter.Target.transform.position, 
            0.3f);
    }
#endif
}
}