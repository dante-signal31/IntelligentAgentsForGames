using System.Collections.Generic;
using System.Linq;
using Levels;
using Pathfinding;
using Sensors;
using UnityEngine;

namespace SteeringBehaviors
{
public class PipelineHidingConstraint: MonoBehaviour, IPipelineConstraint
{
    [Header("CONFIGURATION:")] 
    [Tooltip("Agent to hide from.")]
    [SerializeField] private AgentMover threat;
    [Tooltip("How many used hiding points to remember to avoid loops?")]
    [SerializeField] private int usedHidingPointsMemorySize = 3;
    [SerializeField] private float usedHidingPointsRadius = 0.5f;
    
    [Header("WIRING:")]
    [SerializeField] private Tools.HidingPointsDetector hidingPointsDetector;
    [SerializeField] private RaySensor threatVisibilitySensor;

    /// <summary>
    /// Agent to hide from.
    /// </summary>
    public AgentMover Threat
    {
        get => threat;
        set
        {
            threat = value;
            if (hidingPointsDetector != null)  
                hidingPointsDetector.Threat = value.gameObject;
            if (threatVisibilitySensor != null)
                threatVisibilitySensor.detectionLayers = 
                    (1 << Threat.gameObject.layer) | hidingPointsDetector.ObstaclesLayer;
        }
    }
    
    /// <summary>
    /// Whether threat can see the current agent.
    /// </summary>
    /// <returns>True is threat can see the current agent. Otherwise, false.</returns>
    public bool ThreatCanSeeUs => IsPositionVisibleByThreat(transform.position);
    
    private Courtyard _currentLevel;
    private Vector2 _alternativeGoalPosition = Vector2.positiveInfinity;
    private readonly Queue<Vector2> _alreadyUsedHidingPoints = new();

    private void Start()
    {
        Threat = threat;
        InitializeHidingPointsDetector();
        InitializeThreatVisibilitySensor();
    }

    private void InitializeThreatVisibilitySensor()
    {
        threatVisibilitySensor.name = $"{name} - ThreatVisibilitySensor";
        // Set RaySensor as a child of the scene root to avoid moving it when this agent
        // moves.
        threatVisibilitySensor.transform.parent = null;
    }

    private void OnDestroy()
    {
        // RaySensor is not our child, so it is not destroyed automatically when the 
        // object is. So we must destroy it manually.
        Destroy(threatVisibilitySensor);
    }

    private void InitializeHidingPointsDetector()
    {
        _currentLevel = FindAnyObjectByType<Courtyard>();
        hidingPointsDetector.ObstaclesPositions = _currentLevel.ObstaclePositions;
    }
    
    /// <summary>
    /// Determines whether a given position is visible to the threat agent.
    /// </summary>
    /// <param name="position">The position to check visibility.</param>
    /// <returns>True if the position is visible to the threat agent;
    /// otherwise, false.</returns>
    private bool IsPositionVisibleByThreat(Vector2 position)
    {
        threatVisibilitySensor.transform.position = position;
        threatVisibilitySensor.GlobalEndPosition = Threat.transform.position;
        threatVisibilitySensor.UpdateRay();
        if (threatVisibilitySensor.AnyObjectDetected)
        {
            GameObject detectedObject = threatVisibilitySensor.FirstDetectedObject.gameObject;
            return detectedObject.name == Threat.name;
        }
        
        return false;
    }

    /// <summary>
    /// Finds the nearest hiding position from a specified position.
    /// </summary>
    /// <param name="position">The position from which the search for the nearest
    /// hiding position starts.</param>
    /// <param name="hidingPoints">A subset of the hiding point to choose from. If not
    /// given, the entire set will be used.</param>
    /// <returns>The nearest hiding position as a <see cref="Vector2"/>.</returns>
    private Vector2 GetNearestHidingPosition(
        Vector2 position, 
        Vector2[] hidingPoints = null)
    {
        if (hidingPoints == null) 
            hidingPoints = hidingPointsDetector.HidingPoints.ToArray();
        
        // If there are no hiding points, then return the position.
        if (hidingPoints.Length == 0) return position;
        
        // Find the nearest hiding point.
        Vector2 nearestHidingPosition = Vector2.zero;
        float nearestHidingDistance = float.MaxValue;
        foreach (Vector2 hidingPosition in hidingPoints)
        {
            float distance = Vector2.Distance(position, hidingPosition);
            if (distance < nearestHidingDistance)
            {
                nearestHidingDistance = distance;
                nearestHidingPosition = hidingPosition;
            }
        }
        return nearestHidingPosition;   
    }
    
    private void RegisterUsedHidingPoint(Vector2 hidingPosition)
    {
        if (_alreadyUsedHidingPoints.Count >= usedHidingPointsMemorySize)
            _alreadyUsedHidingPoints.Dequeue();
        _alreadyUsedHidingPoints.Enqueue(hidingPosition);   
    }

    private bool IsHidingPointUsed(Vector2 hidingPosition)
    {
        foreach (Vector2 alreadyUsedHidingPoint in _alreadyUsedHidingPoints)
        {
            if (Vector2.Distance(hidingPosition, alreadyUsedHidingPoint) < 
                usedHidingPointsRadius) 
                return true;
        }
        return false;
    }

    private Vector2[] GetUnusedHidingPoints()
    {
        HashSet<Vector2> unusedHidingPoints = new();
        foreach (Vector2 hidingPoint in hidingPointsDetector.HidingPoints)
        {
            if (!IsHidingPointUsed(hidingPoint)) unusedHidingPoints.Add(hidingPoint);
        }
        return unusedHidingPoints.ToArray();
    }

    public bool IsViolated(Path pathToGoal, PipelineGoal goal, SteeringBehaviorArgs args)
    {
        if (pathToGoal.positions.Count < 1) return false;
        
        // If we are already heading to a hiding point, then we don't need to check
        // whether we are visible. We chose the nearest hiding point, and we may need to
        // go through a visible zone to get to it.
        if (pathToGoal.positions.Count > 1 && 
            Vector2.Distance(pathToGoal.positions[^1], _alternativeGoalPosition) < 
            usedHidingPointsRadius) 
            return false;
        
        // Check whether any of path points is visible from the threat agent.
        // Discard the first point because it is the current position.
        Vector2[] pathPoints = pathToGoal.positions.GetRange(
            1, 
            pathToGoal.positions.Count - 1).ToArray();
        foreach (Vector2 pathPosition in pathPoints)
        {
            if (IsPositionVisibleByThreat(pathPosition)) return true;
        }
        return false;
    }

    public PipelineGoal SuggestGoal(Path pathToGoal, PipelineGoal goal, SteeringBehaviorArgs args)
    {
        // Discard the first point because it is the current position.
        Vector2[] pathPoints = pathToGoal.positions.GetRange(
            1, 
            pathToGoal.positions.Count - 1).ToArray();
        
        // If any of the path points is visible from the threat agent,look for the hiding
        // point nearest to that path point. That hiding point will become the new
        // positional goal. When that goal is reached, a new evaluation will be performed
        // to get the high-level goal, but at that moment the threat could have moved and
        // a new hidden path can be achieved. To avoid loops in the hiding path, we
        // remember the hiding points that we have already used to not use them again.
        _alternativeGoalPosition = Vector2.zero;
        bool violationFound = false;
        foreach (Vector2 pathPosition in  pathPoints)
        {
            if (!IsPositionVisibleByThreat(pathPosition)) continue;
            violationFound = true;
            _alternativeGoalPosition = GetNearestHidingPosition(
                pathPosition,
                GetUnusedHidingPoints());
            RegisterUsedHidingPoint(_alternativeGoalPosition);
            break;
        }
        
        if (!violationFound) return goal;

        PipelineGoal hiddenGoal = goal.GetGoalCopy();
        hiddenGoal.Position = _alternativeGoalPosition;
        return hiddenGoal;
    }
}
}