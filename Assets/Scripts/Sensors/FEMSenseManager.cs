using System.Collections.Generic;
using Pathfinding;
using Tools;
using UnityEngine;

namespace Sensors
{
/// <summary>
/// The FEMSenseManager class is responsible for managing sensors and propagating signals
/// across a graph-based structure. While RegionSenseManager deals with signals that
/// disappear once delivered, FEMSenseManager can deal with smell-like signals that
/// linger in a place dissipating over time until its disappearance.
/// </summary>
public class FEMSenseManager: RegionSenseManager
{
    [Header("FEM SENSE MANAGER CONFIGURATION:")]
    [Tooltip("Dissipation factor. On every dissipation update, every cell intensity will " +
             "be multiplied by this factor to get the new value.")]
    [Range(0.0f, 1.0f)]
    [SerializeField] public float dissipationFactor = 0.9f;
    [Tooltip("How many seconds between each dissipation update.")]
    [SerializeField] public float dissipationUpdatePeriod = 1.0f;
    [Tooltip("Under this intensity, a node will be considered as low that does not " +
             "disseminate anymore.")]
    [SerializeField] public float minimumDisseminationIntensity = 1.0f;
    
    [Header("DEBUG:")]
    [Tooltip("Whether to show the dissipation heat map.")]
    [SerializeField] public bool showGizmos;
    [Tooltip("Color for the dissipation heat map.")]
    [SerializeField] public Color dissipationColor = Color.red;
    [Tooltip("Transparency for the dissipation heat map.")]
    [Range(0.0f, 1.0f)]
    [SerializeField] public float gizmoAlpha = 0.5f;
    
    [Header("WIRING:")]
    [Tooltip("Map graph used by this manager to know walkable areas to spread smell-like" +
             "signals.")]
    [SerializeField] private MapGraph mapGraph;
    [Tooltip("Timer that controls the dissipation update.")]
    [SerializeField] private CustomTimer dissipationUpdateTimer;

    /// <summary>
    /// Map graph used by this manager.
    /// </summary>
    public MapGraph MapGraph => mapGraph;
    
    /// <summary>
    /// Dictionary of node intensities.
    /// </summary>
    public Dictionary<uint, float> NodeIntensities => _nodeIntensities;

    private readonly Dictionary<uint, float> _dissipationIntensities = new();
    private readonly Dictionary<uint, float> _nodeIntensities = new();
    private readonly Dictionary<uint, List<IRegionSenseSensor>> _registeredNodeSensors = 
        new();
    private readonly Queue<PositionNode> _openNodes = new();
    private readonly HashSet<uint> _closedNodes = new();
    private uint _frameCounter;
    
    protected void Awake()
    {
        ConfigureDissipationTimer();
    }

    private void ConfigureDissipationTimer()
    {
        dissipationUpdateTimer.waitTime = dissipationUpdatePeriod;
        dissipationUpdateTimer.oneShot = false;
        dissipationUpdateTimer.autoStart = true;
        dissipationUpdateTimer.timeout.AddListener(OnDissipationUpdate);
    }

    private void OnDissipationUpdate()
    {
        _dissipationIntensities.Clear();
        // Apply dissipation in any node that already has an intensity.
        foreach (KeyValuePair<uint, float> nodeIntensity in _nodeIntensities)
        {
            float newIntensity = Mathf.Max(
                0, 
                nodeIntensity.Value * Mathf.Pow(
                    dissipationFactor, 
                    dissipationUpdatePeriod));
            _dissipationIntensities[nodeIntensity.Key] = newIntensity;
        }
    }
    
    public override void RegisterSensor(IRegionSenseSensor sensor)
    {
        PositionNode node = mapGraph.GetNodeAtNearestPosition(sensor.Position);
        if (!_registeredNodeSensors.ContainsKey(node.Id))
            _registeredNodeSensors.Add(node.Id, new List<IRegionSenseSensor>());
        _registeredNodeSensors[node.Id].Add(sensor);
    }
    
    public override void UnregisterSensor(IRegionSenseSensor sensor)
    {
        PositionNode node = mapGraph.GetNodeAtNearestPosition(sensor.Position);
        _registeredNodeSensors[node.Id].Remove(sensor);
    }
    
    /// <summary>
    /// Called by signal sources to send a signal to the sensors.
    /// </summary>
    /// <remarks>
    /// FEMSenseManager uses a map-graph-based approach to determine whether to relay or
    /// not a signal to sensors.
    /// </remarks>
    /// <param name="signal">Signal to be sent.</param>
    public override void RegisterSignal(RegionSenseSignal signal)
    {
        UpdateNodeIntensitiesWithSignal(signal);
        foreach (KeyValuePair<uint, List<IRegionSenseSensor>> nodeSensors in
                 _registeredNodeSensors)
        { 
            NotifySensors(signal, nodeSensors.Value.ToArray());
        }
    }

    /// <summary>
    /// Disseminate the signal through the graph and add it to the remaining intensities
    /// of previous signals not yet dissipated.
    /// </summary>
    /// <param name="signal">Signal just received.</param>
    private void UpdateNodeIntensitiesWithSignal(RegionSenseSignal signal)
    {
        Dictionary<uint, float> disseminationIntensities = new();
        
        // Nodes not fully explored yet, ordered as they are found to traverse the graph
        // using a breath-first order.
        _openNodes.Clear();
        
        // Nodes already fully explored.
        _closedNodes.Clear();
        
        // Start from the source signal node.
        PositionNode sourceNode = 
            mapGraph.GetNodeAtNearestPosition(signal.source.transform.position);
        disseminationIntensities[sourceNode.Id] = signal.strength;
        _openNodes.Enqueue(sourceNode);
        // Add the node to be explored in the closed list once you have added it to the
        // open list. This way you can avoid exploring the same node multiple times. 
        _closedNodes.Add(sourceNode.Id);
        
        // Breath-first exploration of the graph.
        while (_openNodes.Count > 0)
        {
            PositionNode current = _openNodes.Dequeue();
            
            foreach (GraphConnection graphConnection in current.Connections.Values)
            {
                // Where does that connection lead us?
                PositionNode endNode = 
                    mapGraph.GetPositionNodeById(graphConnection.endNodeId);
                // If that connection leads to an already explored node, skip it.
                if (_closedNodes.Contains(endNode.Id)) continue;
                
                // Otherwise, calculate the signal attenuation from the current node
                // through this connection.
                float strengthThroughConnection =
                    disseminationIntensities[current.Id] * signal.modality.Attenuation;
                
                // If a node has such a low signal intensity, skip it.
                if (strengthThroughConnection < minimumDisseminationIntensity) continue;
                
                // As we are using the same attenuation factor for all connections, and
                // the exploration is a breath-first order, we can safely assume that
                // the first time you meet this end node, it will receive the signal
                // with the highest possible intensity. 
                disseminationIntensities[endNode.Id] = strengthThroughConnection;
                
                // Include the discovered node in the open set to explore it further
                // later.
                _openNodes.Enqueue(endNode);
                // Add the node to be explored in the closed list once you have added it
                // to the open list. This way you can avoid exploring the same node
                // multiple times.
                _closedNodes.Add(endNode.Id);
            }
        }
        
        // Node intensity is a sum of dissipation (the remaining intensity from previous
        // iterations) and dissemination (the added intensity from the new signal). Now
        // that dissemination intensities are calculated, we must add the dissipation 
        // intensities.
        _nodeIntensities.Clear();
        // First, dissipation intensities.
        foreach (KeyValuePair<uint, float> dissipationIntensity in 
                 _dissipationIntensities)
        {
            _nodeIntensities[dissipationIntensity.Key] = dissipationIntensity.Value;
        }
        // Next, dissemination intensities.
        foreach (KeyValuePair<uint, float> disseminationIntensity in 
                 disseminationIntensities)
        {
            if (_nodeIntensities.ContainsKey(disseminationIntensity.Key))
            {
                _nodeIntensities[disseminationIntensity.Key] += 
                    disseminationIntensity.Value;
            }
            else
            {
                _nodeIntensities[disseminationIntensity.Key] = 
                    disseminationIntensity.Value;
            }
        }
    }
    
    protected override bool SignalPowerfulEnoughForSensor(
        RegionSenseSignal signal, 
        IRegionSenseSensor sensor)
    {
        uint sensorNodeId = mapGraph.GetNodeAtNearestPosition(sensor.Position).Id;
        if (!_nodeIntensities.TryGetValue(sensorNodeId, out var receivedPower)) 
            return false;
        return receivedPower >= sensor.ModalityThreshold(signal.modality);
    }
}
}