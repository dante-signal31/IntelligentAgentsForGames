using System.Collections.Generic;
using PropertyAttribute;
using Sensors;
using Tools;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SteeringBehaviors
{
/// <summary>
/// The ContextSteeringBehavior class is responsible for implementing a context-based
/// steering mechanism. This behavior processes environmental data using sensors, such as
/// danger detection, and dynamically adjusts the steering output to avoid obstacles
/// while pursuing the target goal. The algorithm uses interests and contextual
/// vectors to make nuanced adjustments based on the environment.
/// </summary>
/// <remarks>
/// The steering logic filters interest vectors based on detected obstacles, calculates
/// valid directions, and determines the final steering output by taking into account
/// both high-priority and lower-priority interests. The resolution of the context and
/// distance radius for the context can be adjusted via properties to fine-tune the
/// behavior. 
/// </remarks>
[ExecuteAlways]
public class ContextSteeringBehavior : SteeringBehavior
{
    private class InterestComparer : IComparer<Interest>
    {
        private readonly Transform currentAgentTransform;
        
        public int Compare(
            Interest a, 
            Interest b)
        {
            // Compare the values.
            int result = b.value.CompareTo(a.value);
            if (result != 0) return result;
            
            // If the values are equal, give preference to the minimum angle from
            // agent's forward vector'.
            float angleX = Mathf.Abs(
                Vector2.Angle(a.direction, currentAgentTransform.up));
            float angleY = Mathf.Abs(
                Vector2.Angle(b.direction, currentAgentTransform.up));
            result = angleX.CompareTo(angleY);
            if (result != 0) return result;
            
            // Final deterministic tie-breaker.
            return a.direction.x.CompareTo(b.direction.x);
        }
        
        public InterestComparer(Transform currentAgentTransform)
        {
            this.currentAgentTransform = currentAgentTransform;
        }
    }

    [Header("CONFIGURATION:")] 
    [Tooltip("Number of whiskers to use for context. Whiskers will be set complying " +
             "with its internal rules. So final number of whiskers may not be " +
             "exactly equal to this value. ")]
    [SerializeField] private uint contextResolution = 10;
    [Tooltip("Radius of the context circle.")]
    [SerializeField] private float contextRadius = 1.0f;
    [Tooltip("How many remaining interests from context to add to get the final result.")]
    [SerializeField] public uint addedInterests = 2;
    
    [Header("WIRING")]
    [InterfaceCompliant(typeof(ITargeter))]
    [SerializeField] private SteeringBehavior behavior;
    [SerializeField] private WhiskersSensor dangerSensor;
    [SerializeField] private InterestWhiskers interestWhisker;
    
    [Header("DEBUG:")]
    [SerializeField] private bool showGizmos;
    [SerializeField] private Color gizmosColor;
    [SerializeField] private Color gizmosColorInterest;
    
    /// <summary>
    /// <p>Number of whiskers to use for context.</p>
    /// <p>Whiskers will be set complying with its internal rules. So the final number
    /// of whiskers may not be exactly equal to this value.</p> 
    /// </summary>
    public uint ContextResolution
    {
        get => contextResolution;
        set
        {
            contextResolution = value;
            ConfigureWhiskers();
        }
    }
    
    /// <summary>
    /// Radius of the context circle.
    /// </summary>
    public float ContextRadius
    {
        get => contextRadius;
        set
        {
            contextRadius = value;
            ConfigureWhiskers();
        }
    }
    
    private AgentMover _currentAgent;
    private List<Interest> _interests = new();
    private Vector2 _currentSteeringVector;

    /// <summary>
    /// Configures the whisker components for the context-steering behavior.
    /// </summary>
    /// <remarks>
    /// This method ensures that both the danger sensor and interest whisker are
    /// properly configured to align with the current context resolution and radius.
    /// </remarks>
    private void ConfigureWhiskers()
    {
        ConfigureDangerSensor();
        ConfigureInterestWhisker();
    }

    /// <summary>
    /// Configures the interest whisker component for the context-steering behavior.
    /// </summary>
    /// <remarks>
    /// This method ensures that the interest whisker is updated with the ray endpoints
    /// from the danger sensor, allowing it to function correctly within the context
    /// steering logic.
    /// </remarks>
    private void ConfigureInterestWhisker()
    {
        if (interestWhisker == null) return;
        interestWhisker.ReloadWhiskers(dangerSensor.rayEnds);
    }

    /// <summary>
    /// Configures the danger sensor used in the context steering behavior.
    /// </summary>
    /// <remarks>
    /// This method sets the sensor's resolution based on the current context resolution
    /// and adjusts the range of the sensor to match the context radius.
    /// </remarks>
    private void ConfigureDangerSensor()
    {
        if (dangerSensor == null) return;
        dangerSensor.SensorResolution = 
            (int) Mathf.Max(0, Mathf.Ceil(((int)ContextResolution - 3.0f) / 2));
        dangerSensor.Range = ContextRadius;
    }
    
    /// <summary>
    /// Counts the number of trues in the given range of the mask.
    /// </summary>
    /// <param name="mask">Mask to assess.</param>
    /// <param name="start">Index where assess starts.</param>
    /// <param name="end">Index where assess ends (exclusive).</param>
    /// <returns>Number of bits set to true in the given range of the mask.</returns>
    private int CountTruesInRange(List<bool> mask, int start, int end)
    {
        int count = 0;
        for (int i = start; i < end; i++)
        {
            if (mask[i]) count++;
        }
        return count;
    }

    /// <summary>
    /// Sets the given range of bits of the mask to true.
    /// </summary>
    /// <param name="mask">Mask whose bits we are going to set.</param>
    /// <param name="start">Index where to start to set bits.</param>
    /// <param name="end">Index where to end the bit setup (exclusive).</param>
    private void SetRangeToTrue(List<bool> mask, int start, int end)
    {
        for (int i = start; i < end; i++)
        {
            mask[i] = true;
        }
    }

    private void Start()
    {
        ConfigureWhiskers();
        _currentAgent = GetComponentInParent<AgentMover>();
    }

    public override SteeringOutput GetSteering(SteeringBehaviorArgs args)
    {
        SteeringOutput steering = behavior.GetSteering(args);
        _currentSteeringVector = steering.Linear;
        // If there is no danger, we don't need to do anything. Just go straight to
        // the target.
        if (!dangerSensor.IsAnyColliderDetected) return steering;
        
        // If any obstacle detected, then get fresh interests.
        interestWhisker.CalculateInterests(steering.Linear);
        _interests = interestWhisker.GetInterests();
        
        // We want to decisively avoid obstacles. Therefore, we will filter the vectors
        // of interest that lie on the side where more obstacles have been detected.
        List<bool> detectionMask = dangerSensor.DetectionMask;
        int centerBitIndex = detectionMask.Count / 2;

        // Analyze both halves and set the half with more trues to be completely true.
        int leftTrues = CountTruesInRange(detectionMask, 0, centerBitIndex);
        int rightTrues = CountTruesInRange(
            detectionMask, 
            centerBitIndex, 
            detectionMask.Count);

        if (leftTrues > rightTrues)
        {
            SetRangeToTrue(detectionMask, 0, centerBitIndex);
        }
        else if (rightTrues > leftTrues)
        {
            SetRangeToTrue(detectionMask, centerBitIndex, detectionMask.Count);
        }
        
        // Once completely filtered the side with more detections, sort the
        // remaining interests by their value to get the highest ones later.
        List<Interest> validInterests = new();
        int index = 0;
        foreach (Interest interest in _interests)
        {
            if (detectionMask[index++]) continue;
            validInterests.Add(interest);
        }
        validInterests.Sort(new InterestComparer(transform));
        
        // We use the highest interest nuanced by the lower interests.
        _currentSteeringVector = Vector2.zero;
        int highestInterestIndex = 0;
        foreach (Interest interest in validInterests)
        {
            if (highestInterestIndex++ >= addedInterests) break;
            _currentSteeringVector += interest.direction * interest.value;
        }
        
        _currentSteeringVector = _currentSteeringVector.normalized * args.MaximumSpeed;
        // If you are walking through hell, keep walking.
        if (_currentSteeringVector.magnitude == 0) 
            _currentSteeringVector = _currentAgent.Forward * args.MaximumSpeed;
        return new SteeringOutput(_currentSteeringVector, steering.Angular);
    }
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        RequestEditorRefresh();
    }

    private void RequestEditorRefresh()
    {
        // We only want to update the editor when the game is not playing.
        if (Application.isPlaying) return;
        
        EditorApplication.delayCall -= ApplyEditorRefresh;
        EditorApplication.delayCall += ApplyEditorRefresh;
    }

    private void ApplyEditorRefresh()
    {
        if (this == null) return;
        if (Application.isPlaying) return;
        
        ConfigureWhiskers();
        SceneView.RepaintAll();
    }
    
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        // Draw valid interests.
        int index = 0;
        foreach (Interest interest in _interests)
        {
            if (dangerSensor.DetectionMask[index++] || 
                !dangerSensor.IsAnyColliderDetected) continue;
            Gizmos.color = gizmosColorInterest;
            Gizmos.DrawLine(
                transform.position,
                transform.position + 
                (Vector3) interest.direction.normalized * interest.value);
        }
        
        // Draw resulting steering.
        Gizmos.color = gizmosColor;
        Gizmos.DrawLine(
            transform.position,
            transform.position + (Vector3) _currentSteeringVector);
    }
#endif
}
}

