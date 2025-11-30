using PropertyAttribute;
using SteeringBehaviors;
using UnityEngine;

namespace Groups
{
/// <summary>
/// The class is used to define and manage the positions of the group members
/// in relation to the group origin. 
/// </summary>
public class GroupPattern : MonoBehaviour, IGizmos
{
    [Header("GROUP PATTERN CONFIGURATION:")] 
    [Tooltip("Group members positions relative to group origin.")]
    [HelpBar("Every change here will be kept at the scriptable object asset, " +
             "so it will be replicated to every asset that makes use of this " +
             "scriptable object.", MessageTypes.MessageType.Info)]
    [SerializeField] public OffsetList positions;

    [Header("DEBUG:")]
    [SerializeField] private bool showGizmos;
    [SerializeField] private Color gizmosColor;
    [SerializeField] public float originGizmoRadius = 0.1f;
    [SerializeField] public float positionGizmoRadius = 0.5f;
    [SerializeField] public Vector2 gizmoTextOffset = new(0.1f, 0.1f);

    public OffsetList Positions => positions;
    
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
    
    /// <summary>
    /// Where to place strings with the number of positions.
    /// </summary>
    public Vector2 GizmoTextPosition => 
        new Vector2(positionGizmoRadius, positionGizmoRadius) + gizmoTextOffset;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // Draw formation pattern origin.
        Gizmos.color = gizmosColor;
        Gizmos.DrawSphere(transform.position, originGizmoRadius);
        
        if (positions == null) return;
        
        // Draw formation pattern positions.
        foreach (var offset in positions.Offsets)
        {
            Gizmos.DrawWireSphere(
                transform.TransformPoint(offset), 
                positionGizmoRadius);
        }
    }
#endif
}
}

