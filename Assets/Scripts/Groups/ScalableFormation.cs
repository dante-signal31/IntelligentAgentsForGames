
using System;
using System.Collections.Generic;
using System.Linq;
using PropertyAttribute;
using SteeringBehaviors;
using UnityEngine;

namespace Groups
{
/// <summary>
/// <p>In many situations, you will want to distribute a fixed number of agents evenly
/// across a given rectangular area. This is seen in games like Total War when you reshape
/// a rectangular formation dragging the mouse. In other situations, you will want your
/// formation shape to change, keeping its proportions, when its number of agents is
/// modified. Both cases are scalable formations.</p>
/// <p>This class covers both cases of scalable formations.</p>
/// <remarks>For simplicity, this class only covers rectangular formations.</remarks>
/// </summary>
[ExecuteAlways]
public class ScalableFormation : MonoBehaviour, IGizmos, IFormation
{
    /// <summary>
    /// Emitted when the formation dimensions are updated when quantity or density of
    /// formation members are changed.
    /// </summary>
    public event EventHandler<FormationDimensionsChangedArgs> FormationDimensionsChanged;
    
    
    /// <summary>
    /// <p>Defines how the formation is distributed.</p>
    /// <ul><b>QuantityAndDimensionsDefined</b>: Fixed number of agents that are
    /// distributed evenly across a given rectangular area.</ul>
    /// <ul><b>DensityAndQuantityDefined</b>: A fixed number of agents is distributed
    /// across an area, with a given density, and keeping formation former proportions.
    /// </ul>
    /// <ul><b>DensityAndDimensionsDefined</b>: With a given density, the formation is
    /// distributed across a given rectangular area.</ul> 
    /// </summary>
    private enum DistributionType
    {
        QuantityAndDimensionsDefined,
        DensityAndQuantityDefined,
        DensityAndDimensionsDefined
    }
    
    [Header("CONFIGURATION:")]
    [Tooltip("Formation member prefab to instance.")]
    [SerializeField] private GameObject memberPrefab;
    [Tooltip("How the formation is distributed.")]
    [SerializeField] private DistributionType distributionType;
    [Tooltip("Formation member radius.")]
    [SerializeField] private float memberRadius;
    [Tooltip("the minimum allowable distance between agents in the formation to ensure " +
             "proper spacing and avoid overlap.")]
    [SerializeField] private Vector2 minimumDistanceBetweenAgents;
    [HelpBar("This field means different depending on the distribution type. " +
             "For QuantityDefined and DensityDefined it is the given area to cover " +
             "with this formation. For DensityAndQuantityDefined it is the formation " +
             "proportions to keep.", MessageTypes.MessageType.Info)]
    [SerializeField] private Vector2 formationDimensions;
    [Tooltip("Agent density in the formation. Actually, it is the current separation " +
             "between agents, horizontally and vertically.")]
    [SerializeField] private Vector2 density;
    [Tooltip("Total number of agents in the formation.")]
    [SerializeField] private int quantity;
    
    [Header("DEBUG:")]
    [SerializeField] private bool showGizmos;
    [SerializeField] private Color gizmosColor;
    [SerializeField] private float originGizmoRadius = 0.1f;
    
    [Header("WIRING:")]
    [SerializeField] private HingeJoint2D rightHinge;
    [SerializeField] private HingeJoint2D leftHinge;
    
    /// <summary>
    /// Specifies the distribution configuration type for the scalable formation.
    /// </summary>
    private DistributionType Distribution
    {
        get => distributionType;
        set
        {
            distributionType = value;
            if (distributionType == DistributionType.DensityAndDimensionsDefined)
            {
                _formationFormerDimensions = FormationDimensions;
            }
            UpdateFormation();
        }
    }
    
    /// <summary>
    /// Formation member radius.
    /// </summary>
    public float MemberRadius
    {
        get => memberRadius;
        private set
        {
            memberRadius = value;
            UpdateFormation();
        }
    }
    
    /// <summary>
    /// <p>This field means different depending on the distribution type.</p>
    /// <p>For <b>QuantityDefined</b> and <b>DensityDefined</b> it is the given area to
    /// cover with this formation.</p>
    /// <p>For <b>DensityAndQuantityDefined</b> it is the formation proportions to
    /// keep.</p> 
    /// </summary>
    private Vector2 FormationDimensions
    {
        get => formationDimensions;
        set
        {
            formationDimensions = value;
            
            // This void is needed to avoid infinite calls when the dimensions are
            // corrected from CalculateMembersPositions.
            if (_correctingDimensions)
            {
                _correctingDimensions = false;
                return;
            }
            
            UpdateFormation();
        }
    }
    
    /// <summary>
    /// <p>Agent density in the formation. Actually, it is the current separation between
    /// agents, horizontally and vertically.</p>
    /// <p> Only used for <b>DensityAndQuantityDefined</b> and
    /// <b>DensityAndDimensionsDefined</b> distribution types. </p>
    /// </summary>
    private Vector2 Density
    {
        get => density;
        set
        {
            density = value;
            
            // This void is needed to avoid infinite calls when the density is corrected
            // from CalculateMembersPositions.
            if (_correctingDensity)
            {
                _correctingDensity = false;
                return;
            }
            
            UpdateFormation();
        }
    }
    
    /// <summary>
    /// Total number of agents in the formation.
    /// <p> Only used for <b>QuantityAndDimensionsDefined</b> and
    /// <b>DensityAndQuantityDefined</b> distribution types. </p>
    /// </summary>
    private int Quantity
    {
        get => quantity;
        set
        {
            quantity = value;
            
            // This void is needed to avoid infinite calls when the quantity is corrected
            // from CalculateMembersPositions.
            if (_correctingQuantity)
            {
                _correctingQuantity = false;
                return;
            }
            
            UpdateFormation();
        }
    }
    
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

    private bool _correctingQuantity;
    private bool _correctingDimensions;
    private bool _correctingDensity;
    private Vector2 _formationFormerDimensions;
    private AgentMover _parentAgent;
    
    public List<GameObject> Members { get; private set; } = new();
    public List<Vector2> MemberPositions { get; private set; } = new();

    private void Awake()
    {
        _parentAgent = GetComponentInParent<AgentMover>();
    }

    private void Start()
    {
        AlignWithAgentForward();
        UpdateFormation();
        if (Application.isEditor) return;
        GenerateMembers();
    }
    
    /// <summary>
    /// Aligns the formation's rotation to match the forward direction of its parent
    /// agent.
    /// </summary>
    private void AlignWithAgentForward()
    {
        if (_parentAgent == null) return;
        
        float angle = Vector2.SignedAngle(transform.up, _parentAgent.Forward);
        transform.Rotate(Vector3.forward, angle);
    }
    
    /// <summary>
    /// Repositions the formation so that its central axis is aligned with the midpoint
    /// of its horizontal dimension. This helps ensure that the formation is correctly
    /// centered relative to its agent origin.
    /// </summary>
    private void RecenterFormation()
    {
        Vector2 newLocalPosition = transform.localPosition;
        newLocalPosition.x = -FormationDimensions.x / 2;
        transform.localPosition = newLocalPosition;
    }
    
    /// <summary>
    /// Place hinges at the front corners members.
    /// </summary>
    private void UpdateHingesPosition()
    {
        if (leftHinge == null || rightHinge == null) return;
        
        // Place hinges at the front corners members.
        // The left corner member is always the first one.
        leftHinge.transform.localPosition = MemberPositions[0];
        rightHinge.transform.localPosition = GetRightFrontCornerMemberPosition();
    }
    
    private Vector2 GetRightFrontCornerMemberPosition()
    {
        // The right font corner member is the one with the highest X but lowest Y.
        return MemberPositions
            .OrderByDescending(position => position.x)
            .ThenBy(position => position.y)
            .First();
    }
    
    /// <summary>
    /// Refresh formation members positions.
    /// </summary>
    private void UpdateFormation()
    {
        CalculateMembersPositions();
        RecenterFormation();
        UpdateHingesPosition();
        FormationDimensionsChanged?.Invoke(
            this, 
            new FormationDimensionsChangedArgs(
                MemberPositions.ToArray(), 
                MemberRadius));
    }
    
    /// <summary>
    /// Distribute members evenly across the formation area, depending on the
    /// selected distribution type.
    /// </summary>
    private void CalculateMembersPositions()
    {
        switch (Distribution)
        {
            case DistributionType.QuantityAndDimensionsDefined:
                CalculateQuantityAndDimensionsDefinedFormation();
                break;
            case DistributionType.DensityAndQuantityDefined:
                CalculateDensityAndQuantityDefinedFormation();
                break;
            case DistributionType.DensityAndDimensionsDefined:
                CalculateDensityAndDimensionsDefinedFormation();
                break;
        }
    }
    
    /// <summary>
    /// With quantity and dimensions already defined, what we must calculate is the
    /// resulting formation density.
    /// </summary>
    private void CalculateQuantityAndDimensionsDefinedFormation()
    {
        // Get grid distribution with current quantity and proportions.
        (int columnsAmount, int rowsAmount) = GetGrid();

        // In this kind of formation, the user defines the dimensions unless
        // member separation is under MinimumDistanceBetweenAgents. In that case, 
        // MinimumDistanceBetweenAgents prevails and Quantity is corrected.
        float columnsSeparation = (FormationDimensions.x - 2 * MemberRadius) /
            (columnsAmount - 1);
        float rowsSeparation = (FormationDimensions.y - 2 * MemberRadius) /
            (rowsAmount - 1);
        if ((columnsSeparation < minimumDistanceBetweenAgents.x) ||
            (rowsSeparation < minimumDistanceBetweenAgents.y))
        {
            // In case of conflict with MinimumDistanceBetweenAgents, we correct the
            // quantity to place the maximum number of agents possible (without
            // going under MinimumDistanceBetweenAgents) in the current formation area.
            columnsSeparation = columnsSeparation < minimumDistanceBetweenAgents.x ?
                minimumDistanceBetweenAgents.x : columnsSeparation;
            rowsSeparation = rowsSeparation < minimumDistanceBetweenAgents.y ?
                minimumDistanceBetweenAgents.y : rowsSeparation;
            columnsAmount = Mathf.FloorToInt(FormationDimensions.x / columnsSeparation);
            rowsAmount = Mathf.FloorToInt(FormationDimensions.y / rowsSeparation);
            _correctingQuantity = true;
            Quantity = Mathf.RoundToInt(columnsAmount * rowsAmount);
        }
        
        // Update formation density.
        _correctingDensity = true;
        Density = new Vector2(columnsSeparation, rowsSeparation);

        // Once the formation grid is defined, use it to calculate members' local
        // positions.
        MemberPositions.Clear();
        for (int i = 0; i < Quantity; i++)
        {
            int column = i % columnsAmount;
            int row = i / columnsAmount;
            float x = MemberRadius + column * (columnsSeparation);
            float y = MemberRadius + row * (rowsSeparation);
            MemberPositions.Add(new Vector2(x, y));
        }
    }
    
    /// <summary>
    /// Calculates the number of columns and rows required to evenly distribute agents
    /// within the specified formation dimensions while maintaining proportionality
    /// between the formation's width and length. Ensures the formation accommodates
    /// the given agent quantity.
    /// </summary>
    /// <returns>
    /// A tuple containing the computed number of columns and rows in the grid.
    /// </returns>
    private (int columnsAmount, int rowsAmount) GetGrid(Vector2 formationRelation = new())
    {
        if (formationRelation == Vector2.zero) formationRelation = FormationDimensions;
        
        // Our formation has two dimensions W (width, FormationsDimensions.X) and L
        // (length, FormationDimensions.Y). Their relation is W/L. We want to 
        // distribute agents evenly across the area, keeping the formation proportions.
        // That distribution will have a number of C columns and R rows. As they will
        // keep proportion, C/R will be approximately the same as W/L.
        // Along with that, our total agents quantity (N) will be approximately the same
        // as C*R. Or, what is the same, R is approximately N/C.
        // Substitute that value for R in the equivalence above between C/R and W/L, and 
        // you will get that C is approximately the square root of N*W/L
        int columnsAmount = Mathf.RoundToInt(
            Mathf.Sqrt(Quantity * formationRelation.x / formationRelation.y));
        int rowsAmount = Mathf.CeilToInt(Quantity / (float)columnsAmount);
        return (columnsAmount, rowsAmount);
    }
    
    /// <summary>
    /// With density and quantity already defined, what we must calculate is the
    /// resulting formation dimensions.
    /// </summary>
    private void CalculateDensityAndQuantityDefinedFormation()
    {
        // Get grid distribution with current quantity and proportions.
        (int columnsAmount, int rowsAmount) = GetGrid(_formationFormerDimensions);

        // Once the formation grid is defined, use it to calculate members' local
        // positions.
        MemberPositions.Clear();
        for (int i = 0; i < Quantity; i++)
        {
            int column = i % columnsAmount;
            int row = i / columnsAmount;
            float x = MemberRadius + column * (Density.x);
            float y = MemberRadius + row * (Density.y);
            MemberPositions.Add(new Vector2(x, y));
        }
        
        // Update formation dimensions.
        _correctingDimensions = true;
        FormationDimensions = new Vector2(
            columnsAmount * Density.x + 2 * MemberRadius,
            rowsAmount * Density.y + 2 * MemberRadius);
    }
    
    /// <summary>
    /// With density and dimensions already defined, what we must calculate is the
    /// resulting quantity formation members.
    /// </summary>
    private void CalculateDensityAndDimensionsDefinedFormation()
    {
        int columnsAmount = Mathf.FloorToInt(FormationDimensions.x / Density.x);
        int rowsAmount = Mathf.FloorToInt(FormationDimensions.y / Density.y);
        _correctingQuantity = true;
        Quantity = Mathf.RoundToInt(columnsAmount * rowsAmount);
        
        // Once the formation grid is defined, use it to calculate members' local
        // positions.
        MemberPositions.Clear();
        for (int i = 0; i < Quantity; i++)
        {
            int column = i % columnsAmount;
            int row = i / columnsAmount;
            float x = MemberRadius + column * (Density.x);
            float y = MemberRadius + row * (Density.y);
            MemberPositions.Add(new Vector2(x, y));
        }
    }
    
    /// <summary>
    /// Instantiates and places members of the formation based on predefined positions.
    /// This method iterates through all calculated position offsets and creates
    /// individual formation members at those positions.
    /// </summary>
    private void GenerateMembers()
    {
        foreach (Vector2 positionOffset in MemberPositions)
        {
            GenerateMember(positionOffset);
        }
    }
    
    /// <summary>
    /// Creates and adds a new member to the formation at the specified local position.
    /// The instantiated member's process mode is disabled to prevent unwanted behavior,
    /// such as automatic movement, making it appropriate for static formation positioning.
    /// </summary>
    /// <param name="positionOffset">The local position at which the member is placed
    /// within the formation.</param>
    private void GenerateMember(Vector2 positionOffset)
    {
        GameObject member = Instantiate(
            memberPrefab, 
            positionOffset,
            Quaternion.identity);
        member.transform.parent = transform;
        Members.Add(member);
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // Mark formation origin.
        Gizmos.color = gizmosColor;
        Gizmos.DrawSphere(transform.position, originGizmoRadius);
        Gizmos.DrawLine(transform.position, transform.position + transform.up * 0.5f);
        
        if (MemberPositions == null) return;
        
        // Draw formation positions.
        for (int i=0; i < MemberPositions.Count; i++)
        {
            Gizmos.DrawWireSphere(
                transform.TransformPoint(MemberPositions[i]), 
               MemberRadius);
        }
    }
    
    private void OnValidate()
    {
        // Auto-assign to properties to run initialization code.
        Distribution= distributionType;
        MemberRadius = memberRadius;
        FormationDimensions = formationDimensions;
        Density = density;
        Quantity = quantity;
    }
#endif
}
}

