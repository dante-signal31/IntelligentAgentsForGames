
using Sensors;
using SteeringBehaviors;
using UnityEngine;

namespace Tools
{
/// <summary>
/// <p>Script to detect future collisions with other agents nearby.</p>
/// </summary>
[RequireComponent(typeof(BoxRangeManager))]
[ExecuteAlways]
public class PotentialCollisionDetector : MonoBehaviour
{
    [Header("CONFIGURATION:")]
    [Tooltip("Range to detect possible collisions.")]
    [SerializeField] private float detectionRange = 10f;
    [Tooltip("Semicone angle for detection (in degrees).")]
    [Range(0f, 90f)]
    [SerializeField] private float detectionSemiconeAngle = 45f;
    [Tooltip("Specifies the physics layers that the will monitor for potential " +
             "collisions.")]
    [SerializeField] private LayerMask layersToDetect;
    [Tooltip("Represents the radius of an agent used for potential collision detection.")]
    [SerializeField] private float agentRadius;

    /// <summary>
    /// Range to detect possible collisions.
    /// </summary>
    public float DetectionRange
    {
        get => detectionRange;
        set
        {
            detectionRange = value;
            UpdateDetectionArea();
        }
    }
    
    /// <summary>
    /// Semicone angle for detection (in degrees).
    /// </summary>
    public float DetectionSemiconeAngle
    {
        get => detectionSemiconeAngle;
        set
        {
            detectionSemiconeAngle = value;
            UpdateDetectionArea();
        }
    }
    
    /// <summary>
    /// Specifies the physics layers that the will monitor for potential collisions.
    /// </summary>
    public LayerMask LayersToDetect
    {
        get => layersToDetect;
        set => layersToDetect = value;
    }
    
    /// <summary>
    /// Represents the radius of an agent used for potential collision detection.
    /// </summary>
    public float AgentRadius
    {
        get => agentRadius;
        set
        {
            agentRadius = value;
            CollisionDistance = 2 * agentRadius;
        }
    }
    public bool PotentialCollisionDetected { get; private set; }
    
    public AgentMover PotentialCollisionAgent { get; private set; }
    
    public float TimeToPotentialCollision {get; private set; }
    
    public Vector2 RelativePositionAtPotentialCollision {get; private set; }
    
    public float SeparationAtPotentialCollision {get; private set; }
    
    public Vector2 CurrentRelativePositionToPotentialCollisionAgent {get; private set; }
    
    public Vector2 CurrentRelativeVelocityToPotentialCollisionAgent {get; private set; }
    
    public float CurrentDistanceToPotentialCollisionAgent => 
        CurrentRelativePositionToPotentialCollisionAgent.magnitude;
    
    public float CollisionDistance { get; private set; }

    private BoxRangeManager _boxRangeManager;
    private BoxCollider2D _detectionArea;
    private AgentMover _currentAgent;

    private void UpdateDetectionArea()
    {
        if (_boxRangeManager == null) return;
        _boxRangeManager.Range = detectionRange;
        _boxRangeManager.Width = DetectionRange * 
                                 Mathf.Sin(detectionSemiconeAngle * 
                                           Mathf.Deg2Rad);
    }
    
    private void Awake()
    {
        _boxRangeManager = GetComponent<BoxRangeManager>();
        _detectionArea = GetComponent<BoxCollider2D>();
        _currentAgent = GetComponentInParent<AgentMover>();
        CollisionDistance = 2 * agentRadius;
    }
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        UpdateDetectionArea();
    }
#endif
}
}

