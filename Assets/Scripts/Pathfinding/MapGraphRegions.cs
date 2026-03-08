using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
/// <summary>
/// This class divides a MapGraph in regions, based on defined seeds.
/// </summary>
public class MapGraphRegions: MonoBehaviour

{
    /// <summary>
    /// This collection manages nodes to be explored in priority order
    /// based on their accumulated path cost, ensuring that the lowest-cost nodes
    /// are processed first.
    /// </summary>
    private class NodeRegionsRecordSet :
        HeuristicGraphPathFinder<RegionNodeRecord>.PrioritizedNodeRecordSet
    {
        // Comparer to keep the SortedSet ordered by TotalEstimatedCostToTarget
        private class NodeRecordComparer : IComparer<RegionNodeRecord>
        {
            public int Compare(RegionNodeRecord x, RegionNodeRecord y)
            {
                int result = x.costSoFar.CompareTo(y.costSoFar);
                // If costs are equal, we must not return 0, otherwise SortedSet 
                // thinks they are the same element and won't add the new one.
                if (result == 0 && x.node != y.node)
                    return x.regionId.CompareTo(y.regionId);
                return result;
            }
        }

        public NodeRegionsRecordSet() : base(new NodeRecordComparer())
        {
        }
    }

    [Header("CONFIGURATION:")]
    [Tooltip("Take connection cost into account when calculating the regions.")]
    public bool costAware = true;

    [Tooltip("Default cost for a region.")]
    public float defaultCost = 100;

    [Tooltip("List of seeds to create regions.")]
    public List<RegionSeed> seeds = new();

    [Header("WIRING:")]
    [Tooltip("MapGraph this component is going to divide into regions.")]
    public MapGraph mapGraph;

    [Tooltip("MapGraphRegions serialized backend.")] 
    [SerializeField] private MapGraphRegionsResource graphRegionsResource = new();

    [Header("DEBUG:")] public bool showGizmos;
    public Color gizmosColor = Color.blanchedAlmond;
    public float seedRadius = 10;
    [Range(0.0f, 1.0f)] public float gizmoAlpha = 0.5f;

    public MapGraphRegionsResource GraphRegionsResource => graphRegionsResource;

    private uint _seedsCount;
    private readonly NodeRegionsRecordSet _nodeRegionsOpenSet = new();

    /// <summary>
    /// Map of graph nodes to their corresponding region.
    /// </summary>
    private readonly Dictionary<PositionNode, RegionNodeRecord> _exploredNodes = new();

    /// <summary>
    /// A dictionary that maps region IDs to their corresponding influence values.
    /// </summary>
    private readonly Dictionary<uint, float> _regionsInfluence = new();

    /// <summary>
    /// Colors to show the regions in debugging mode.
    /// </summary>
    public readonly Dictionary<uint, Color> regionColors = new();

    /// <summary>
    /// IDs of regions present in the map graph.
    /// </summary>
    public HashSet<uint> Regions { get; } = new();

    /// <summary>
    /// IDs of nodes present in each region.
    /// </summary>
    public readonly System.Collections.Generic.Dictionary<uint, HashSet<uint>>
        NodesByRegion
            = new();

    public uint GetRegionByPosition(Vector2 position)
    {
        uint nearestNodeId = mapGraph.GetNodeAtPosition(position).Id;
        return graphRegionsResource.nodesIdToRegionsId[nearestNodeId];
    }

    public uint GetRegionByNodeId(uint nodeId) =>
        graphRegionsResource.nodesIdToRegionsId[nodeId];

    public Vector2 GetRegionCenter(uint regionId)
    {
        return seeds[(int)regionId].position;
    }

    public void Ready()
    {
        UpdateRegionsColors();
        UpdateRegionsArray();
        UpdateNodesByRegion();
    }
    
    /// <summary>
    /// Generates and assigns regions within the map graph. Each region is defined by its
    /// influence and cost parameters.
    /// </summary>
    public void GenerateRegions()
    {
        InitCollections();
        
        while (_nodeRegionsOpenSet.Count > 0)
        {
            RegionNodeRecord current = _nodeRegionsOpenSet.Get();
            if (current == null) break;

            foreach (GraphConnection graphConnection in current.node.Connections.Values)
            {
                float connectionCost = current.costSoFar + (costAware?
                    // The more region influence, the lower the spreading cost.
                    graphConnection.cost / _regionsInfluence[current.regionId] : 
                    defaultCost / _regionsInfluence[current.regionId]);
                
                // Where does that connection lead us?
                PositionNode endNode = 
                    (PositionNode) mapGraph.GetNodeById(graphConnection.endNodeId);
                
                // Is that node already explored?
                if (_exploredNodes.TryGetValue(
                        endNode, 
                        out RegionNodeRecord endNodeRecord))
                {
                    // If the node was already explored, but with a lower cost, skip it.
                    if (connectionCost >= endNodeRecord.costSoFar) continue;
                    // Otherwise, update the record with the lower cost, the
                    // connection and the new cost.
                    endNodeRecord.regionId = current.regionId;
                    endNodeRecord.connection = graphConnection;
                    endNodeRecord.costSoFar = connectionCost;
                    _nodeRegionsOpenSet.Add(endNodeRecord);
                }
                else
                {
                    // If the node was not explored yet, add it to the open set.
                    RegionNodeRecord newRecord = new()
                    {
                        node = endNode,
                        connection = graphConnection,
                        costSoFar = connectionCost,
                        regionId = current.regionId
                    };
                    _nodeRegionsOpenSet.Add(newRecord);
                    _exploredNodes[endNode] = newRecord;
                }
            }
        }
        UpdateRegionsResource(_exploredNodes);
        UpdateRegionsArray();
        UpdateNodesByRegion();
    }
    
    /// <summary>
    /// Updates the mapping of nodes to their respective region IDs in the graph regions
    /// resource. This way, the generated regions are kept serialized across executions.
    /// </summary>
    private void UpdateRegionsResource(
        Dictionary<PositionNode, RegionNodeRecord> exploredNodes)
    {
        graphRegionsResource.nodesIdToRegionsId.Clear();
        foreach (KeyValuePair<PositionNode, RegionNodeRecord> exploredNode in
                 exploredNodes)
        {
            graphRegionsResource.nodesIdToRegionsId[exploredNode.Key.Id] =
                exploredNode.Value.regionId;
        }
    }
    
    /// <summary>
    /// Updates the set of region IDs by synchronizing the current collection of regions
    /// with the mappings stored in the associated MapGraphRegionsResource object.
    /// This ensures that the Regions property reflects the latest region assignments.
    /// </summary>
    private void UpdateRegionsArray()
    {
        Regions.Clear();
        Regions.UnionWith(graphRegionsResource.nodesIdToRegionsId.Values);
    }
    
    /// <summary>
    /// Updates the mapping between region IDs and the sets of node IDs that belong to
    /// those regions.
    /// </summary>
    private void UpdateNodesByRegion()
    {
        foreach (uint regionId in Regions)
        {
            HashSet<uint> nodesInRegion = new();
            foreach (KeyValuePair<uint, uint> nodeIdTopRegionId in 
                     graphRegionsResource.nodesIdToRegionsId)
            {
                if (nodeIdTopRegionId.Value == regionId) 
                    nodesInRegion.Add(nodeIdTopRegionId.Key);
            }
            NodesByRegion[regionId] = nodesInRegion;
        }
    }
    
    /// <summary>
    /// Initializes and clears the collections used for region generation in the map
    /// graph.
    /// </summary>
    private void InitCollections()
    {
        _nodeRegionsOpenSet.Clear();
        _regionsInfluence.Clear();
        _exploredNodes.Clear();
        for (uint i = 0; i < seeds.Count; i++)
        {
            RegionSeed regionSeed = seeds[(int)i];
            _regionsInfluence[i] = regionSeed.influence;
            PositionNode seedNode = 
                (PositionNode) mapGraph.GetNodeAtPosition(regionSeed.position);
            RegionNodeRecord nodeRecord = new RegionNodeRecord()
            {
                node = seedNode,
                connection = null,
                costSoFar = 0,
                regionId = i
            };
            _nodeRegionsOpenSet.Add(nodeRecord);
            // Take seed nodes as already explored.
            _exploredNodes[seedNode] = nodeRecord;
        }
    }
    
    /// <summary>
    /// Updates the colors of the defined regions in the map based on the
    /// <see cref="RegionSeed.GizmoColor"/> property of each seed.
    /// </summary>
    private void UpdateRegionsColors()
    {
        regionColors.Clear();
        for (uint i = 0; i < seeds.Count; i++)
        {
            RegionSeed regionSeed = seeds[(int)i];
            regionColors[i] = regionSeed.gizmoColor;
        }
    }

    private void Update()
    {
        // Any update to the seeds array elements?
        if (seeds.Count != _seedsCount)
        {
            UpdateRegionsColors();
            _seedsCount = (uint)seeds.Count;
        }
    }
}
}