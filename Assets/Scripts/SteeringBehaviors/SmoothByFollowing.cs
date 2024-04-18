using UnityEngine;

/// <summary>
/// Smooths an agent movement making its current steering behavior (usually a simple one)
/// follow another object (usually a complex one). This way agents movement avoid usual
/// jittering in complex behaviors.
/// </summary>
public class SmoothByFollowing : MonoBehaviour, ITargeter
{
    [Header("WIRING:")]
    [Tooltip("Current steering behaviour of this agent.")]
    [SerializeField] private SteeringBehavior steeringBehavior;
    [Tooltip("Instatiator of the object to follow.")]
    [SerializeField] private Instantiator instantiator;
    [Header("CONFIGURATION:")]
    [Tooltip("Name of the object to follow.")]
    [SerializeField] private string followedObjectName;
    [Tooltip("Target for the followed object.")] 
    [SerializeField] private GameObject target;

    public GameObject Target
    {
        get => target;
        set => target = value;
    }
    
    private void Start()
    {
        // Instantiator creates object in Awake() phase so we must call them from start.
        GameObject followedObject = instantiator.InstantiatedObjects[followedObjectName];
        MakeFollowedObjectInvisible(followedObject);
        //MakeFollowedObjectNotCollidable(followedObject);
        FollowTheFollowedObject(followedObject);
        GiveATargetToFollowedObject(followedObject);
    }

    private void MakeFollowedObjectInvisible(GameObject followedObject)
    {
        SpriteRenderer[] spriteRenderers = followedObject.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            spriteRenderer.enabled = false;
        }
    }

    private void MakeFollowedObjectNotCollidable(GameObject followedObject)
    {
        Collider2D[] colliders = followedObject.GetComponentsInChildren<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }
    }

    private void FollowTheFollowedObject(GameObject followedObject)
    {
        if (steeringBehavior is ITargeter currentSteeringBehavior)
        {
            currentSteeringBehavior.Target = followedObject;
        }
        else
        {
            Debug.LogError("[SmoothByFollowing] Targeter interface not found on steering behavior.");
        }
    }
    
    private void GiveATargetToFollowedObject(GameObject followedObject)
    {
        SeekSteeringBehavior followedObjectSeek = followedObject.GetComponent<SeekSteeringBehavior>();
        if (followedObjectSeek) followedObjectSeek.Target = target;
    }
}
