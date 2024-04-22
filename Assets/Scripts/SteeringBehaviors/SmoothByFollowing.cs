using PropertyAttribute;
using UnityEngine;

/// <summary>
/// Obstacle avoider agents can show jittering rotation behavior when evading obstacles. Although
/// overall path is correct visually, it is not smooth. To fix that you can use the Ushier Method.
///
/// If you have seen a Victorian film or series you already know the Ushier Method. When a guest
/// arrives to a noble house (e.g. Downton Abbey), he is ushered by a servant through halls and
/// alleys to where the house master is. The servant acts as a guide and the guest just follows him.
/// The guide can doubt and jitter while selecting a path, but the guest can walk steadily because
/// only needs to follow the overall path of the servant. Make the servant invisible and you'll
/// only see a guest walking surely through the house.
/// 
/// So, this scripts smooths an agent movement making its current steering behavior (usually a simple
/// one) follow another object (usually a complex one). This way agents movement avoid usual
/// jittering in complex behaviors.
///
/// The key point here is that the following object should not reach the ushier or it will suffer
/// the same jittering behavior. Instead of that it it reaches Ushier it must let it go forward
/// before following again.
/// </summary>
public class SmoothByFollowing : MonoBehaviour, ITargeter
{
    [Header("WIRING:")]
    [Tooltip("Current steering behaviour of this agent (the guest).")]
    [HelpBar("The steering behavior you set here should comply with ITargeter interface.", MessageTypes.MessageType.Info)]
    [SerializeField] private SteeringBehavior steeringBehavior;
    [Tooltip("Instantiator of the object to follow (the Ushier).")]
    [SerializeField] private Instantiator instantiator;
    [Header("CONFIGURATION:")]
    [Tooltip("Name of the object to follow (the Ushier name).")]
    [SerializeField] private string followedObjectName;
    [Tooltip("Target for the followed object (the house master).")] 
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
