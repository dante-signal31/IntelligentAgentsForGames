using System.Collections;
using System.Linq;
using NUnit.Framework;
using Tests.PlayTests.Common;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.PlayTests
{
    public class SimpleBehaviourTests
    {
        private string _currentScene = "TestClearTiledYard";

        private Transform _seekStartPosition;
        private Transform _alignStartPosition;
        private Transform _faceStartPosition;
        private Transform _fleeStartPosition;
        private Transform _pursuitStartPosition;
        private Transform _pursuitTargetStartPosition;
        private Transform _targetPosition;
        private Transform _targetPosition2;
        private Transform _targetPosition3;
        private Transform _targetPosition4;

        private TargetPlacement _target;

        private GameObject _seekGameObject;
        private GameObject _alignGameObject;
        private GameObject _faceGameObject;
        private GameObject _fleeGameObject;
        private GameObject _arriveGameObject;
        private GameObject _pursuitGameObject;
        private GameObject _evadeGameObject;
        private GameObject _velocityMatchingGameObject;
    
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return TestLevelManagement.ReLoadScene(_currentScene);

            if (_targetPosition == null)
                _targetPosition = GameObject.Find("TargetPosition").transform;
            if (_targetPosition2 == null)
                _targetPosition2 = GameObject.Find("TargetPosition2").transform;
            if (_targetPosition3 == null)
                _targetPosition3 = GameObject.Find("TargetPosition3").transform;
            if (_targetPosition4 == null)
                _targetPosition4 = GameObject.Find("TargetPosition4").transform;
            if (_target == null)
            {
                _target = GameObject.Find("Target").GetComponent<TargetPlacement>();
                _target.TargetPosition = _targetPosition.position;
                _target.Enabled = false;
            }
            
            if (_seekStartPosition == null) 
                _seekStartPosition = GameObject.Find("SeekStartPosition").transform;
            // if (_alignStartPosition == null) 
            //     _alignStartPosition = GameObject.Find("AlignStartPosition").transform;
            // if (_faceStartPosition == null) 
            //     _faceStartPosition = GameObject.Find("FaceStartPosition").transform;
            // if (_fleeStartPosition == null) 
            //     _fleeStartPosition = GameObject.Find("FleeStartPosition").transform;
            // if (_pursuitStartPosition == null)
            //     _pursuitStartPosition = GameObject.Find("PursuitStartPosition").transform;
            // if (_pursuitTargetStartPosition == null)
            //     _pursuitTargetStartPosition = GameObject.Find("PursuitTargetStartPosition").transform;
            
            if (_seekGameObject == null)
            {
                _seekGameObject = GameObject.Find("SeekMovingAgent");
                _seekGameObject.SetActive(false);
            }
            // if (_alignGameObject == null)
            // {
            //     _alignGameObject = GameObject.Find("AlignMovingAgent");
            //     _alignGameObject.SetActive(false);
            // }
            // if (_faceGameObject == null)
            // {
            //     _faceGameObject = GameObject.Find("FaceMovingAgent");
            //     _faceGameObject.SetActive(false);
            // }
            // if (_fleeGameObject == null)
            // {
            //     _fleeGameObject = GameObject.Find("FleeMovingAgent");
            //     _fleeGameObject.SetActive(false);
            // }
            if (_arriveGameObject == null)
            {
                _arriveGameObject = GameObject.Find("ArriveMovingAgent");
                _arriveGameObject.SetActive(false);
            }
            // if (_pursuitGameObject == null)
            // {
            //     _pursuitGameObject = GameObject.Find("PursuitMovingAgent");
            //     _pursuitGameObject.SetActive(false);
            // }
            // if (_evadeGameObject == null)
            // {
            //     _evadeGameObject = GameObject.Find("EvadeMovingAgent");
            //     _evadeGameObject.SetActive(false);
            // }
            // if (_velocityMatchingGameObject == null)
            // {
            //     _velocityMatchingGameObject = GameObject.Find("VelocityMatchingMovingAgent");
            //     _velocityMatchingGameObject.SetActive(false);
            // }
        }
        
        /// <summary>
        /// Test that SeekBehavior can reach a target.
        /// </summary>
        [UnityTest]
        public IEnumerator SeekBehaviourTest()
        {
            // Test setup.
            _seekGameObject.transform.position = _seekStartPosition.position;
            var seekSteeringBehavior = _seekGameObject.GetComponent<SeekSteeringBehavior>();
            var agentMover = _seekGameObject.GetComponent<AgentMover>();
            agentMover.MaximumSpeed = 10.0f;
            _target.Enabled = true;
            _target.TargetPosition = _targetPosition.position;
            seekSteeringBehavior.Target = _target.gameObject;
            seekSteeringBehavior.ArrivalDistance = 0.2f;
            _seekGameObject.SetActive(true);
            
            // Give time for the seeker to get to the target.
            yield return new WaitForSeconds(2.0f);
            
            // Assert the target was reached.
            Assert.True(Vector3.Distance(_targetPosition.position, 
                _seekGameObject.transform.position) <= 
                        (seekSteeringBehavior.ArrivalDistance + 0.1));
            
            // Cleanup.
            _seekGameObject.SetActive(false);
            _target.Enabled = false;
        }
        
    /// <summary>
    /// Test that ArriveBehavior can reach a target and that it accelerates
    /// at the beginning and brakes at the end.
    /// </summary>
    [UnityTest]
    public IEnumerator ArriveBehaviourTest()
    {
        // Test setup.
        _arriveGameObject.transform.position = _seekStartPosition.position;
        var arriveSteeringBehavior = _arriveGameObject.GetComponent<ArriveSteeringBehavior>();
        var agentMover = _arriveGameObject.GetComponent<AgentMover>();
        agentMover.MaximumSpeed = 10.0f;
        _target.Enabled = true;
        _target.TargetPosition = _targetPosition.position;
        arriveSteeringBehavior.Target = _target.gameObject;
        arriveSteeringBehavior.ArrivingRadius = 0.2f;
        _arriveGameObject.SetActive(true);
        
        // Check that arriver is accelerating at the beginning.
        yield return new WaitUntil(() =>
            (Vector3.Distance(_seekStartPosition.position, 
                _arriveGameObject.transform.position) >= 0.1f) && 
            (Vector3.Distance(_seekStartPosition.position, 
            _arriveGameObject.transform.position) < 
                arriveSteeringBehavior.AccelerationRadius));
        Assert.True(agentMover.CurrentSpeed > 0.0f && 
                    agentMover.CurrentSpeed < agentMover.MaximumSpeed);
        
        // Check that arriver gets its full cruise speed. 
        yield return new WaitUntil(() =>
            Vector3.Distance(_seekStartPosition.position, 
                _arriveGameObject.transform.position) > 
            (arriveSteeringBehavior.AccelerationRadius + 0.1f));
        yield return null;
        yield return null;
        Assert.True(
            Mathf.Approximately(agentMover.CurrentSpeed, 
                agentMover.MaximumSpeed));
        
        // Check that arriver is braking at the end.
        yield return new WaitUntil(() =>
            Vector3.Distance(_targetPosition.position, 
                _arriveGameObject.transform.position) < 
                (arriveSteeringBehavior.BrakingRadius - 0.2f));
        yield return null;
        yield return null;
        Assert.True(agentMover.CurrentSpeed > 0.0f && 
                    agentMover.CurrentSpeed < agentMover.MaximumSpeed);
    
        // Assert the target was reached.
        yield return new WaitForSeconds(0.75f);
        Assert.True(Vector3.Distance(_targetPosition.position, 
                        _arriveGameObject.transform.position) <= 
                    (arriveSteeringBehavior.ArrivingRadius));
        
        // Cleanup.
        _arriveGameObject.SetActive(false);
        _target.Enabled = false;
    }
    //     
    //     /// <summary>
    //     /// Test that FleeBehavior males game object go away from its threath.
    //     /// </summary>
    //     [UnityTest]
    //     public IEnumerator FleeBehaviourTest()
    //     {
    //         // Test setup.
    //         _fleeGameObject.transform.position = _fleeStartPosition.position;
    //         var fleeSteeringBehavior = _fleeGameObject.GetComponent<FleeSteeringBehavior>();
    //         fleeSteeringBehavior.Threath = _target.gameObject;
    //         fleeSteeringBehavior.PanicDistance = 5.0f;
    //         var agentMover = _fleeGameObject.GetComponent<AgentMover>();
    //         agentMover.MaximumSpeed = 10.0f;
    //         _target.Enabled = true;
    //         _target.TargetPosition = _targetPosition.position;
    //         _fleeGameObject.SetActive(true);
    //         
    //         // Give time for the seeker to get to the target.
    //         int testsSamples = 5;
    //         for (int i = 0; i < testsSamples; i++)
    //         {
    //             // Place target in a random position inside the flee range.
    //             _target.TargetPosition = _fleeGameObject.transform.position +
    //                                      (Vector3)(Random.insideUnitCircle *
    //                                      fleeSteeringBehavior.PanicDistance);
    //             // Let the fleer go away from the target.
    //             yield return new WaitForSeconds(1.0f);
    //             // Assert fleer is now farther from the target than before.
    //             Assert.True(Vector3.Distance(_targetPosition.position, 
    //                             _fleeGameObject.transform.position) > 
    //                        fleeSteeringBehavior.PanicDistance);
    //         }
    //         
    //         // Cleanup.
    //         _fleeGameObject.SetActive(false);
    //         _target.Enabled = false;
    //     }
    //     
    //     /// <summary>
    //     /// Test that AlignBehavior can face in the same direction as other GameObject.
    //     /// </summary>
    //     [UnityTest]
    //     public IEnumerator AlignBehaviourTest()
    //     {
    //         // Test setup.
    //         _seekGameObject.transform.position = _seekStartPosition.position;
    //         var seekSteeringBehavior = _seekGameObject.GetComponent<SeekSteeringBehavior>();
    //         var agentMover = _seekGameObject.GetComponent<AgentMover>();
    //         agentMover.MaximumSpeed = 2.0f;
    //         _target.Enabled = true;
    //         _target.TargetPosition = _targetPosition.position;
    //         seekSteeringBehavior.Target = _target.gameObject;
    //         seekSteeringBehavior.ArrivalDistance = 0.2f;
    //         _alignGameObject.transform.position = _alignStartPosition.position;
    //         var alignSteeringBehavior = _alignGameObject.GetComponent<AlignSteeringBehavior>();
    //         alignSteeringBehavior.Target = _seekGameObject;
    //         _alignGameObject.SetActive(true);
    //         _seekGameObject.SetActive(true);
    //         
    //         // Move seeker to face the first target.
    //         _target.TargetPosition = _targetPosition.position;
    //         yield return new WaitForSeconds(2.0f);
    //         Assert.True(
    //             Mathf.Abs(
    //                 _seekGameObject.GetComponent<AgentMover>().Orientation-
    //                  _alignGameObject.GetComponent<AgentMover>().Orientation) <= 
    //                     alignSteeringBehavior.ArrivingMargin);
    //         
    //         // Move seeker to face the second target.
    //         _target.TargetPosition = _targetPosition2.position;
    //         yield return new WaitForSeconds(2.0f);
    //         Assert.True(
    //             Mathf.Abs(
    //                 _seekGameObject.GetComponent<AgentMover>().Orientation- 
    //                   _alignGameObject.GetComponent<AgentMover>().Orientation) <=
    //                     alignSteeringBehavior.ArrivingMargin);
    //         
    //         // Move seeker to face the third target.
    //         _target.TargetPosition = _targetPosition3.position;
    //         yield return new WaitForSeconds(2.0f);
    //         Assert.True(
    //             Mathf.Abs(
    //                 _seekGameObject.GetComponent<AgentMover>().Orientation- 
    //                   _alignGameObject.GetComponent<AgentMover>().Orientation) <=
    //                   alignSteeringBehavior.ArrivingMargin);
    //         
    //         // Cleanup.
    //         _seekGameObject.SetActive(false);
    //         _alignGameObject.SetActive(false);
    //         _target.Enabled = false;
    //     }
    //     
    //     /// <summary>
    //     /// Test that FaceBehavior can face towards a target while it moves.
    //     /// </summary>
    //     [UnityTest]
    //     public IEnumerator FaceBehaviourTest()
    //     {
    //         // Test setup.
    //         _seekGameObject.transform.position = _seekStartPosition.position;
    //         var seekSteeringBehavior = _seekGameObject.GetComponent<SeekSteeringBehavior>();
    //         var agentMover = _seekGameObject.GetComponent<AgentMover>();
    //         agentMover.MaximumSpeed = 2.0f;
    //         _target.Enabled = true;
    //         _target.TargetPosition = _targetPosition.position;
    //         seekSteeringBehavior.Target = _target.gameObject;
    //         seekSteeringBehavior.ArrivalDistance = 0.2f;
    //         _faceGameObject.transform.position = _faceStartPosition.position;
    //         var faceSteeringBehavior = _faceGameObject.GetComponent<FaceMatchingSteeringBehavior>();
    //         faceSteeringBehavior.Target = _seekGameObject;
    //         _faceGameObject.SetActive(true);
    //         _seekGameObject.SetActive(true);
    //         
    //         // Sample the tested agent alignment in some moments of the
    //         // seeker travel, to check if it is still facing the seeker.
    //         float totalTestTimeInSeconds = 5.0f;
    //         int numberOfSamples = 5;
    //         float sampleInterval = totalTestTimeInSeconds / numberOfSamples;
    //         foreach (int _ in Enumerable.Range(1, numberOfSamples))
    //         {
    //             yield return new WaitForSeconds(sampleInterval);
    //             float currentAngle = Vector3.Angle(Vector3.up,
    //                 _seekGameObject.transform.position -
    //                 _faceGameObject.transform.position);
    //             // 5 degrees tolerance, because target is constantly moving.
    //             Assert.True(
    //                 Mathf.Abs(
    //                     currentAngle - 
    //                     _faceGameObject.GetComponent<AgentMover>().Orientation
    //                     ) <= 5); 
    //         }
    //         
    //         // Cleanup.
    //         _seekGameObject.SetActive(false);
    //         _faceGameObject.SetActive(false);
    //         _target.Enabled = false;
    //     }
    //     
    //     /// <summary>
    //     /// Test that PursuitBehavior can intercept its target.
    //     /// </summary>
    //     [UnityTest]
    //     public IEnumerator PursuitBehaviourTest()
    //     {
    //         // Test setup.
    //         _seekGameObject.transform.position = _pursuitTargetStartPosition.position;
    //         var seekSteeringBehavior = _seekGameObject.GetComponent<SeekSteeringBehavior>();
    //         var agentMover = _seekGameObject.GetComponent<AgentMover>();
    //         agentMover.MaximumSpeed = 2.0f;
    //         _target.Enabled = true;
    //         _target.TargetPosition = _targetPosition4.position;
    //         seekSteeringBehavior.Target = _target.gameObject;
    //         seekSteeringBehavior.ArrivalDistance = 0.2f;
    //         _pursuitGameObject.transform.position = _pursuitStartPosition.position;
    //         var pursuitSteeringBehavior = _pursuitGameObject.GetComponent<PursuitSteeringBehavior>();
    //         var pursuitAgentMover = _seekGameObject.GetComponent<AgentMover>();
    //         pursuitAgentMover.MaximumSpeed = 2.0f;
    //         pursuitSteeringBehavior.Target = _seekGameObject;
    //         _seekGameObject.SetActive(true);
    //         _pursuitGameObject.SetActive(true);
    //         
    //         // Give time for the chaser to get to the target.
    //         yield return new WaitForSeconds(3.0f);
    //         
    //         // Assert the target was reached.
    //         // We test for a distance equal to the radius of both agents, plus
    //         // a 0.1 of tolerance. That should be the distance of centers when
    //         // both agents are touching.
    //         Assert.True(Vector3.Distance(_seekGameObject.transform.position, 
    //                         _pursuitGameObject.transform.position) <= (1.1f));
    //         
    //         // Cleanup.
    //         _seekGameObject.SetActive(false);
    //         _pursuitGameObject.SetActive(false);
    //         _target.Enabled = false;
    //     }
    //     
    //     /// <summary>
    //     /// Test that EvadeBehavior can can keep away its agent from its chaser.
    //     /// </summary>
    //     [UnityTest]
    //     public IEnumerator EvadeBehaviourTest()
    //     {
    //         // Test setup.
    //         _evadeGameObject.transform.position = _fleeStartPosition.position;
    //         var evadeSteeringBehavior = _evadeGameObject.GetComponent<EvadeSteeringBehavior>();
    //         var evadeAgentMover = _evadeGameObject.GetComponent<AgentMover>();
    //         _seekGameObject.transform.position = _seekStartPosition.position;
    //         var seekSteeringBehavior = _seekGameObject.GetComponent<SeekSteeringBehavior>();
    //         var seekAgentMover = _seekGameObject.GetComponent<AgentMover>();
    //         seekAgentMover.MaximumSpeed = 2.0f;
    //         evadeAgentMover.MaximumSpeed = 2.0f;
    //         seekSteeringBehavior.Target = _evadeGameObject;
    //         evadeSteeringBehavior.Threath = _seekGameObject;
    //         evadeSteeringBehavior.PanicDistance = 3.0f;
    //         _evadeGameObject.SetActive(true);
    //         _seekGameObject.SetActive(true);
    //         
    //         // Give time for the chaser to try to reach evader.
    //         yield return new WaitForSeconds(4.0f);
    //         
    //         // Assert the evader was not reached.
    //         Assert.True(Vector3.Distance(_seekGameObject.transform.position, 
    //             _evadeGameObject.transform.position) >= (2.0f));
    //         
    //         // Cleanup.
    //         _seekGameObject.SetActive(false);
    //         _evadeGameObject.SetActive(false);
    //         _target.Enabled = false;
    //     }
    //     
    //     /// <summary>
    //     /// Test that VelocityMatchingBehavior can can copy its target's velocity.
    //     /// </summary>
    //     [UnityTest]
    //     public IEnumerator VelocityMatchingBehaviourTest()
    //     {
    //         // Test setup.
    //         _velocityMatchingGameObject.transform.position = _pursuitTargetStartPosition.position;
    //         var velocityMatchingSteeringBehavior = _velocityMatchingGameObject.GetComponent<VelocityMatchingSteeringBehavior>();
    //         var velocityMatchingAgentMover = _velocityMatchingGameObject.GetComponent<AgentMover>();
    //         _arriveGameObject.transform.position = _alignStartPosition.position;
    //         var arriveSteeringBehavior = _arriveGameObject.GetComponent<ArriveSteeringBehavior>();
    //         var arriveAgentMover = _arriveGameObject.GetComponent<AgentMover>();
    //         velocityMatchingAgentMover.MaximumSpeed = 2.0f;
    //         velocityMatchingAgentMover.MaximumAcceleration = 4.0f;
    //         velocityMatchingAgentMover.StopSpeed = 0.1f;
    //         arriveAgentMover.MaximumSpeed = 2.0f;
    //         velocityMatchingSteeringBehavior.Target = _arriveGameObject;
    //         arriveSteeringBehavior.Target = _targetPosition.gameObject;
    //         _velocityMatchingGameObject.SetActive(true);
    //         _arriveGameObject.SetActive(true);
    //         
    //         // Give time for the VelocityMatcher to try to reach its target
    //         // cruise Velocity and assert its velocity has matched the one
    //         // of its target.
    //         yield return new WaitUntil(() =>
    //             Mathf.Approximately(
    //                 arriveAgentMover.CurrentSpeed, 
    //                 arriveAgentMover.MaximumSpeed));
    //         yield return new WaitForSeconds(velocityMatchingSteeringBehavior.TimeToMatch);
    //         Assert.True(velocityMatchingAgentMover.Velocity == arriveAgentMover.Velocity);
    //         
    //         // Wait until arriver brakes and asserts that the VelocityMatcher
    //         // has braked to.
    //         yield return new WaitUntil(() => 
    //             Mathf.Approximately(arriveAgentMover.CurrentSpeed, 0));
    //         yield return new WaitForSeconds(velocityMatchingSteeringBehavior.TimeToMatch);
    //         Assert.True(velocityMatchingAgentMover.Velocity == arriveAgentMover.Velocity);
    //
    //         // Cleanup.
    //         _velocityMatchingGameObject.SetActive(false);
    //         _arriveGameObject.SetActive(false);
    //     }
     }
}
