using UnityEngine;

namespace Pathfinding
{
/// <summary>
/// Classes implementing inheriting this class are responsible for navigating a graph
/// structure to find a path to a target position.
/// <remarks>
/// The heuristic pathfinders are informed searchers that use heuristics to
/// estimate which graph branches are more promising to get the goal so they can be
/// explored first.
/// </remarks> 
/// </summary>
public abstract class HeuristicGraphPathFinder<T, TU>: GraphPathFinder<T>
    where T: NodeRecord, new()
    where TU: PrioritizedNodeRecordSet<T>, new()
{
    public T currentNodeRecord;
    protected readonly TU openRecordSet = new();
    
    public delegate bool EndCondition();

    public override PathData FindPath(
        Vector2 targetPosition,
        Vector2 fromPosition = default)
    {
        // Get graph nodes associated with the start and target positions. 
        CurrentStartNode = fromPosition==default? 
            Graph.GetNodeAtPosition(transform.position): 
            Graph.GetNodeAtPosition(fromPosition);
        CurrentTargetNode = Graph.GetNodeAtPosition(targetPosition);

        CalculateCosts(CurrentStartNode, 
            () => currentNodeRecord.node.Id == CurrentTargetNode.Id);
        
        // If we get here and the current record does not point to the targetNode, then
        // we've fully explored the graph without finding a valid path to get the target.
        if (currentNodeRecord?.node == null || 
            currentNodeRecord.node.Id != CurrentTargetNode.Id) return null;
    
        // As we've got the target node, analyze the closedDict to follow back
        // connections from the target node to start node to build the path.
        PathData calculatedPath = BuildPath(
            closedDict, 
            CurrentStartNode, 
            CurrentTargetNode);
        return calculatedPath;
    }
    
    public abstract void CalculateCosts(
        IPositionNode startNode, 
        EndCondition endCondition);
}
}