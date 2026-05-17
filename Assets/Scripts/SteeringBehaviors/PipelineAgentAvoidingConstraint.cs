using Pathfinding;
using Tools;
using UnityEngine;

namespace SteeringBehaviors
{
public class PipelineAgentAvoidingConstraint: MonoBehaviour, IPipelineConstraint
{
    [Header("WIRING:")]
    [SerializeField] private VOAgentAvoiderSteeringBehavior voAvoider;
    
    private PotentialCollisionDetector _collisionDetector;
    private float _currentSpeed;

    private void Start()
    {
        _collisionDetector = 
            voAvoider.GetComponentInChildren<PotentialCollisionDetector>();
    }

    public bool IsViolated(Path pathToGoal, PipelineGoal goal, SteeringBehaviorArgs args)
    {
        // Check whether the velocity vector to get next path point is going to make
        // us collide with another agent.
        Vector2 velocityToFirstPoint = 
            GetVelocityToFirstPathPoint(pathToGoal, goal, args);
        
        return _collisionDetector.IsCollidingVelocity(
            velocityToFirstPoint,
            args.CurrentAgent.GetComponent<AgentMover>().Radius, 
            out _);
    }
    
    private Vector2 GetVelocityToFirstPathPoint(
        Path pathToGoal, 
        PipelineGoal goal, 
        SteeringBehaviorArgs args)
    {
        if (pathToGoal.positions.Count < 2) return Vector2.zero;
        // Remember that the zero index point is the current position. So, you must
        // use de 1 index point to get the velocity vector to the next point.
        Vector2 directionToNextPoint = 
            (pathToGoal.positions[1] - (Vector2) transform.position).normalized;
        float speed = goal.HasSpeed? goal.Speed : args.MaximumSpeed;
        Vector2 velocityToNextPoint = directionToNextPoint * speed;
        return velocityToNextPoint;
    }

    public PipelineGoal SuggestGoal(
        Path pathToGoal, 
        PipelineGoal goal, 
        SteeringBehaviorArgs args)
    {
        // Update the sampling disc if the desired speed has changed.
        float desiredSpeed = goal.HasSpeed? goal.Speed : args.MaximumSpeed;
        if (!Mathf.Approximately(desiredSpeed, _currentSpeed))
        {
            voAvoider.UpdateSamplingDisc(desiredSpeed);
            _currentSpeed = desiredSpeed;
        }
        
        // Check whether the velocity vector to get next path point is going to make
        // us collide with another agent.
        Vector2 velocityToFirstPoint = 
            GetVelocityToFirstPathPoint(pathToGoal, goal, args);
        
        if (!_collisionDetector.IsCollidingVelocity(
                velocityToFirstPoint, 
                (args.CurrentAgent.GetComponent<AgentMover>()).Radius, 
                out _)) return goal;
        
        // If we are going to collide with an agent, get the velocity vector most similar
        // to the one to get us to the first path point. Then convert that velocity vector 
        // into a synthetic goal.
        Vector2 bestCandidateVelocity =
            voAvoider.GetBestCandidateVelocity(velocityToFirstPoint);
        
        PipelineGoal avoidGoal = goal.GetGoalCopy();
        avoidGoal.Position = (Vector2)transform.position + bestCandidateVelocity;
        return avoidGoal;
    }
}
}