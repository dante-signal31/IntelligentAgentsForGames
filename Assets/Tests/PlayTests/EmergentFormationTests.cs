using System.Collections;
using NUnit.Framework;
using SteeringBehaviors;
using Tests.PlayTests.Common;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.TestTools;

namespace Tests.PlayTests
{
public class EmergentFormationTests
{
    private const string CurrentScene = "TestEmergentFormationYard";
    
    private Transform _position1;
    private Transform _position2;
    private Transform _position3;
    private Transform _position4;
    private Transform _position5;
    private Transform _position6;
    private Transform _position7;
    private Transform _position8;
    private Transform _position9;
    
    private TargetPlacement _target;

    private GameObject _leaderGameObject;
    private GameObject _wingman1GameObject;
    private GameObject _wingman2GameObject;
    private GameObject _wingman3GameObject;
    private GameObject _wingman4GameObject;
    private GameObject _wingman5GameObject;
    
    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // Clean up any existing objects first
        _leaderGameObject = null;
        _wingman1GameObject = null;
        _wingman2GameObject = null;
        _wingman3GameObject = null;
        _wingman4GameObject = null;
        _wingman5GameObject = null;
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
        if (_target == null)
        {
            _target = GameObject.Find("Target").GetComponent<TargetPlacement>();
            _target.TargetPosition = _position1.position;
            _target.Enabled = false;
        }

        if (_leaderGameObject == null)
        {
            _leaderGameObject = GameObject.Find("EmergentFormationLeader");
            _leaderGameObject.SetActive(false);
        }

        if (_wingman1GameObject == null)
        {
            _wingman1GameObject = GameObject.Find("EmergentFormationWingman1");
            _wingman1GameObject.SetActive(false);
        }
        if (_wingman2GameObject == null)
        {
            _wingman2GameObject = GameObject.Find("EmergentFormationWingman2");
            _wingman2GameObject.SetActive(false);
        }
        if (_wingman3GameObject == null)
        {
            _wingman3GameObject = GameObject.Find("EmergentFormationWingman3");
            _wingman3GameObject.SetActive(false);
        }
        if (_wingman4GameObject == null)
        {
            _wingman4GameObject = GameObject.Find("EmergentFormationWingman4");
            _wingman4GameObject.SetActive(false);
        }
        if (_wingman5GameObject == null)
        {
            _wingman5GameObject = GameObject.Find("EmergentFormationWingman5");
            _wingman5GameObject.SetActive(false);
        }
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (_leaderGameObject != null)
            _leaderGameObject.SetActive(false);
        if (_wingman1GameObject != null)
            _wingman2GameObject.SetActive(false);
        if (_wingman2GameObject != null)
            _wingman2GameObject.SetActive(false);
        if (_wingman3GameObject != null)
            _wingman2GameObject.SetActive(false);
        if (_wingman4GameObject != null)
            _wingman2GameObject.SetActive(false);
        if (_wingman5GameObject != null)
            _wingman2GameObject.SetActive(false);
        if (_target != null)
            _target.Enabled = false;
        yield return null;
    }

    /// <summary>
    /// Test that EmergentFormation keep formation partners together while moving.
    /// </summary>
    [UnityTest]
    public IEnumerator EmergentFormationTest()
    {
        // TODO: Implement test.
        yield return null;
    }

}
}