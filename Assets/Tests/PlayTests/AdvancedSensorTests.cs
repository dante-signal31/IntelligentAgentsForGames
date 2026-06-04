using System.Collections;
using System.Linq;
using NUnit.Framework;
using SteeringBehaviors;
using ninja.dlab.Commontesttools;
using Sensors;
using Tools;
using UnityEngine;
using UnityEngine.TestTools;


namespace Tests.PlayTests
{
public class AdvancedSensorTests
{
    private const string CurrentScene = "TestAdvancedSensesYard";

    private Transform _position1;
    private Transform _position2;
    private Transform _position3;
    private Transform _position4;
    private Transform _position5;
    private Transform _position6;
    private Transform _position7;
    private Transform _position8;

    private Target _target;

    private GameObject _soundEmitterGameObject;
    private GameObject _soundChaserGameObject;
    private GameObject _soundChaserGameObject2;
    private GameObject _soundChaserGameObject3;

    private AgentMover _soundEmitterAgent;
    private AgentMover _soundChaserAgent;
    private AgentMover _soundChaserAgent2;
    private AgentMover _soundChaserAgent3;

    private AgentColor _soundEmitterAgentColor;
    private AgentColor _soundChaserAgentColor;
    private AgentColor _soundChaserAgentColor2;
    private AgentColor _soundChaserAgentColor3;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // Clean up any existing objects first
        _soundEmitterGameObject = null;
        _soundChaserGameObject = null;
        _soundChaserGameObject2 = null;
        _soundChaserGameObject3 = null;

        _soundEmitterAgent = null;
        _soundChaserAgent = null;
        _soundChaserAgent2 = null;
        _soundChaserAgent3 = null;

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

        if (_target == null)
        {
            _target = GameObject.Find("Target").GetComponent<Target>();
            _target.TargetPosition = _position1.position;
            _target.Enabled = false;
        }

        if (_soundEmitterGameObject== null)
        {
            _soundEmitterGameObject = GameObject.Find("SoundEmitterMovingAgent");
            _soundEmitterGameObject.SetActive(false);
        }

        if (_soundChaserGameObject == null)
        {
            _soundChaserGameObject = GameObject.Find("SoundChaserMovingAgent");
            _soundChaserGameObject.SetActive(false);
        }

        if (_soundChaserGameObject2 == null)
        {
            _soundChaserGameObject2 = GameObject.Find("SoundChaserMovingAgent2");
            _soundChaserGameObject2.SetActive(false);
        }

        if (_soundChaserGameObject3 == null)
        {
            _soundChaserGameObject3 = GameObject.Find("SoundChaserMovingAgent3");
            _soundChaserGameObject3.SetActive(false);
        }

        if (_soundEmitterAgent == null)
        {
            _soundEmitterAgent = _soundEmitterGameObject.GetComponent<AgentMover>();
            _soundEmitterAgentColor = _soundEmitterGameObject.GetComponent<AgentColor>();
        }

        if (_soundChaserAgent == null)
        {
            _soundChaserAgent = _soundChaserGameObject.GetComponent<AgentMover>();
            _soundChaserAgentColor = _soundChaserGameObject.GetComponent<AgentColor>();
        }

        if (_soundChaserAgent2 == null)
        {
            _soundChaserAgent2 = _soundChaserGameObject2.GetComponent<AgentMover>();
            _soundChaserAgentColor2 = _soundChaserGameObject2.GetComponent<AgentColor>();
        }

        if (_soundChaserAgent3 == null)
        {
            _soundChaserAgent3 = _soundChaserGameObject3.GetComponent<AgentMover>();
            _soundChaserAgentColor3 = _soundChaserGameObject3.GetComponent<AgentColor>();
        }
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (_soundEmitterGameObject != null)
            _soundEmitterGameObject.SetActive(false);
        if (_soundChaserGameObject != null)
            _soundChaserGameObject.SetActive(false);
        if (_soundChaserGameObject2 != null)
            _soundChaserGameObject2.SetActive(false);
        if (_soundChaserGameObject3 != null)
            _soundChaserGameObject3.SetActive(false);
        
        if (_target != null)
            _target.Enabled = false;

        yield return null;
    }


    /// <summary>
    /// Test that sound chasers react to an emitting sound.
    /// </summary>
    [UnityTest]
    public IEnumerator SoundEmittingSensorTest()
    {
        // Get references.
        SeekSteeringBehavior emitterSeekSteeringBehavior = 
            _soundEmitterGameObject.GetComponentInChildren<SeekSteeringBehavior>();
        RegionSenseSoundSignalEmitter signalEmitter = 
            _soundEmitterGameObject.GetComponentInChildren<RegionSenseSoundSignalEmitter>();
        RegionSenseSoundSensor chaserSensor = 
            _soundChaserGameObject.GetComponentInChildren<RegionSenseSoundSensor>();
        RegionSenseSoundSensor chaserSensor2 = 
            _soundChaserGameObject2.GetComponentInChildren<RegionSenseSoundSensor>();
        RegionSenseSoundSensor chaserSensor3 = 
            _soundChaserGameObject3.GetComponentInChildren<RegionSenseSoundSensor>();
        
        // Setup agents before the test.
        _target.transform.position = _position1.transform.position;
        
        _soundEmitterAgent.MaximumSpeed = 4.0f;
        _soundEmitterAgent.transform.position = _position1.transform.position;
        _soundEmitterAgent.transform.eulerAngles = Vector3.zero;
        _soundEmitterAgentColor.Color = Color.green;
        _soundEmitterGameObject.SetActive(true);
        emitterSeekSteeringBehavior.Target = _target.gameObject;
        signalEmitter.ModalityMaximumRange = 10.0f;
        signalEmitter.ModalityAttenuation = 0.9f;
        signalEmitter.ModalityInverseTransmissionSpeed = 2 / 100f;
        signalEmitter.EmissionPeriod = 0.3f;
        signalEmitter.autoStartEmission = true;
        signalEmitter.signalStrength = 100f;
        
        _soundChaserAgent.MaximumSpeed = 3.0f;
        _soundChaserAgent.transform.position = _position3.transform.position;
        _soundChaserAgentColor.Color = Color.red;
        _soundChaserGameObject.SetActive(true);
        chaserSensor.detectionBufferSize = 10;
        chaserSensor.minimumStrengthDetectionThreshold = 40;
        chaserSensor.detectionExpirationTime = 1.0f;
        chaserSensor.cleaningPeriod = 0.3f;
        
        _soundChaserAgent2.MaximumSpeed = 3.0f;
        _soundChaserAgent2.transform.position = _position4.transform.position;
        _soundChaserAgentColor2.Color = Color.red;
        _soundChaserGameObject2.SetActive(true);
        chaserSensor2.detectionBufferSize = 10;
        chaserSensor2.minimumStrengthDetectionThreshold = 40;
        chaserSensor2.detectionExpirationTime = 1.0f;
        chaserSensor2.cleaningPeriod = 0.3f;
        
        _soundChaserAgent3.MaximumSpeed = 3.0f;
        _soundChaserAgent3.transform.position = _position2.transform.position;
        _soundChaserAgentColor3.Color = Color.red;
        _soundChaserGameObject3.SetActive(true);
        chaserSensor3.detectionBufferSize = 10;
        chaserSensor3.minimumStrengthDetectionThreshold = 40;
        chaserSensor3.detectionExpirationTime = 1.0f;
        chaserSensor3.cleaningPeriod = 0.3f;

        // Start test.

        // The soundEmitter has not moved yet, nor any of the chasers.
        yield return new WaitForSeconds(1.0f);
        Assert.True(Vector2.Distance(_soundEmitterAgent.transform.position, _position1.transform.position) < 0.3f);
        Assert.True(Vector2.Distance(_soundChaserAgent.transform.position, _position3.transform.position) < 0.3f);
        Assert.True(Vector2.Distance(_soundChaserAgent2.transform.position, _position4.transform.position) < 0.3f);
        Assert.True(Vector2.Distance(_soundChaserAgent3.transform.position, _position2.transform.position) < 0.3f);
        

        // Change to a position where one of the chasers can hear us.
        _target.transform.position = _position5.transform.position;
        yield return new WaitForSeconds(5.0f);
        _target.transform.position = _position6.transform.position;
        yield return new WaitForSeconds(4.0f);
        // Did we make the chaser move?
        Assert.True(Vector2.Distance(_soundChaserAgent3.transform.position, _position2.transform.position) > 1.0f);
        // The other two should not have moved.
        Assert.True(Vector2.Distance(_soundChaserAgent.transform.position, _position3.transform.position) < 0.3f);
        Assert.True(Vector2.Distance(_soundChaserAgent2.transform.position, _position4.transform.position) < 0.3f);
        
        // Change to a position where the second can hear us.
        _target.transform.position = _position7.transform.position;
        yield return new WaitForSeconds(1.0f);
        _target.transform.position = _position8.transform.position;
        yield return new WaitForSeconds(3.0f);
        _target.transform.position = _position1.transform.position;
        yield return new WaitForSeconds(2.0f);
        // Did we make the chaser move?
        Assert.True(Vector2.Distance(_soundChaserAgent.transform.position, _position3.transform.position) > 1.0f);
        // The last chased should not have moved.
        Assert.True(Vector2.Distance(_soundChaserAgent2.transform.position, _position4.transform.position) < 0.3f);
    }
}
}