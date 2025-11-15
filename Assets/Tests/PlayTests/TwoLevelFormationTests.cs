using System.Collections;
using NUnit.Framework;
using SteeringBehaviors;
using Tests.PlayTests.Common;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.PlayTests
{
public class TwoLevelFormationTests
{
    private const string CurrentScene = "TestFormationYard";
        
    private Transform _position1;
    private Transform _position2;
    private Transform _position3;
    private Transform _position4;
    private Transform _position5;
    private Transform _position6;

    private TargetPlacement _target;
    
    private GameObject _twoLevelFormationGameObject;
    private UsherWaiterFormationAgent _usherWaiterFormationAgent;
    private SeekSteeringBehavior _seekSteeringBehavior;
    
    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // Clean up any existing objects first
        _twoLevelFormationGameObject = null;
        _usherWaiterFormationAgent = null;
        _seekSteeringBehavior = null;
        _target = null;

        // Load the test scene
        yield return TestLevelManagement.ReLoadScene(CurrentScene);
        yield return null;
        
        GameObject.Find("ScalableFormationGroup").SetActive(false);
        GameObject.Find("TwoLevelFormationGroup").SetActive(true);
        GameObject.Find("FixedFormationGroup").SetActive(false);
        
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
        
        if (_target == null)
        {
            _target = GameObject.Find("Target").GetComponent<TargetPlacement>();
            _target.TargetPosition = _position1.position;
            _target.Enabled = false;
        }

        if (_twoLevelFormationGameObject == null)
        {
            _twoLevelFormationGameObject = GameObject.Find("UsherWaiterFormationAgent");
            _twoLevelFormationGameObject.SetActive(false);
        }

        if (_usherWaiterFormationAgent == null)
        {
            _usherWaiterFormationAgent = _twoLevelFormationGameObject.GetComponent<UsherWaiterFormationAgent>();
        }

        if (_seekSteeringBehavior == null)
        {
            _seekSteeringBehavior = _twoLevelFormationGameObject.GetComponentInChildren<SeekSteeringBehavior>();
        }
    }
    
    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (_twoLevelFormationGameObject != null)
            _twoLevelFormationGameObject.SetActive(false);
        if (_target != null)
            _target.Enabled = false;

        yield return null;
    }
    
    /// <summary>
    /// Test that formation can make a full turn in unrealistic mode.
    /// </summary>
    [UnityTest]
    public IEnumerator TwoLevelFormationNonRealisticTurnDownUpTest()
    {
        // Setup agents before the tests.
        _target.Enabled = true;
        _target.TargetPosition = _position2.position;
        _twoLevelFormationGameObject.transform.position = _position1.position;
        _usherWaiterFormationAgent.MaximumSpeed = 2.0f;
        _usherWaiterFormationAgent.StopSpeed = 0.1f;
        _usherWaiterFormationAgent.MaximumRotationalSpeed = 360f;
        _usherWaiterFormationAgent.StopRotationThreshold = 1f;
        _seekSteeringBehavior.Target = _target.gameObject;
        _seekSteeringBehavior.ArrivalDistance = 0.3f;

        _twoLevelFormationGameObject.SetActive(true);
        
        _usherWaiterFormationAgent.RealisticTurns = false;
        
        // Start test.
        
        // Assert that formation reached its target.
        yield return new WaitForSeconds(7f);
        Assert.True(Vector2.Distance(
            _twoLevelFormationGameObject.transform.position, 
            _position2.transform.position) < 0.5f);
        
        // Move the target to another position.
        _target.transform.position = _position3.transform.position;
        
        // Assert that formation reached its target.
        yield return new WaitForSeconds(12f);
        Assert.True(Vector2.Distance(
            _twoLevelFormationGameObject.transform.position, 
            _position3.transform.position) < 0.5f);
        
        // Move the target to another position.
        _target.transform.position = _position4.transform.position;
        
        // Assert that formation reached its target.
        yield return new WaitForSeconds(7f);
        Assert.True(Vector2.Distance(
            _twoLevelFormationGameObject.transform.position, 
            _position4.transform.position) < 0.5f);
    }
    
    /// <summary>
    /// Test that formation can make a full turn in realistic mode.
    /// </summary>
    [UnityTest]
    public IEnumerator TwoLevelFormationRealisticTurnDownUpTest()
    {
        // Setup agents before the tests.
        _target.Enabled = true;
        _target.TargetPosition = _position2.position;
        _twoLevelFormationGameObject.transform.position = _position1.position;
        _usherWaiterFormationAgent.MaximumSpeed = 2.0f;
        _usherWaiterFormationAgent.StopSpeed = 0.1f;
        _usherWaiterFormationAgent.MaximumRotationalSpeed = 360f;
        _usherWaiterFormationAgent.StopRotationThreshold = 1f;
        _seekSteeringBehavior.Target = _target.gameObject;
        _seekSteeringBehavior.ArrivalDistance = 0.3f;

        _usherWaiterFormationAgent.RealisticTurns = true;
        
        _twoLevelFormationGameObject.SetActive(true);
        
        // Start test.
        // Assert that formation reached its target.
        yield return new WaitForSeconds(12f);
        Assert.True(Vector2.Distance(
            _twoLevelFormationGameObject.transform.position, 
            _position2.transform.position) < 0.5f);
        
        // Move the target to another position.
        _target.transform.position = _position3.transform.position;
        
        // Assert that formation reached its target.
        yield return new WaitForSeconds(15f);
        Assert.True(Vector2.Distance(
            _twoLevelFormationGameObject.transform.position, 
            _position3.transform.position) < 0.5f);
        
        // Move the target to another position.
        _target.transform.position = _position4.transform.position;
        
        // Assert that formation reached its target.
        yield return new WaitForSeconds(14f);
        Assert.True(Vector2.Distance(
            _twoLevelFormationGameObject.transform.position, 
            _position4.transform.position) < 0.5f);
    }
    
    /// <summary>
    /// Test that formation can move across obstacles.
    /// </summary>
    [UnityTest]
    public IEnumerator TwoLevelFormationMovementAcrossObstaclesTest()
    {
        // Setup agents before the tests.
        _target.Enabled = true;
        _target.TargetPosition = _position6.position;
        _twoLevelFormationGameObject.transform.position = _position1.position;
        _usherWaiterFormationAgent.MaximumSpeed = 2.0f;
        _usherWaiterFormationAgent.StopSpeed = 0.1f;
        _usherWaiterFormationAgent.MaximumRotationalSpeed = 360f;
        _usherWaiterFormationAgent.StopRotationThreshold = 1f;
        _seekSteeringBehavior.Target = _target.gameObject;
        _seekSteeringBehavior.ArrivalDistance = 0.3f;

        _twoLevelFormationGameObject.SetActive(true);
        
        _usherWaiterFormationAgent.RealisticTurns = false;
        
        // Start test.
        
        // Assert that formation reached its target.
        yield return new WaitForSeconds(18f);
        Assert.True(Vector2.Distance(
            _twoLevelFormationGameObject.transform.position, 
            _position6.transform.position) < 0.5f);
        
        // Assert that members reached their ushers.
        for (int i=0; i < _usherWaiterFormationAgent.Formation.Members.Count; i++) 
        {
            GameObject member = _usherWaiterFormationAgent.Formation.Members[i];
            Vector2 usherPosition = _usherWaiterFormationAgent.transform.TransformPoint(
                _usherWaiterFormationAgent.Formation.MemberPositions[i]);
            Assert.True(
                Vector2.Distance(member.transform.position, usherPosition) < 50f
            );
        }
        
        // Move the target to another position.
        _target.transform.position = _position5.transform.position;
        
        // Assert that formation reached its target.
        yield return new WaitForSeconds(28f);
        Assert.True(Vector2.Distance(
            _twoLevelFormationGameObject.transform.position, 
            _position5.transform.position) < 0.5f);
        
        // Assert that members reached their ushers.
        for (int i=0; i < _usherWaiterFormationAgent.Formation.Members.Count; i++) 
        {
            GameObject member = _usherWaiterFormationAgent.Formation.Members[i];
            Vector2 usherPosition = _usherWaiterFormationAgent.transform.TransformPoint(
                _usherWaiterFormationAgent.Formation.MemberPositions[i]);
            Assert.True(
                Vector2.Distance(member.transform.position, usherPosition) < 50f
            );
        }
    }
}
}