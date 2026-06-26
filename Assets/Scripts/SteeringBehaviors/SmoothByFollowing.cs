using PropertyAttribute;
using UnityEngine;

namespace SteeringBehaviors
{
/// <summary>
/// Obstacle avoider agents can show jittering rotation behavior when evading obstacles.
/// Although the overall path is correct visually, it is not smooth. To fix that, you can
/// use the Usher Method.
///
/// If you have seen a Victorian film or series, you already know the Usher Method. When
/// a guest arrives at a noble house (e.g., Downton Abbey), a servant ushers him through
/// halls and alleys to where the housemaster is. The servant acts as a guide and the
/// guest just follows him. The guide can doubt and jitter while selecting a path, but
/// the guest can walk steadily because
/// only needs to follow the overall path of the servant. Make the servant invisible, and
/// you'll only see a guest walking surely through the house.
/// 
/// So, this script smooths an agent movement making its current steering behavior
/// (usually a simple one) follow another object (usually a complex one). This way
/// agents' movement avoids the usual jittering in complex behaviors.
///
/// The key point here is that the following object should not reach the usher, or it
/// will suffer the same jittering behavior. Instead of that it reaches Usher, it must
/// let it go forward before following again.
/// </summary>
public class SmoothByFollowing : MonoBehaviour, ITargeter
{
    [Header("WIRING:")]
    [Tooltip("Current steering behaviour of this agent (the guest).")]
    [HelpBar("The steering behavior you set here should comply with ITargeter " +
             "interface.", MessageTypes.MessageType.Info)]
    [SerializeField] private SteeringBehavior steeringBehavior;
    [Tooltip("Instantiator of the object to follow (the usher).")]
    [SerializeField] private Instantiator instantiator;
    [Header("CONFIGURATION:")]
    [Tooltip("Name of the object to follow (the usher name).")]
    [SerializeField] private string followedObjectName;
    [Tooltip("Target for the followed object (the housemaster).")] 
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
        FollowTheFollowedObject(followedObject);
        GiveATargetToFollowedObject(followedObject);
    }

    private void MakeFollowedObjectInvisible(GameObject followedObject)
    {
        SpriteRenderer[] spriteRenderers = 
            followedObject.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            spriteRenderer.enabled = false;
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
            Debug.LogError("[SmoothByFollowing] Targeter interface not found on " +
                           "steering behavior.");
        }
    }

    private void GiveATargetToFollowedObject(GameObject followedObject)
    {
        SeekSteeringBehavior followedObjectSeek = 
            followedObject.GetComponent<SeekSteeringBehavior>();
        if (followedObjectSeek) followedObjectSeek.Target = target;
    }
}
}
