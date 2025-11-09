using System;
using System.Collections.Generic;
using UnityEngine;

namespace Groups
{
/// <summary>
/// <p>This class takes an IFormation object and generates ushers at every of its
/// positions and one agent following every usher.</p>
/// <p>The IFormation object that creates the ushers is a child of this object, so it,
/// and its ushers, will follow its movements. In contrast with that, generated agent
/// members won't be this object children, so they can move freely to follow, the best
/// they can, to this object usher children.</p>
/// </summary>
public class TwoLevelFormation : MonoBehaviour, IFormation
{
    public event EventHandler<FormationDimensionsChangedArgs> FormationDimensionsChanged;
    
    [Header("CONFIGURATION:")]
    [Tooltip("Prefab to instantiate for each member. ¡It must have an ITargeter " +
             "SteeringBehavior!")]
    [SerializeField] private GameObject memberPrefab;
    
    public List<GameObject> Members { get; } = new();

    public List<Vector2> MemberPositions => _usherFormation.MemberPositions;

    public float MemberRadius => _usherFormation.MemberRadius;

    private IFormation _usherFormation;
    private bool _membersGenerated;
    private bool _membersUpdated;

    private void Start()
    {
        // IFormation components are supposed to be a single tree. So we got a reference
        // to the first top-most one.
        _usherFormation = GetComponentInChildren<IFormation>();
        if (_usherFormation == null)
        {
            Debug.LogError("No IFormation component found in children.");
            return;       
        }
        
        GenerateMembers();
        AssignUshersToAgents();
    }

    /// <summary>
    /// Create this formation's members.
    /// <remarks> Generated member are not children of this object, so they can move
    /// freely to follow, the best they can, to this formation ushers.</remarks>   
    /// </summary>
    private void GenerateMembers()
    {
        // Let _usherFormation create Ushers. We are going to focus on creating agents.
        // I've set a Target prefab at _usherFormation._memberPrefab to let formation
        // positions be seen.
        Members.Clear();
        foreach (Vector2 positionOffset in MemberPositions)
        {
            GameObject member = Instantiate(
                memberPrefab, 
                transform.TransformPoint(positionOffset), 
                Quaternion.identity);
            Members.Add(member);
        }
    }

    private void OnEnable()
    {
        _usherFormation.FormationDimensionsChanged += OnFormationDimensionsChanged;
    }

    private void OnDisable()
    {
        _usherFormation.FormationDimensionsChanged -= OnFormationDimensionsChanged;
    }

    private void OnFormationDimensionsChanged(
        object sender, 
        FormationDimensionsChangedArgs e)
    {
        FormationDimensionsChanged?.Invoke(this, e);
    }
    
    /// <summary>
    /// Let every agent know which usher to follow.
    /// </summary>
    private void AssignUshersToAgents()
    {
        for (int i = 0; i < Members.Count; i++)
        {
            ITargeter targeter = Members[i].GetComponentInChildren<ITargeter>();
            targeter.Target = _usherFormation.Members[i];
        }
    }
}
}

