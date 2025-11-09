using System;
using System.Collections.Generic;
using UnityEngine;

namespace Groups
{
/// <summary>
/// Represents the interface for a formation structure that manages a group of members
/// and their relative positional arrangements within the formation.
/// </summary>
public interface IFormation
{
    /// <summary>
    /// Event raised when the dimensions of the formation change.
    /// </summary>
    // I've used a C# event instead of a UnityEvent because UnityEvents cannot be placed
    // in interfaces, only in classes.
    public event EventHandler<FormationDimensionsChangedArgs> FormationDimensionsChanged;
    
    /// <summary>
    /// List of formation members nodes.
    /// <remarks>
    /// <p>At game runtime, this list contains the agents in the formation.</p>
    /// <p>At editor time, this list is empty and has no use.</p>
    /// </remarks>   
    /// </summary>
    public List<GameObject> Members { get; }
    
    /// <summary>
    /// List of formation members local positions.
    /// <remarks> At game runtime, this field is redundant with Members one, because
    /// you can get the position of a member by calling its GetGlobalPosition() method.
    /// The point is that the Members field is only initialized when the game starts,
    /// while the MemberPositions field is populated even in the editor, so it can be
    /// used to draw editor gizmos. </remarks>
    /// </summary>
    public List<Vector2> MemberPositions { get; }
    
    /// <summary>
    /// Radius size for every formation member.
    /// </summary>
    public float MemberRadius { get; }
}
}