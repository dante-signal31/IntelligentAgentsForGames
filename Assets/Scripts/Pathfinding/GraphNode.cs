using System;
using System.Collections.Generic;
using Tools;
using UnityEngine;
using Random = System.Random;

namespace Pathfinding
{
/// <summary>
/// Represents a node in a graph data structure, uniquely identified by an ID
/// and capable of maintaining connections to other nodes.
/// </summary>
/// <remarks>
/// This class is core to a graph representation and is designed to store
/// connections (or edges) to other nodes in the graph. Each connection is
/// directional and associated with a specific orientation and cost.
/// </remarks>
[Serializable]
public class GraphNode
{
    private static readonly HashSet<uint> assignedIds = new();
    private static readonly Random random = new();

    [SerializeField] private uint id = GenerateUniqueId();

    /// <summary>
    /// This node unique identifier.
    /// </summary>
    public uint Id => id;
    
    // Don't make this field readonly, or it will break serialization.
    public CustomUnityDictionaries.OrientationGraphConnectionDictionary connections = 
        new ();

    /// <summary>
    /// Generates a unique identifier for a node by creating a random 32-bit unsigned
    /// integer that is not currently assigned to any other node. Ensures identifier
    /// uniqueness by checking against a set of already assigned IDs.
    /// </summary>
    /// <returns>A 32-bit unsigned integer representing a unique identifier for the
    /// node.</returns>
    private static uint GenerateUniqueId()
    {
        uint newId;
        byte[] buffer = new byte[4];
        do
        {
            random.NextBytes(buffer);
            newId = BitConverter.ToUInt32(buffer, 0);
        } while (assignedIds.Contains(newId));

        assignedIds.Add(newId);
        return newId;
    }
    
    /// <summary>
    /// Adds a connection between the current node and a specified destination node
    /// with a given cost, orientation, and optionally bidirectional behavior.
    /// </summary>
    /// <param name="endNodeKey">The destination node id to which the connection should
    /// point.</param>
    /// <param name="cost">The cost or weight associated with traversing the edge.</param>
    /// <param name="orientation">The orientation of the edge connecting the two
    /// nodes.</param>
    public void AddConnection(
        uint endNodeKey,
        float cost, 
        Orientation orientation)
    {
        GraphConnection graphConnection = new(Id, endNodeKey, cost);
        connections[orientation] = graphConnection;
    }
}    
}
