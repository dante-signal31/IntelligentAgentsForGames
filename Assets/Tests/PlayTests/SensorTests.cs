using System.Collections;
using NUnit.Framework;
using SteeringBehaviors;
using ninja.dlab.Commontesttools;
using Sensors;
using Tools;
using UnityEngine;
using UnityEngine.TestTools;


namespace Tests.PlayTests
{
public class SensorTests
{
    private const string CurrentScene = "TestObstacleTiledYard";

    private Transform _position1;
    private Transform _position2;
    private Transform _position3;
    private Transform _position4;
    private Transform _position5;
    private Transform _position6;
    private Transform _position7;
    private Transform _position8;
    private Transform _position9;
    private Transform _position10;

    private Target _target;

    private GameObject _seekGameObject;
    private GameObject _hideGameObject;
    private GameObject _wallAvoiderGameObject;
    private GameObject _smoothedWallAvoiderGameObject;
    private GameObject _weightBlendedHideWallAvoiderGameObject;
    private GameObject _priorityWeightBlendedHideWallAvoiderGameObject;
    private GameObject _priorityDitheringBlendedHideWallAvoiderGameObject;
    private GameObject _pathFollowingGameObject;
    private GameObject _pathGameObject;
    private GameObject _contextGameObject;
    private GameObject _pipelineGameObject;
    private GameObject _wanderObstacleGameObject;
    private GameObject _pipelineTestPath;
    private GameObject _coneSensorGameObject;

    private SeekSteeringBehavior _seekSteeringBehavior;
    private HideSteeringBehavior _hideSteeringBehavior;
    private ActiveWallAvoiderSteeringBehavior _wallAvoiderSteeringBehavior;
    private SmoothedWallAvoiderSteeringBehavior _smoothedWallAvoiderSteeringBehavior;
    private ContextSteeringBehavior _contextSteeringBehavior;
    private PathFollowingSteeringBehavior _pathFollowingSteeringBehavior;

    private AgentMover _seekAgent;
    private AgentMover _hideAgent;
    private AgentMover _wallAvoiderAgent;
    private AgentMover _smoothedWallAvoiderAgent;
    private AgentMover _weightBlendedHideWallAvoiderAgent;
    private AgentMover _priorityWeightBlendedHideWallAvoiderAgent;
    private AgentMover _priorityDitheringBlendedHideWallAvoiderAgent;
    private AgentMover _contextAgent;
    private AgentMover _pipelineAgent;
    private AgentMover _wanderObstacleAgent;
    private AgentMover _pathFollowingAgent;
    private AgentMover _coneSensorAgent;

    private AgentColor _seekAgentColor;
    private AgentColor _hideAgentColor;
    private AgentColor _wallAvoiderAgentColor;
    private AgentColor _smoothedWallAvoiderAgentColor;
    private AgentColor _weightBlendedHideWallAvoiderAgentColor;
    private AgentColor _priorityWeightBlendedHideWallAvoiderAgentColor;
    private AgentColor _priorityDitheringBlendedHideWallAvoiderAgentColor;
    private AgentColor _contextAgentColor;
    private AgentColor _pipelineAgentColor;
    private AgentColor _wanderObstacleAgentColor;
    private AgentColor _pathFollowingAgentColor;
    private AgentColor _coneSensorAgentColor;
    
    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // Clean up any existing objects first
        _seekGameObject = null;
        _hideGameObject = null;
        _wallAvoiderGameObject = null;
        _seekSteeringBehavior = null;
        _hideSteeringBehavior = null;
        _wallAvoiderSteeringBehavior = null;
        _smoothedWallAvoiderSteeringBehavior = null;
        _seekAgent = null;
        _hideAgent = null;
        _wallAvoiderAgent = null;
        _smoothedWallAvoiderAgent = null;
        _weightBlendedHideWallAvoiderAgent = null;
        _seekAgentColor = null;
        _hideAgentColor = null;
        _wallAvoiderAgentColor = null;
        _smoothedWallAvoiderAgentColor = null;
        _weightBlendedHideWallAvoiderAgentColor = null;
        _priorityWeightBlendedHideWallAvoiderAgentColor = null;
        _contextGameObject = null;
        _contextAgentColor = null;
        _contextAgent = null;
        _pipelineGameObject = null;
        _pipelineAgent = null;
        _wanderObstacleGameObject = null;
        _wanderObstacleAgent = null;
        _pathFollowingGameObject = null;
        _coneSensorGameObject = null;
        _coneSensorAgent = null;
        _target = null;

        // Load the test scene
        yield return TestLevelManagement.ReLoadScene(CurrentScene);
        yield return null;

        if (_position1 == null)
            _position1 = GameObject.Find("Position1").transform;
        if (_position2 == null)
            _position2 = GameObject.Find("Position2").transform;
        if (_position3 == null)
            _position3 = GameObject.Find("Position3").transform;
        if (_position4 == null)
            _position4 = GameObject.Find("Position4").transform;
        if (_position5 == null)
            _position5 = GameObject.Find("Position5").transform;
        if (_position6 == null)
            _position6 = GameObject.Find("Position6").transform;
        if (_position7 == null)
            _position7 = GameObject.Find("Position7").transform;
        if (_position8 == null)
            _position8 = GameObject.Find("Position8").transform;
        if (_position9 == null)
            _position9 = GameObject.Find("Position9").transform;
        if (_position10 == null)
            _position10 = GameObject.Find("Position10").transform;
        
        if (_target == null)
        {
            _target = GameObject.Find("Target").GetComponent<Target>();
            _target.TargetPosition = _position1.position;
            _target.Enabled = false;
        }

        if (_seekGameObject == null)
        {
            _seekGameObject = GameObject.Find("SeekMovingAgent");
            _seekGameObject.SetActive(false);
        }

        if (_hideGameObject == null)
        {
            _hideGameObject = GameObject.Find("HideMovingAgent");
            _hideGameObject.SetActive(false);
        }

        if (_wallAvoiderGameObject == null)
        {
            _wallAvoiderGameObject = GameObject.Find("WallAvoiderMovingAgent");
            _wallAvoiderGameObject.SetActive(false);
        }

        if (_smoothedWallAvoiderGameObject == null)
        {
            _smoothedWallAvoiderGameObject =
                GameObject.Find("SmoothedWallAvoiderMovingAgent");
            _smoothedWallAvoiderGameObject.SetActive(false);
        }

        if (_weightBlendedHideWallAvoiderGameObject == null)
        {
            _weightBlendedHideWallAvoiderGameObject =
                GameObject.Find("WeightBlendedHideWallAvoiderMovingAgent");
            _weightBlendedHideWallAvoiderGameObject.SetActive(false);
        }

        if (_priorityWeightBlendedHideWallAvoiderGameObject == null)
        {
            _priorityWeightBlendedHideWallAvoiderGameObject =
                GameObject.Find("PriorityWeightBlendedHideWallAvoiderMovingAgent");
            _priorityWeightBlendedHideWallAvoiderGameObject.SetActive(false);
        }

        if (_priorityDitheringBlendedHideWallAvoiderGameObject == null)
        {
            _priorityDitheringBlendedHideWallAvoiderGameObject =
                GameObject.Find("PriorityDitheringBlendedHideWallAvoiderMovingAgent");
            _priorityDitheringBlendedHideWallAvoiderGameObject.SetActive(false);
        }

        if (_pathFollowingGameObject == null)
        {
            _pathFollowingGameObject = GameObject.Find("PathFollowingMovingAgent");
            _pathFollowingGameObject.SetActive(false);
        }

        if (_contextGameObject == null)
        {
            _contextGameObject = GameObject.Find("ContextMovingAgent");
            _contextGameObject.SetActive(false);
        }

        if (_pipelineGameObject == null)
        {
            _pipelineGameObject = GameObject.Find("PipelineMovingAgent");
            _pipelineGameObject.SetActive(false);
        }

        if (_wanderObstacleGameObject == null)
        {
            _wanderObstacleGameObject = GameObject.Find("WanderMovingAgent_Obstacle");
            _wanderObstacleGameObject.SetActive(false);
        }

        if (_coneSensorGameObject == null)
        {
            _coneSensorGameObject = GameObject.Find("ConeSensorMovingAgent");
            _coneSensorGameObject.SetActive(false);
        }

        if (_pathGameObject == null)
        {
            _pathGameObject = GameObject.Find("Path");
            _pathGameObject.SetActive(false);
        }

        if (_pipelineTestPath == null)
        {
            _pipelineTestPath = GameObject.Find("PipelineTestPath");
            _pipelineTestPath.SetActive(false);
        }

        if (_seekSteeringBehavior == null)
            _seekSteeringBehavior =
                _seekGameObject.GetComponentInChildren<SeekSteeringBehavior>();
        if (_hideSteeringBehavior == null)
            _hideSteeringBehavior =
                _hideGameObject.GetComponentInChildren<HideSteeringBehavior>();
        if (_wallAvoiderSteeringBehavior == null)
            _wallAvoiderSteeringBehavior =
                _wallAvoiderGameObject
                    .GetComponentInChildren<ActiveWallAvoiderSteeringBehavior>();
        if (_smoothedWallAvoiderSteeringBehavior == null)
            _smoothedWallAvoiderSteeringBehavior =
                _smoothedWallAvoiderGameObject
                    .GetComponentInChildren<SmoothedWallAvoiderSteeringBehavior>();
        if (_contextSteeringBehavior == null)
            _contextSteeringBehavior = _contextGameObject
                .GetComponentInChildren<ContextSteeringBehavior>();
        if (_pathFollowingSteeringBehavior == null)
            _pathFollowingSteeringBehavior = _pathFollowingGameObject
                .GetComponentInChildren<PathFollowingSteeringBehavior>();

        if (_seekAgent == null)
            _seekAgent = _seekGameObject.GetComponent<AgentMover>();
        if (_hideAgent == null)
            _hideAgent = _hideGameObject.GetComponent<AgentMover>();
        if (_wallAvoiderAgent == null)
            _wallAvoiderAgent = _wallAvoiderGameObject.GetComponent<AgentMover>();
        if (_smoothedWallAvoiderAgent == null)
            _smoothedWallAvoiderAgent =
                _smoothedWallAvoiderGameObject.GetComponent<AgentMover>();
        if (_weightBlendedHideWallAvoiderAgent == null)
            _weightBlendedHideWallAvoiderAgent =
                _weightBlendedHideWallAvoiderGameObject.GetComponent<AgentMover>();
        if (_priorityWeightBlendedHideWallAvoiderAgent == null)
            _priorityWeightBlendedHideWallAvoiderAgent =
                _priorityWeightBlendedHideWallAvoiderGameObject
                    .GetComponent<AgentMover>();
        if (_priorityDitheringBlendedHideWallAvoiderAgent == null)
            _priorityDitheringBlendedHideWallAvoiderAgent =
                _priorityDitheringBlendedHideWallAvoiderGameObject
                    .GetComponent<AgentMover>();
        if (_contextAgent == null)
            _contextAgent = _contextGameObject.GetComponent<AgentMover>();
        if (_pipelineAgent == null)
            _pipelineAgent = _pipelineGameObject.GetComponent<AgentMover>();
        if (_wanderObstacleAgent == null)
            _wanderObstacleAgent =
                _wanderObstacleGameObject.GetComponent<AgentMover>();
        if (_pathFollowingAgent == null)
            _pathFollowingAgent = _pathFollowingGameObject.GetComponent<AgentMover>();
        if (_coneSensorAgent == null)
            _coneSensorAgent = _coneSensorGameObject.GetComponent<AgentMover>();

        if (_seekAgentColor == null)
            _seekAgentColor = _seekGameObject.GetComponent<AgentColor>();
        if (_hideAgentColor == null)
            _hideAgentColor = _hideGameObject.GetComponent<AgentColor>();
        if (_wallAvoiderAgentColor == null)
            _wallAvoiderAgentColor =
                _wallAvoiderGameObject.GetComponent<AgentColor>();
        if (_smoothedWallAvoiderAgentColor == null)
            _smoothedWallAvoiderAgentColor =
                _smoothedWallAvoiderGameObject.GetComponent<AgentColor>();
        if (_weightBlendedHideWallAvoiderAgentColor == null)
            _weightBlendedHideWallAvoiderAgentColor =
                _weightBlendedHideWallAvoiderGameObject.GetComponent<AgentColor>();
        if (_priorityWeightBlendedHideWallAvoiderAgentColor == null)
            _priorityWeightBlendedHideWallAvoiderAgentColor =
                _priorityWeightBlendedHideWallAvoiderGameObject
                    .GetComponent<AgentColor>();
        if (_priorityDitheringBlendedHideWallAvoiderAgentColor == null)
            _priorityDitheringBlendedHideWallAvoiderAgentColor =
                _priorityDitheringBlendedHideWallAvoiderGameObject
                    .GetComponent<AgentColor>();
        if (_contextAgentColor == null)
            _contextAgentColor = _contextGameObject.GetComponent<AgentColor>();
        if (_pipelineAgentColor == null)
            _pipelineAgentColor = _pipelineGameObject.GetComponent<AgentColor>();
        if (_wanderObstacleAgentColor)
            _wanderObstacleAgentColor =
                _wanderObstacleGameObject.GetComponent<AgentColor>();
        if (_pathFollowingAgentColor == null)
            _pathFollowingAgentColor =
                _pathFollowingGameObject.GetComponent<AgentColor>();
        if (_coneSensorAgentColor == null)
            _coneSensorAgentColor = _coneSensorGameObject.GetComponent<AgentColor>();
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (_seekGameObject != null)
            _seekGameObject.SetActive(false);
        if (_hideGameObject != null)
            _hideGameObject.SetActive(false);
        if (_wallAvoiderGameObject != null)
            _wallAvoiderGameObject.SetActive(false);
        if (_smoothedWallAvoiderGameObject != null)
            _smoothedWallAvoiderGameObject.SetActive(false);
        if (_weightBlendedHideWallAvoiderGameObject != null)
            _weightBlendedHideWallAvoiderGameObject.SetActive(false);
        if (_priorityWeightBlendedHideWallAvoiderGameObject != null)
            _priorityWeightBlendedHideWallAvoiderGameObject.SetActive(false);
        if (_priorityDitheringBlendedHideWallAvoiderGameObject != null)
            _priorityDitheringBlendedHideWallAvoiderGameObject.SetActive(false);
        if (_contextGameObject != null)
            _contextGameObject.SetActive(false);
        if (_pipelineGameObject != null)
            _pipelineGameObject.SetActive(false);
        if (_wanderObstacleGameObject != null)
            _wanderObstacleGameObject.SetActive(false);
        if (_pathGameObject != null)
            _pathGameObject.SetActive(false);
        if (_coneSensorGameObject != null)
            _coneSensorGameObject.SetActive(false);
        if (_target != null)
            _target.Enabled = false;

        yield return null;
    }


    /// <summary>
    /// Test that ConeSensor works with line-of-sight.
    /// </summary>
    [UnityTest]
    public IEnumerator ConeSensorTest()
    {
        // Get reference to the ConeSensor.
        ConeSensor coneSensor = _coneSensorGameObject.GetComponentInChildren<ConeSensor>();
            
        // Setup agents before the test.
        _coneSensorAgent.MaximumSpeed = 0.0f;
        _coneSensorAgent.transform.position = _position4.transform.position;
        _coneSensorAgent.transform.eulerAngles = Vector3.zero;
        _coneSensorAgentColor.Color = Color.green;
        _coneSensorGameObject.SetActive(true);
        
        _hideAgent.transform.position = _position1.transform.position;
        _hideAgent.MaximumSpeed = 0.0f;
        _hideAgentColor.Color = Color.red;
        _hideGameObject.SetActive(true);
        
        _seekAgent.transform.position = _position9.transform.position;
        _seekAgentColor.Color = Color.red;
        _seekAgent.MaximumSpeed = 0.0f;
        _seekGameObject.SetActive(true);
        
        _pathFollowingAgent.transform.position = _position8.transform.position;
        _pathFollowingAgent.MaximumSpeed = 0.0f;
        _pathFollowingAgentColor.Color = Color.red;
        _pathFollowingGameObject.SetActive(true);
        
        // Start test.

        // The coneSensorAgents looks away from the other agents. So, it should detect
        // none.
        yield return new WaitForSeconds(1.0f);
        Assert.True(coneSensor.DetectedObjects.Count == 0);
        
        // Change to a position where it should see the three of the other agents.
        _coneSensorAgent.transform.position = _position2.transform.position;
        _coneSensorAgent.transform.eulerAngles = new Vector3(0, 0, 40);
        // Give cone sensor time to detect agents.
        yield return new WaitForSeconds(1.0f);
        Assert.True(coneSensor.DetectedObjects.Count == 3);
        
        // Change to a position where it should only one of the agents because the
        // other two are behind an obstacle. 
        _coneSensorAgent.transform.position = _position10.transform.position;
        _coneSensorAgent.transform.eulerAngles = new Vector3(0, 0, -130);
        // Give cone sensor time to detect agents.
        yield return new WaitForSeconds(1.0f);
        Assert.True(coneSensor.DetectedObjects.Count == 1);
    }
}
}