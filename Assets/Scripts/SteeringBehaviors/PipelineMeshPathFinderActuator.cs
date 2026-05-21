using Pathfinding;
using UnityEngine;

namespace SteeringBehaviors
{
/// <summary>
/// Actuator that uses a MeshPathFinderSteeringBehavior to get a path to a goal and a
/// steering output to move the agent along the calculated path.
/// </summary>
public class PipelineMeshPathFinderActuator: MonoBehaviour, IPipelineActuator
{
    [Header("WIRING:")]
    [SerializeField] private MeshPathFinderSteeringBehavior meshPathFinderSteeringBehavior;
    
    private Vector2 currentGoalPosition;

    public Path GetPath(PipelineGoal goal, SteeringBehaviorArgs args)
    {
        if (currentGoalPosition != goal.Position)
        {
            meshPathFinderSteeringBehavior.TargetPosition = goal.Position;
            currentGoalPosition = goal.Position;
        }
        
        return meshPathFinderSteeringBehavior.CurrentPath;
    }

    public SteeringOutput GetOutput(PipelineGoal goal, SteeringBehaviorArgs args)
    {
        meshPathFinderSteeringBehavior.TargetPosition = goal.Position;
        
        SteeringOutput steeringOutput = 
            meshPathFinderSteeringBehavior.GetSteering(args);
        SteeringOutput output = new(
            linear: goal.HasSpeed? 
                steeringOutput.Linear.normalized * goal.Speed: 
                steeringOutput.Linear.normalized * args.MaximumSpeed,
            angular: goal.HasRotation? goal.Rotation: SteeringOutput.angularUnset);
        return output;
    }
}
}