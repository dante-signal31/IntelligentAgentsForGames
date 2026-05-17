using Pathfinding;
using UnityEngine;

namespace SteeringBehaviors
{
/// <summary>
/// Represents an aggregator responsible for managing and applying
/// pipeline constraint logic during pathfinding and steering behaviors. It collects
/// child nodes implementing <c>IPipelineConstraint</c> and applies their constraints
/// to determine the validity of a given pipeline goal and the associated movement path.
/// </summary>
public class PipelineConstraintsAggregator: MonoBehaviour
{
   [Header("CONFIGURATION:")] 
   [Tooltip("Maximum number of tries to find a valid constraint.")]
   // If we want to give every constraint the chance to find a violation and propose an
   // alternative, then we must set this value at least to the same value as the number
   // of constraints.
   [SerializeField] public int maximumTries = 2;
   
   private IPipelineConstraint[] _pipelineConstraints;

   private void Start()
   {
      // Be aware that order matters. Constraints will be evaluated in the order they are
      // in the tree. So, they will be evaluated from top to bottom.
      _pipelineConstraints = GetComponentsInChildren<IPipelineConstraint>();
   }

   public PipelineGoal ApplyConstraints(
      PipelineGoal goal, 
      SteeringBehaviorArgs args,
      IPipelineActuator actuator)
   {
      // goal is passed by reference, so I allocate a new one to not overwrite the
      // original goal.
      PipelineGoal validGoal = goal.GetGoalCopy();
        
      for (int i = 0; i < maximumTries; i++)
      {
         bool violationFound = false;
         // Get the path the actuator would choose to get the goal.
         Path path = actuator.GetPath(validGoal, args);

         // Check if any constraint is violated by the path or the goal.
         foreach (IPipelineConstraint constraint in _pipelineConstraints)
         {
            if (constraint.IsViolated(path, validGoal, args))
            {
               violationFound = true;
                    
               // If so, try to find a new goal that does not violate the constraint.
               validGoal = constraint.SuggestGoal(path, validGoal, args);
                    
               // Make a new round to check if suggested goal complies with the
               // constraints.
               break;
            }
         }
         if (!violationFound) break;
      }
        
      return validGoal;
   }
}
}