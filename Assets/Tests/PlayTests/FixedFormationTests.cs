using System.Collections;
using NUnit.Framework;
using SteeringBehaviors;
using Tests.PlayTests.Common;
using Tools;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.PlayTests
{
public class FixedFormationTests
{
    private const string CurrentScene = "TestFormationYard";
        
    private Transform _position1;
    private Transform _position2;
    private Transform _position3;
    private Transform _position4;

    private Target _target;
    
    private GameObject _fixedFormationGameObject;
    private UsherFormationAgent _usherFixedFormationAgent;
    private SeekSteeringBehavior _seekSteeringBehavior;
    
    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // Clean up any existing objects first
        _fixedFormationGameObject = null;
        _usherFixedFormationAgent = null;
        _seekSteeringBehavior = null;
        _target = null;

        // Load the test scene
        yield return TestLevelManagement.ReLoadScene(CurrentScene);
        yield return null;
        
        GameObject.Find("ScalableFormationGroup").SetActive(false);
        GameObject.Find("TwoLevelFormationGroup").SetActive(false);
        GameObject.Find("FixedFormationGroup").SetActive(true);
        
        if (_position1 == null)
            _position1 = GameObject.Find("Position1").transform;
        if (_position2 == null)
            _position2 = GameObject.Find("Position2").transform;
        if (_position3 == null)
            _position3 = GameObject.Find("Position3").transform;
        if (_position4 == null)
            _position4 = GameObject.Find("Position4").transform;
        if (_target == null)
        {
            _target = GameObject.Find("Target").GetComponent<Target>();
            _target.TargetPosition = _position1.position;
            _target.Enabled = false;
        }

        if (_fixedFormationGameObject == null)
        {
            _fixedFormationGameObject = GameObject.Find("UsherFixedFormationAgent");
            _fixedFormationGameObject.SetActive(false);
        }

        if (_usherFixedFormationAgent == null)
        {
            _usherFixedFormationAgent = _fixedFormationGameObject.GetComponent<UsherFormationAgent>();
        }

        if (_seekSteeringBehavior == null)
        {
            _seekSteeringBehavior = _fixedFormationGameObject.GetComponentInChildren<SeekSteeringBehavior>();
        }
    }
    
    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (_fixedFormationGameObject != null)
            _fixedFormationGameObject.SetActive(false);
        if (_target != null)
            _target.Enabled = false;

        yield return null;
    }
    
    /// <summary>
    /// Test that formation can make a full turn in unrealistic mode.
    /// </summary>
    [UnityTest]
    public IEnumerator FixedFormationNonRealisticTurnDownUpTest()
    {
        // Setup agents before the tests.
        _target.Enabled = true;
        _target.TargetPosition = _position2.position;
        _fixedFormationGameObject.transform.position = _position1.position;
        _usherFixedFormationAgent.MaximumSpeed = 2.0f;
        _usherFixedFormationAgent.StopSpeed = 0.1f;
        _usherFixedFormationAgent.MaximumRotationalSpeed = 360f;
        _usherFixedFormationAgent.StopRotationThreshold = 1f;
        _seekSteeringBehavior.Target = _target.gameObject;
        _seekSteeringBehavior.ArrivalDistance = 0.3f;

        _fixedFormationGameObject.SetActive(true);
        
        _usherFixedFormationAgent.RealisticTurns = false;
        
        
        
        // Start test.
        
        // Assert that formation reached its target.
        yield return new WaitForSeconds(7f);
        Assert.True(Vector2.Distance(
            _fixedFormationGameObject.transform.position, 
            _position2.transform.position) < 0.5f);
        
        // Move the target to another position.
        _target.transform.position = _position3.transform.position;
        
        // Assert that formation reached its target.
        yield return new WaitForSeconds(12f);
        Assert.True(Vector2.Distance(
            _fixedFormationGameObject.transform.position, 
            _position3.transform.position) < 0.5f);
        
        // Move the target to another position.
        _target.transform.position = _position4.transform.position;
        
        // Assert that formation reached its target.
        yield return new WaitForSeconds(5f);
        Assert.True(Vector2.Distance(
            _fixedFormationGameObject.transform.position, 
            _position4.transform.position) < 0.5f);
    }
    
    /// <summary>
    /// Test that formation can make a full turn in realistic mode.
    /// </summary>
    [UnityTest]
    public IEnumerator FixedFormationRealisticTurnDownUpTest()
    {
        // Setup agents before the tests.
        _target.Enabled = true;
        _target.TargetPosition = _position2.position;
        _fixedFormationGameObject.transform.position = _position1.position;
        _usherFixedFormationAgent.MaximumSpeed = 2.0f;
        _usherFixedFormationAgent.StopSpeed = 0.1f;
        _usherFixedFormationAgent.MaximumRotationalSpeed = 360f;
        _usherFixedFormationAgent.StopRotationThreshold = 1f;
        _seekSteeringBehavior.Target = _target.gameObject;
        _seekSteeringBehavior.ArrivalDistance = 0.3f;

        _usherFixedFormationAgent.RealisticTurns = true;
        
        _fixedFormationGameObject.SetActive(true);
        
        // Start test.
        // Assert that formation reached its target.
        yield return new WaitForSeconds(12f);
        Assert.True(Vector2.Distance(
            _fixedFormationGameObject.transform.position, 
            _position2.transform.position) < 0.5f);
        
        // Move the target to another position.
        _target.transform.position = _position3.transform.position;
        
        // Assert that formation reached its target.
        yield return new WaitForSeconds(15f);
        Assert.True(Vector2.Distance(
            _fixedFormationGameObject.transform.position, 
            _position3.transform.position) < 0.5f);
        
        // Move the target to another position.
        _target.transform.position = _position4.transform.position;
        
        // Assert that formation reached its target.
        yield return new WaitForSeconds(14f);
        Assert.True(Vector2.Distance(
            _fixedFormationGameObject.transform.position, 
            _position4.transform.position) < 0.5f);
    }
    
    /// <summary>
    /// Test that formation can make a full turn in unrealistic mode.
    /// </summary>
    [UnityTest]
    public IEnumerator FixedFormationNonRealisticTurnUpDownTest()
    {
        // Setup agents before the tests.
        _target.Enabled = true;
        _target.TargetPosition = _position3.position;
        _fixedFormationGameObject.transform.position = _position4.position;
        _usherFixedFormationAgent.MaximumSpeed = 2.0f;
        _usherFixedFormationAgent.StopSpeed = 0.1f;
        _usherFixedFormationAgent.MaximumRotationalSpeed = 360f;
        _usherFixedFormationAgent.StopRotationThreshold = 1f;
        _seekSteeringBehavior.Target = _target.gameObject;
        _seekSteeringBehavior.ArrivalDistance = 0.3f;

        _usherFixedFormationAgent.RealisticTurns = false;
        
        _fixedFormationGameObject.SetActive(true);
        
        // Start test.
        
        // Assert that formation reached its target.
        yield return new WaitForSeconds(5f);
        Assert.True(Vector2.Distance(
            _fixedFormationGameObject.transform.position, 
            _position3.transform.position) < 0.5f);
        
        // Move the target to another position.
        _target.transform.position = _position2.transform.position;
        
        // Assert that formation reached its target.
        yield return new WaitForSeconds(12f);
        Assert.True(Vector2.Distance(
            _fixedFormationGameObject.transform.position, 
            _position2.transform.position) < 0.5f);
        
        // Move the target to another position.
        _target.transform.position = _position1.transform.position;
        
        // Assert that formation reached its target.
        yield return new WaitForSeconds(8f);
        Assert.True(Vector2.Distance(
            _fixedFormationGameObject.transform.position, 
            _position1.transform.position) < 0.5f);
    }
    
    /// <summary>
    /// Test that formation can make a full turn in realistic mode.
    /// </summary>
    [UnityTest]
    public IEnumerator FixedFormationRealisticTurnUpDownTest()
    {
        // Setup agents before the tests.
        _target.Enabled = true;
        _target.TargetPosition = _position3.position;
        _fixedFormationGameObject.transform.position = _position4.position;
        _usherFixedFormationAgent.MaximumSpeed = 2.0f;
        _usherFixedFormationAgent.StopSpeed = 0.1f;
        _usherFixedFormationAgent.MaximumRotationalSpeed = 360f;
        _usherFixedFormationAgent.StopRotationThreshold = 1f;
        _seekSteeringBehavior.Target = _target.gameObject;
        _seekSteeringBehavior.ArrivalDistance = 0.3f;

        _usherFixedFormationAgent.RealisticTurns = true;
        
        _fixedFormationGameObject.SetActive(true);
        
        // Start test.
        // Assert that formation reached its target.
        yield return new WaitForSeconds(14f);
        Assert.True(Vector2.Distance(
            _fixedFormationGameObject.transform.position, 
            _position3.transform.position) < 0.5f);
        
        // Move the target to another position.
        _target.transform.position = _position2.transform.position;
        
        // Assert that formation reached its target.
        yield return new WaitForSeconds(15f);
        Assert.True(Vector2.Distance(
            _fixedFormationGameObject.transform.position, 
            _position2.transform.position) < 0.5f);
        
        // Move the target to another position.
        _target.transform.position = _position1.transform.position;
        
        // Assert that formation reached its target.
        yield return new WaitForSeconds(10f);
        Assert.True(Vector2.Distance(
            _fixedFormationGameObject.transform.position, 
            _position1.transform.position) < 0.5f);
    }
}
}