using System.Collections;
using Groups;
using NUnit.Framework;
using SteeringBehaviors;
using Tests.PlayTests.Common;
using Tools;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.PlayTests
{
public class ScalableFormationTests
{
    private const string CurrentScene = "TestFormationYard";

    private Transform _position1;
    private Transform _position2;
    private Transform _position3;
    private Transform _position4;

    private Target _target;

    private GameObject _scalableFormationGameObject;
    private UsherFormationAgent _usherScalableFormationAgent;
    private SeekSteeringBehavior _seekSteeringBehavior;
    private ScalableFormation _scalableFormation;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // Clean up any existing objects first
        _scalableFormationGameObject = null;
        _usherScalableFormationAgent = null;
        _seekSteeringBehavior = null;
        _scalableFormation = null;
        _target = null;

        // Load the test scene
        yield return TestLevelManagement.ReLoadScene(CurrentScene);
        yield return null;
        
        GameObject.Find("ScalableFormationGroup").SetActive(true);
        GameObject.Find("TwoLevelFormationGroup").SetActive(false);
        GameObject.Find("FixedFormationGroup").SetActive(false);

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

        if (_scalableFormationGameObject == null)
        {
            _scalableFormationGameObject =
                GameObject.Find("UsherScalableFormationAgent");
            _scalableFormationGameObject.SetActive(false);
        }

        if (_usherScalableFormationAgent == null)
        {
            _usherScalableFormationAgent = _scalableFormationGameObject
                .GetComponent<UsherFormationAgent>();
        }

        if (_seekSteeringBehavior == null)
        {
            _seekSteeringBehavior = _scalableFormationGameObject
                .GetComponentInChildren<SeekSteeringBehavior>();
        }
        
        if (_scalableFormation == null)
        {
            _scalableFormation = _scalableFormationGameObject.GetComponentInChildren<ScalableFormation>();
        }
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (_scalableFormationGameObject != null)
            _scalableFormationGameObject.SetActive(false);
        if (_target != null)
            _target.Enabled = false;

        yield return null;
    }

    /// <summary>
    /// Test that formation can make a full turn in unrealistic mode.
    /// </summary>
    [UnityTest]
    public IEnumerator ScalableFormationNonRealisticTurnDownUpTest()
    {
        // Setup agents before the tests.
        _target.Enabled = true;
        _target.TargetPosition = _position2.position;
        _scalableFormationGameObject.transform.position = _position1.position;
        _usherScalableFormationAgent.MaximumSpeed = 2.0f;
        _usherScalableFormationAgent.StopSpeed = 0.1f;
        _usherScalableFormationAgent.MaximumRotationalSpeed = 360f;
        _usherScalableFormationAgent.StopRotationThreshold = 1f;
        _seekSteeringBehavior.Target = _target.gameObject;
        _seekSteeringBehavior.ArrivalDistance = 0.3f;

        _usherScalableFormationAgent.RealisticTurns = false;

        _scalableFormationGameObject.SetActive(true);

        // Start test.

        // Assert that formation reached its target.
        yield return new WaitForSeconds(7f);
        Assert.True(Vector2.Distance(
            _scalableFormationGameObject.transform.position,
            _position2.transform.position) < 0.5f);

        // Move the target to another position.
        _target.transform.position = _position3.transform.position;

        // Assert that formation reached its target.
        yield return new WaitForSeconds(10f);
        Assert.True(Vector2.Distance(
            _scalableFormationGameObject.transform.position,
            _position3.transform.position) < 0.5f);

        // Move the target to another position.
        _target.transform.position = _position4.transform.position;

        // Assert that formation reached its target.
        yield return new WaitForSeconds(5f);
        Assert.True(Vector2.Distance(
            _scalableFormationGameObject.transform.position,
            _position4.transform.position) < 0.5f);
    }

    /// <summary>
    /// Test that formation can make a full turn in realistic mode.
    /// </summary>
    [UnityTest]
    public IEnumerator ScalableFormationRealisticTurnDownUpTest()
    {
        // Setup agents before the tests.
        _target.Enabled = true;
        _target.TargetPosition = _position2.position;
        _scalableFormationGameObject.transform.position = _position1.position;
        _usherScalableFormationAgent.MaximumSpeed = 2.0f;
        _usherScalableFormationAgent.StopSpeed = 0.1f;
        _usherScalableFormationAgent.MaximumRotationalSpeed = 360f;
        _usherScalableFormationAgent.StopRotationThreshold = 1f;
        _seekSteeringBehavior.Target = _target.gameObject;
        _seekSteeringBehavior.ArrivalDistance = 0.3f;

        _usherScalableFormationAgent.RealisticTurns = true;

        _scalableFormationGameObject.SetActive(true);

        // Start test.
        // Assert that formation reached its target.
        yield return new WaitForSeconds(10f);
        Assert.True(Vector2.Distance(
            _scalableFormationGameObject.transform.position,
            _position2.transform.position) < 0.5f);

        // Move the target to another position.
        _target.transform.position = _position3.transform.position;

        // Assert that formation reached its target.
        yield return new WaitForSeconds(15f);
        Assert.True(Vector2.Distance(
            _scalableFormationGameObject.transform.position,
            _position3.transform.position) < 0.5f);

        // Move the target to another position.
        _target.transform.position = _position4.transform.position;

        // Assert that formation reached its target.
        yield return new WaitForSeconds(12f);
        Assert.True(Vector2.Distance(
            _scalableFormationGameObject.transform.position,
            _position4.transform.position) < 0.5f);
    }

    /// <summary>
    /// Test that formation can make a full turn in unrealistic mode.
    /// </summary>
    [UnityTest]
    public IEnumerator ScalableFormationNonRealisticTurnUpDownTest()
    {
        // Setup agents before the tests.
        _target.Enabled = true;
        _target.TargetPosition = _position3.position;
        _scalableFormationGameObject.transform.position = _position4.position;
        _usherScalableFormationAgent.MaximumSpeed = 2.0f;
        _usherScalableFormationAgent.StopSpeed = 0.1f;
        _usherScalableFormationAgent.MaximumRotationalSpeed = 360f;
        _usherScalableFormationAgent.StopRotationThreshold = 1f;
        _seekSteeringBehavior.Target = _target.gameObject;
        _seekSteeringBehavior.ArrivalDistance = 0.3f;

        _usherScalableFormationAgent.RealisticTurns = false;

        _scalableFormationGameObject.SetActive(true);

        // Start test.

        // Assert that formation reached its target.
        yield return new WaitForSeconds(5f);
        Assert.True(Vector2.Distance(
            _scalableFormationGameObject.transform.position,
            _position3.transform.position) < 0.5f);

        // Move the target to another position.
        _target.transform.position = _position2.transform.position;

        // Assert that formation reached its target.
        yield return new WaitForSeconds(10f);
        Assert.True(Vector2.Distance(
            _scalableFormationGameObject.transform.position,
            _position2.transform.position) < 0.5f);

        // Move the target to another position.
        _target.transform.position = _position1.transform.position;

        // Assert that formation reached its target.
        yield return new WaitForSeconds(7f);
        Assert.True(Vector2.Distance(
            _scalableFormationGameObject.transform.position,
            _position1.transform.position) < 0.5f);
    }

    /// <summary>
    /// Test that formation can make a full turn in realistic mode.
    /// </summary>
    [UnityTest]
    public IEnumerator ScalableFormationRealisticTurnUpDownTest()
    {
        // Setup agents before the tests.
        _target.Enabled = true;
        _target.TargetPosition = _position3.position;
        _scalableFormationGameObject.transform.position = _position4.position;
        _usherScalableFormationAgent.MaximumSpeed = 2.0f;
        _usherScalableFormationAgent.StopSpeed = 0.1f;
        _usherScalableFormationAgent.MaximumRotationalSpeed = 360f;
        _usherScalableFormationAgent.StopRotationThreshold = 1f;
        _seekSteeringBehavior.Target = _target.gameObject;
        _seekSteeringBehavior.ArrivalDistance = 0.3f;

        _usherScalableFormationAgent.RealisticTurns = true;

        _scalableFormationGameObject.SetActive(true);

        // Start test.
        // Assert that formation reached its target.
        yield return new WaitForSeconds(12f);
        Assert.True(Vector2.Distance(
            _scalableFormationGameObject.transform.position,
            _position3.transform.position) < 0.5f);

        // Move the target to another position.
        _target.transform.position = _position2.transform.position;

        // Assert that formation reached its target.
        yield return new WaitForSeconds(15f);
        Assert.True(Vector2.Distance(
            _scalableFormationGameObject.transform.position,
            _position2.transform.position) < 0.5f);

        // Move the target to another position.
        _target.transform.position = _position1.transform.position;

        // Assert that formation reached its target.
        yield return new WaitForSeconds(10f);
        Assert.True(Vector2.Distance(
            _scalableFormationGameObject.transform.position,
            _position1.transform.position) < 0.5f);
    }
    
    /// <summary>
    /// Test that formation quantity and dimensions can be changed in realtime coherently.
    /// </summary>
    [UnityTest]
    public IEnumerator ScalableFormationQuantityAndDimensionsDefinedTest()
    {
        // Setup agents before the tests.
        _target.Enabled = true;
        _target.TargetPosition = _position2.position;
        _scalableFormationGameObject.transform.position = _position1.position;
        _usherScalableFormationAgent.MaximumSpeed = 2.0f;
        _usherScalableFormationAgent.StopSpeed = 0.1f;
        _usherScalableFormationAgent.MaximumRotationalSpeed = 360f;
        _usherScalableFormationAgent.StopRotationThreshold = 1f;
        _seekSteeringBehavior.Target = _target.gameObject;
        _seekSteeringBehavior.ArrivalDistance = 0.3f;

        _usherScalableFormationAgent.RealisticTurns = true;

        _scalableFormation.Distribution =
            ScalableFormation.DistributionType.QuantityAndDimensionsDefined;
        _scalableFormation.FormationDimensions = new Vector2(8, 5);
        _scalableFormation.Quantity = 12;

        _scalableFormationGameObject.SetActive(true);

        // Start test.
        // Assert that formation reached its target.
        yield return new WaitForSeconds(10f);
        Assert.True(Vector2.Distance(
            _scalableFormationGameObject.transform.position,
            _position2.transform.position) < 0.5f);

        // Change Quantity and check that dimensions are kept.
        Assert.True(_scalableFormation.FormationDimensions == new Vector2(8, 5));
        Assert.True(_scalableFormation.Quantity == 12);
        _scalableFormation.Quantity = 10;
        yield return new WaitForSeconds(1f);
        Assert.True(_scalableFormation.Quantity == 10);
        Assert.True(_scalableFormation.FormationDimensions == new Vector2(8, 5));
        
        // Move the target to another position.
        _target.transform.position = _position3.transform.position;

        // Assert that formation reached its target.
        yield return new WaitForSeconds(15f);
        Assert.True(Vector2.Distance(
            _scalableFormationGameObject.transform.position,
            _position3.transform.position) < 0.5f);
        
        // Change Dimensions and check that quantity is kept.
        Assert.True(_scalableFormation.FormationDimensions == new Vector2(8, 5));
        Assert.True(_scalableFormation.Quantity == 10);
        _scalableFormation.FormationDimensions = new Vector2(7, 6);
        yield return new WaitForSeconds(1f);
        Assert.True(_scalableFormation.Quantity == 10);
        Assert.True(_scalableFormation.FormationDimensions == new Vector2(7, 6));

        // Move the target to another position.
        _target.transform.position = _position4.transform.position;

        // Assert that formation reached its target.
        yield return new WaitForSeconds(12f);
        Assert.True(Vector2.Distance(
            _scalableFormationGameObject.transform.position,
            _position4.transform.position) < 0.5f);
    }
    
    /// <summary>
    /// Test that formation density and dimensions can be changed in realtime coherently.
    /// </summary>
    [UnityTest]
    public IEnumerator ScalableFormationDensityAndDimensionsDefinedTest()
    {
        // Setup agents before the tests.
        _target.Enabled = true;
        _target.TargetPosition = _position2.position;
        _scalableFormationGameObject.transform.position = _position1.position;
        _usherScalableFormationAgent.MaximumSpeed = 2.0f;
        _usherScalableFormationAgent.StopSpeed = 0.1f;
        _usherScalableFormationAgent.MaximumRotationalSpeed = 360f;
        _usherScalableFormationAgent.StopRotationThreshold = 1f;
        _seekSteeringBehavior.Target = _target.gameObject;
        _seekSteeringBehavior.ArrivalDistance = 0.3f;

        _usherScalableFormationAgent.RealisticTurns = true;

        _scalableFormation.Distribution =
            ScalableFormation.DistributionType.DensityAndDimensionsDefined;
        _scalableFormation.FormationDimensions = new Vector2(8, 5);
        _scalableFormation.Density = new Vector2(2, 2);

        _scalableFormationGameObject.SetActive(true);

        // Start test.
        // Assert that formation reached its target.
        yield return new WaitForSeconds(10f);
        Assert.True(Vector2.Distance(
            _scalableFormationGameObject.transform.position,
            _position2.transform.position) < 0.5f);

        // Change Density and check that dimensions are kept.
        Assert.True(_scalableFormation.FormationDimensions == new Vector2(8, 5));
        Assert.True(_scalableFormation.Density == new Vector2(2, 2));
        _scalableFormation.Density = new Vector2(1.5f, 1.5f);
        yield return new WaitForSeconds(1f);
        Assert.True(_scalableFormation.Density == new Vector2(1.5f, 1.5f));
        Assert.True(_scalableFormation.FormationDimensions == new Vector2(8, 5));
        
        // Move the target to another position.
        _target.transform.position = _position3.transform.position;

        // Assert that formation reached its target.
        yield return new WaitForSeconds(15f);
        Assert.True(Vector2.Distance(
            _scalableFormationGameObject.transform.position,
            _position3.transform.position) < 0.5f);
        
        // Change Dimensions and check that density is kept.
        Assert.True(_scalableFormation.FormationDimensions == new Vector2(8, 5));
        Assert.True(_scalableFormation.Density == new Vector2(1.5f, 1.5f));
        _scalableFormation.FormationDimensions = new Vector2(7, 6);
        yield return new WaitForSeconds(1f);
        Assert.True(_scalableFormation.Density == new Vector2(1.5f, 1.5f));
        Assert.True(_scalableFormation.FormationDimensions == new Vector2(7, 6));

        // Move the target to another position.
        _target.transform.position = _position4.transform.position;

        // Assert that formation reached its target.
        yield return new WaitForSeconds(11f);
        Assert.True(Vector2.Distance(
            _scalableFormationGameObject.transform.position,
            _position4.transform.position) < 0.5f);
    }
    
    /// <summary>
    /// Test that formation density and quantity can be changed in realtime coherently.
    /// </summary>
    [UnityTest]
    public IEnumerator ScalableFormationDensityAndQuantityDefinedTest()
    {
        // Setup agents before the tests.
        _target.Enabled = true;
        _target.TargetPosition = _position2.position;
        _scalableFormationGameObject.transform.position = _position1.position;
        _usherScalableFormationAgent.MaximumSpeed = 2.0f;
        _usherScalableFormationAgent.StopSpeed = 0.1f;
        _usherScalableFormationAgent.MaximumRotationalSpeed = 360f;
        _usherScalableFormationAgent.StopRotationThreshold = 1f;
        _seekSteeringBehavior.Target = _target.gameObject;
        _seekSteeringBehavior.ArrivalDistance = 0.3f;

        _usherScalableFormationAgent.RealisticTurns = true;

        _scalableFormation.Distribution =
            ScalableFormation.DistributionType.DensityAndQuantityDefined;
        _scalableFormation.Quantity = 15;
        _scalableFormation.Density = new Vector2(2, 2);

        _scalableFormationGameObject.SetActive(true);

        // Start test.
        // Assert that formation reached its target.
        yield return new WaitForSeconds(10f);
        Assert.True(Vector2.Distance(
            _scalableFormationGameObject.transform.position,
            _position2.transform.position) < 0.5f);

        // Change Density and check that quantity is kept.
        Assert.True(_scalableFormation.Quantity == 15);
        Assert.True(_scalableFormation.Density == new Vector2(2, 2));
        _scalableFormation.Density = new Vector2(1.5f, 1.5f);
        yield return new WaitForSeconds(1f);
        Assert.True(_scalableFormation.Density == new Vector2(1.5f, 1.5f));
        Assert.True(_scalableFormation.Quantity == 15);
        
        // Move the target to another position.
        _target.transform.position = _position3.transform.position;

        // Assert that formation reached its target.
        yield return new WaitForSeconds(15f);
        Assert.True(Vector2.Distance(
            _scalableFormationGameObject.transform.position,
            _position3.transform.position) < 0.5f);
        
        // Change Quantity and check that density is kept.
        Assert.True(_scalableFormation.Quantity == 15);
        Assert.True(_scalableFormation.Density == new Vector2(1.5f, 1.5f));
        _scalableFormation.Quantity = 10;
        yield return new WaitForSeconds(1f);
        Assert.True(_scalableFormation.Density == new Vector2(1.5f, 1.5f));
        Assert.True(_scalableFormation.Quantity == 10);

        // Move the target to another position.
        _target.transform.position = _position4.transform.position;

        // Assert that formation reached its target.
        yield return new WaitForSeconds(11f);
        Assert.True(Vector2.Distance(
            _scalableFormationGameObject.transform.position,
            _position4.transform.position) < 0.5f);
    }
}
}
