using Pathfinding;
using UnityEngine;

namespace SteeringBehaviors
{
/// <summary>
/// The <c>PipelinePathDecomposer</c> class is responsible for decomposing a high-level
/// position steering goal into smaller, incremental positional goals along a computed path.
/// </summary>
public class PipelinePathDecomposer: MonoBehaviour, IPipelineDecomposer
{
    [Header("CONFIGURATION:")] 
    [SerializeField] private float minimumChunkLength = 3f;
    
    [Header("WIRING:")]
    [SerializeField] private MeshPathFinderSteeringBehavior meshPathFinderSteeringBehavior;
    
    private Vector2 _currentGoalPosition;
    private Path _currentPath;
    private Path _partialPath;

    private void Start()
    {
        _partialPath = new GameObject($"{name} - Partial Path").AddComponent<Path>();
    }


    public PipelineGoal Decompose(PipelineGoal goal, SteeringBehaviorArgs args)
    {
        meshPathFinderSteeringBehavior.TargetPosition = goal.Position;

        _currentPath = meshPathFinderSteeringBehavior.CurrentPath;

        if (_currentPath == null) return goal;
        
        // If the path is too short, then we don't need to decompose it.
        _partialPath.ClearPath();
        foreach (Vector2 pathTargetPosition in _currentPath.positions)
        {
            _partialPath.AppendPosition(pathTargetPosition);
            if (_partialPath.PathLength >= minimumChunkLength) break;
        }
        if (_partialPath.positions.Count <= 1) return goal;
        
        // Goal is passed by reference, so we are updating directly the passed in
        // parameter. It doesn't matter because decomposers are run sequentially.
        goal.Position = _partialPath.positions[^1];
        
        return goal;
    }
}
}