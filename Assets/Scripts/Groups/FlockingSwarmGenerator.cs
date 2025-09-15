using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace SteeringBehaviors
{
/// <summary>
/// <p>Component to create a flocking swarm automatically.</p>
/// <p>Provided boid scenes are created automatically at game start around this node
/// position. Boid locations are selected randomly after checking that places are actually
/// free of any other agent.</p>
/// </summary>
public class FlockingSwarmGenerator : MonoBehaviour, IGizmos
{
    [Header("SWARM CONFIGURATION:")]
    [Tooltip("Number of boids to create.")]
    [SerializeField] private int swarmSize = 3;
    [Tooltip("Prefab to create boids.")]
    [SerializeField] private Object boidPrefab;
    
    /// <summary>
    /// <p>The radius within which individual boid agents are placed around this node's
    /// position when the flocking swarm is initialized. This value determines the
    /// spatial distribution of the boids and ensures they are positioned within a
    /// circular area of the specified radius.</p>
    /// <p>This value has the effect of creating a circle around the target point in which
    /// the flock moves. The flock is not going out of this circle.</p>
    /// </summary>
    [Tooltip("Radius of the swarm.")]
    [SerializeField] private float swarmRadius = 14f;
    
    [Tooltip("Number of retries to find a free place for a newly created boid before " +
             "giving up and let swarm as it is so far.")]
    [SerializeField] private int swarmGenerationRetries = 5;
    
    [Tooltip("Layers to look for a possible obstacle to place a new boid.")]
    [SerializeField] private LayerMask obstaclesForGenerationLayers;
    
    [Tooltip("Whether to give a random starting rotation to every boid.")]
    [SerializeField] private bool randomizeBoidRotation = true;

    [Header("BOID GENERAL CONFIGURATION:")]
    
    /// <summary>
    /// The radius used to define the personal space around each boid in the swarm.
    /// This value represents the minimum distance other boids must maintain
    /// to avoid overlap.
    /// </summary>
    [Tooltip("The radius used to define the personal space around each boid in the " +
             "swarm.")]
    [SerializeField] private float boidRadius = 0.55f;

    [Tooltip("Defines the color of the boid agents in the flocking swarm.")]
    [SerializeField] private Color boidColor = Color.green;

    [Tooltip("The maximum speed a boid can achieve while moving within the flocking " +
             "swarm.")]
    [SerializeField] private float boidMaximumSpeed = 200f;
    
    [Tooltip("The speed at which a boid is considered to have stopped.")]
    [SerializeField] private float boidStopSpeed = 10f;

    [Tooltip(" Defines the maximum rotational speed a boid in the flocking swarm can " +
             "achieve.")]
    [SerializeField] private float boidMaximumRotationalSpeed = 1080f;
    
    [Tooltip("The angle, in degrees, below which a boid's rotation effectively stops " +
             "responding to rotation behaviors.")]
    [SerializeField] private float boidStopRotationThreshold = 10f;

    [Tooltip("The maximum acceleration value for a boid within the flocking swarm.")]
    [SerializeField] private float boidMaximumAcceleration = 4f;

    [Tooltip("The maximum deceleration value for a boid within the flocking swarm.")]
    [SerializeField] private float boidMaximumDeceleration = 2f;
    
    [Tooltip("Determines whether the boid agents in the flock should automatically " +
             "smooth their movement transitions.")]
    [SerializeField] private bool boidAutoSmooth;
    
    [Tooltip("The number of samples to use when smoothing the boid agents' movement " +
             "transitions.")]
    [SerializeField] private int boidAutoSmoothSamples = 10;
    
    [Header("BOID SEPARATION CONFIGURATION")]
    [Tooltip("The distance threshold used to determine the separation behavior of " +
             "boid agents within the swarm.")]
    [SerializeField] private float boidSeparationThreshold = 4f;

    [Tooltip("The separation algorithm used to determine the separation behavior of " +
             "boid agents within the swarm.")]
    [SerializeField]
    private SeparationSteeringBehavior.SeparationAlgorithms boidSeparationAlgorithm =
        SeparationSteeringBehavior.SeparationAlgorithms.InverseSquare;

    [Tooltip("Coefficient used to calculate the inverse square law separation " +
             "algorithm.")]
    [SerializeField] private float boidSeparationDecayCoefficient = 2f;

    [Header("BOID COHESION CONFIGURATION")]
    
    /// <summary>
    /// <p>The distance within which boid agents begin to slow down as they approach
    /// the cohesion point in the flocking behavior. This value influences the
    /// formation and stability of the swarm by controlling how tightly the
    /// boids converge around their center of mass.</p>
    /// <p>Whithin the circle defined by _boidSeekArrivalDistance, cohesion behavior
    /// makes flock to stay compactly centered around the cohesion point and no
    /// further than this value.</p>
    /// </summary>
    [Tooltip("The distance within which boid agents begin to slow down as they " +
             "approach the cohesion point in the flocking behavior.")]
    [SerializeField] private float boidCohesionArrivalDistance = 5f;

    [Header("BOID SEEK CONFIGURATION")] 
    [Tooltip("The target that each boid in the flock will seek towards.")]
    [SerializeField] private GameObject boidSeekTarget;

    [Header("BOID WANDER CONFIGURATION")] 
    [Tooltip("The distance at which a wandering boid begins stops as it approaches " +
             "its target destination.")]
    [SerializeField] private float boidArrivalDistance = 0.1f;

    [Tooltip("This is the radius of the constraining circle. KEEP IT UNDER " +
             "wanderDistance!")]
    [SerializeField] private float boidWanderRadius = 3f;

    [Tooltip("This is the distance the wander circle is projected in front of the " +
             "agent. KEEP IT OVER wanderRadius!")]
    [SerializeField] private float boidWanderDistance = 6f;

    [Tooltip("Maximum amount of random displacement that can be added to the target " +
             "each second. KEEP IT OVER wanderRadius.")]
    [SerializeField] private float boidWanderJitter = 4f;

    [Tooltip("Time in seconds to recalculate the wander position.")]
    [SerializeField] private float boidWanderRecalculation = 1f;
    
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

    
    private bool boidsCreated;

    private void Start()
    {
        CreateBoids();
    }

    /// <summary>
    /// <p>Whether this point is free from obstacles to be a valid placement point.</p>
    ///
    /// <p><b>WARNING:</b> If you are using a Composite Collider 2D to make the obstacles,
    /// remember to set its Geometry Type to "Polygons" or this method may
    /// malfunction. In case it would be set to "Outlines" the method would
    /// return wrongly true if the projected circle fits inside the obstacle without
    /// touching any of its sides.</p>
    /// </summary>
    /// <param name="candidatePoint">Position to check</param>
    /// <returns>True if position is free from obstacles, false otherwise</returns>
    private bool IsCleanPoint(Vector2 candidatePoint)
    {
        Collider2D detectedCollider = Physics2D.OverlapCircle(
            candidatePoint,
            boidRadius,
            obstaclesForGenerationLayers);
        return detectedCollider == null;
    }
    
    /// <summary>
    /// <p>Creates a group of boid agents to form a swarm based on the configured swarm
    /// size. Each boid is instantiated at a valid, randomly selected position near the
    /// node, ensuring no overlap with other agents.</p>
    /// <p>Once created, the boids are configured with appropriate steering behaviors.</p>
    /// </summary>
    private void CreateBoids()
    {
        List<GameObject> createdBoids = new();
        
        // Create boids.
        for (int i=0; i < swarmSize; i++)
        {
            (bool locationFound, Vector2 spawnPosition) = FindSpawnPosition();
            
            if (locationFound)
            {
                GameObject newBoid = CreateBoid(i, spawnPosition);
                createdBoids.Add(newBoid);
            }
        }
        
        // Configure created boids.
        InitializeBoidSteeringBehaviors(createdBoids);
    }
    
    /// <summary>
    /// Attempts to find a valid spawn position for a new boid by checking if a randomly
    /// generated position is free from obstacles.
    /// </summary>
    /// <returns>A tuple containing a boolean indicating whether a valid position was
    /// found and the corresponding spawn position. If no position is found, the returned
    /// position will be (0, 0).</returns>
    private (bool, Vector2) FindSpawnPosition()
    {
        for (int tries = 0; tries < swarmGenerationRetries; tries++)
        {
            Vector2 randomLocalPosition = Random.insideUnitCircle * swarmRadius;
            Vector2 randomGlobalPosition =
                (Vector2) transform.position + randomLocalPosition;
            if (!IsCleanPoint(randomGlobalPosition)) continue;
            return (true, randomGlobalPosition);
        }
        return (false, Vector2.zero);
    }
    
    /// <summary>
    /// Creates a new boid instance, sets its properties such as name, position,
    /// and optionally randomizes its rotation, then adds it to the scene as a child node.
    /// </summary>
    /// <param name="i">The index number of the boid, used for naming the boid
    /// uniquely.</param>
    /// <param name="spawnPosition">The global position where the boid will be placed
    /// in the scene.</param>
    /// <returns>The newly created boid instance configured with the provided
    /// parameters.</returns>
    private GameObject CreateBoid(int i, Vector2 spawnPosition)
    {
        // Create the boid if a free location was found.
        var boid = (GameObject) Instantiate(
            boidPrefab, 
            transform.position, 
            Quaternion.identity);
        boid.name = $"Boid_{i}";
                
        // Add boid to the scene.
        boid.transform.parent = transform;
        
        // Boid position and rotation.
        boid.transform.position = spawnPosition;
        if (randomizeBoidRotation)
        {
            boid.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
        }
        
        return boid;
    }
    
    /// <summary>
    /// Initializes and configures the steering behaviors for a given list of boid agents.
    /// Sets up properties such as movement parameters and steering behavior thresholds,
    /// and assigns relevant targets for behaviors like separation and cohesion.
    /// </summary>
    /// <param name="createdBoids">The list of boid agents to configure. Each boid will
    /// have its steering behaviors initialized and linked with other boids in the
    /// group.</param>
    private void InitializeBoidSteeringBehaviors(List<GameObject> createdBoids)
    {
        foreach (GameObject boid in createdBoids)
        {
            AgentMover boidMover = boid.GetComponent<AgentMover>();
            AgentColor boidAgentColor = boid.GetComponent<AgentColor>();
            
            // An important caveat here is that SeeKSteeringBehavior game object should
            // be before in the hierarchy (up-to-down) than CohesionSteeringBehavior or
            // WanderSteeringBehavior. Otherwise their respective SeekSteeringBehavior
            // will be found before the standalone SeekSteeringBehavior.
            SeekSteeringBehavior seekBehavior = 
                boid.GetComponentInChildren<SeekSteeringBehavior>();
            SeparationSteeringBehavior separationBehavior = 
                boid.GetComponentInChildren<SeparationSteeringBehavior>();
            CohesionSteeringBehavior cohesionBehavior = 
                boid.GetComponentInChildren<CohesionSteeringBehavior>();
            WanderSteeringBehavior wanderBehavior = 
                boid.GetComponentInChildren<WanderSteeringBehavior>();
            
            // MovingAgent properties
            boidAgentColor.Color = boidColor;
            boidMover.MaximumSpeed = boidMaximumSpeed;
            boidMover.StopSpeed = boidStopSpeed;
            boidMover.MaximumRotationalSpeed = boidMaximumRotationalSpeed;
            boidMover.StopRotationThreshold = boidStopRotationThreshold;
            boidMover.MaximumAcceleration = boidMaximumAcceleration;
            boidMover.MaximumDeceleration = boidMaximumDeceleration;
            boidMover.AutoSmooth = boidAutoSmooth;
            boidMover.AutoSmoothSamples = boidAutoSmoothSamples;
            
            // SteeringBehavior properties
            separationBehavior.SeparationThreshold = boidSeparationThreshold;
            separationBehavior.SeparationAlgorithm = boidSeparationAlgorithm;
            separationBehavior.DecayCoefficient = boidSeparationDecayCoefficient;
            cohesionBehavior.ArrivalDistance = boidCohesionArrivalDistance;
            seekBehavior.Target = boidSeekTarget;
            seekBehavior.ArrivalDistance = swarmRadius;
            wanderBehavior.ArrivalDistance = boidArrivalDistance;
            wanderBehavior.WanderRadius = boidWanderRadius;
            wanderBehavior.WanderDistance = boidWanderDistance;
            wanderBehavior.WanderJitter = boidWanderJitter;
            wanderBehavior.WanderRecalculationTime = boidWanderRecalculation;
            
            
            // Register all the rest boids in every boid's separation and cohesion
            // behavior.
            List<GameObject> otherBoids = createdBoids
                .Where(b => b != boid)
                .ToList();
            List<AgentMover> otherAgentMovers = otherBoids
                .Select(b => b.GetComponent<AgentMover>())
                .ToList();
            separationBehavior.Threats.AddRange(otherAgentMovers.ToArray());
            cohesionBehavior.Targets.AddRange(otherBoids.ToArray());
        }
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!ShowGizmos) return;
    
        Gizmos.color = GizmosColor;
        Gizmos.DrawWireSphere(
            transform.position, 
            swarmRadius);
    }
#endif
}
}

