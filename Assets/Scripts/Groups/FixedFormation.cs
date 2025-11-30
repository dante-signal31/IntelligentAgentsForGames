using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Groups
{
/// <summary>
/// <p>Represents a formation in which all members are positioned according to a
/// predefined pattern without dynamic changes. The class manages the members of the
/// formation and ensures they follow a specified formation pattern.</p>
/// </summary>
public class FixedFormation : MonoBehaviour, IFormation
{
    public event EventHandler<FormationDimensionsChangedArgs> FormationDimensionsChanged;
    
    [Header("CONFIGURATION:")] 
    // memberPrefab should not have a rigidBody2D component. Unity does not apply the
    // movement of a parent's rigidbody to its children rigidbodies. If we want that 
    // memberPrefab instance follows the usher agent, then that instance should not
    // have a rigidBody2D component.
    [Tooltip("Prefab to instantiate for each member of the formation.")]
    [SerializeField] private GameObject memberPrefab;
    [Tooltip("Radius of the formation members.")]
    [SerializeField] private float memberRadius;
    
    [Header("WIRING:")]
    [Tooltip("Members positions for this formation.")]
    [FormerlySerializedAs("formationPattern")]
    [SerializeField] private GroupPattern groupPattern;

    private List<Vector2> memberPositions;

    public List<GameObject> Members { get; } = new();

    public List<Vector2> MemberPositions => new(GroupPattern.positions.Offsets);

    /// <summary>
    /// Gets or sets the formation pattern that specifies the positional arrangement of
    /// formation members. This value is critical for defining how members are spatially
    /// distributed within the formation, using predefined offsets.
    /// </summary>
    /// <remarks>
    /// Setting this property triggers an update to the formation by raising the
    /// <c>FormationDimensionsChanged</c> event and calling <c>UpdateFormation</c>.
    /// Changes to this property also influence the member positions based on the new
    /// pattern's offsets.
    /// </remarks>
    public GroupPattern GroupPattern
    {
        get => groupPattern;
        private set
        {
            groupPattern = value;
            if (value == null) return;
            var args = new FormationDimensionsChangedArgs(
                value.Positions.Offsets.ToArray(),
                MemberRadius);
            FormationDimensionsChanged?.Invoke(this, args);
            UpdateFormation();
        }
    }

    /// <summary>
    /// Updates the current formation by removing all existing members and re-generating
    /// them based on the specified formation pattern. This ensures that the member
    /// positions adhere to the defined offsets in the formation pattern. Clears the
    /// list of existing members and repopulates it with new ones.
    /// </summary>
    private void UpdateFormation()
    {
        foreach (GameObject member in Members)
        {
            Destroy(member);
        }
        Members.Clear();
        GenerateMembers();
    }

    public float MemberRadius
    {
        get => memberRadius;
        set => memberRadius = value;
    }

    private void Awake()
    {
        GenerateMembers();
    }

    /// <summary>
    /// Generates new member instances for the formation based on the defined offsets
    /// in the formation pattern. Each member is instantiated at a position calculated
    /// by applying the offset relative to the formation's transform. The generated
    /// members are parented to the formation's transform and added to the list of
    /// members to maintain the formation's structure.
    /// </summary>
    private void GenerateMembers()
    {
        foreach (Vector2 positionOffset in GroupPattern.positions.Offsets)
        {
            GameObject member = Instantiate(
                memberPrefab, 
                GroupPattern.transform.TransformPoint(positionOffset),
                Quaternion.identity);
            member.transform.parent = transform;
            Members.Add(member);
        }
    }
}
}

