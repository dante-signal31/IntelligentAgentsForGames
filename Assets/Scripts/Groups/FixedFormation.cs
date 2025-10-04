using System;
using System.Collections.Generic;
using SteeringBehaviors;
using UnityEngine;

namespace Groups
{
/// <summary>
/// <p>Represents a formation in which all members are positioned according to a
/// predefined pattern without dynamic changes. The class manages the members of the
/// formation and ensures they follow a specified formation pattern.</p>
/// </summary>
public class FixedFormation : MonoBehaviour
{
    [Header("CONFIGURATION:")] 
    // memberPrefab should not have a rigidBody2D component. Unity does not apply the
    // movement of a parent's rigidbody to its children rigidbodies. If we want that 
    // memeberPrefab instance follows the usher agent then that instance should not
    // have a rigidBody2D component.
    [Tooltip("Prefab to instantiate for each member of the formation.")]
    [SerializeField] private GameObject memberPrefab;

    [Header("WIRING:")]
    [Tooltip("Members positions for this formation.")]
    [SerializeField] private FormationPattern formationPattern;
    
    public List<GameObject> Members { get; private set; } = new();

    private void Start()
    {
        GenerateMembers();
    }
    
    private void GenerateMembers()
    {
        foreach (Vector2 positionOffset in formationPattern.positions.Offsets)
        {
            GameObject member = Instantiate(
                memberPrefab, 
                formationPattern.transform.TransformPoint(positionOffset),
                Quaternion.identity);
            member.transform.parent = transform;
            Members.Add(member);
        }
    }
}
}

