
using System.Collections.Generic;
using SteeringBehaviors;
using UnityEngine;
using UnityEngine.Serialization;

namespace Groups
{
/// <summary>
/// The class is used to define and manage defines the positions of the formation members
/// in relation with formation origin. 
/// </summary>
public class FormationPattern : MonoBehaviour, IGizmos
{
    [Header("CONFIGURATION:")] 
    [Tooltip("Formation members positions relative to formation origin.")]
    [SerializeField] public OffsetList positions;

    [Header("DEBUG:")]
    [SerializeField] private bool showGizmos;
    [SerializeField] private Color gizmosColor;
    [SerializeField] public float originGizmoRadius = 10;
    [SerializeField] public float positionGizmoRadius = 50;
    [SerializeField] public Vector2 gizmoTextOffset = new(10, 10);

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
        for (int i=0; i < positions.Offsets.Length; i++)
        {
            Gizmos.DrawWireSphere(
                transform.TransformPoint(positions.Offsets[i]), 
                positionGizmoRadius);
        }
    }
#endif
}
}

