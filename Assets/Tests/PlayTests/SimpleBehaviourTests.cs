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
        private string _currentScene = "ClearCourtyard";
        private string _seekPrefabPath = "Assets/Prefabs/SteeringBehaviors/SeekMovingAgent.prefab";

        private Transform _seekStartPosition;
        private Transform _alignStartPosition;
        private Transform _faceStartPosition;
        private Transform _targetPosition;
        private Transform _targetPosition2;
        private Transform _targetPosition3;

        private TargetPlacement _target;

        private GameObject _seekGameObject;
        private GameObject _alignGameObject;
        private GameObject _faceGameObject;
    
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
            if (_target == null)
            {
                _target = GameObject.Find("Target").GetComponent<TargetPlacement>();
                _target.TargetPosition = _targetPosition.position;
                _target.Enabled = false;
            }
            
            if (_seekStartPosition == null) 
                _seekStartPosition = GameObject.Find("SeekStartPosition").transform;
            if (_alignStartPosition == null) 
                _alignStartPosition = GameObject.Find("AlignStartPosition").transform;
            if (_faceStartPosition == null) 
                _faceStartPosition = GameObject.Find("FaceStartPosition").transform;
            
            if (_seekGameObject == null)
            {
                _seekGameObject = GameObject.Find("SeekMovingAgent");
                _seekGameObject.SetActive(false);
            }
            if (_alignGameObject == null)
            {
                _alignGameObject = GameObject.Find("AlignMovingAgent");
                _alignGameObject.SetActive(false);
            }
            if (_faceGameObject == null)
            {
                _faceGameObject = GameObject.Find("FaceMovingAgent");
                _faceGameObject.SetActive(false);
            }
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
        /// Test that AlignBehavior can face in the same direction as other GameObject.
        /// </summary>
        [UnityTest]
        public IEnumerator AlignBehaviourTest()
        {
            // Test setup.
            _seekGameObject.transform.position = _seekStartPosition.position;
            var seekSteeringBehavior = _seekGameObject.GetComponent<SeekSteeringBehavior>();
            var agentMover = _seekGameObject.GetComponent<AgentMover>();
            agentMover.MaximumSpeed = 2.0f;
            _target.Enabled = true;
            _target.TargetPosition = _targetPosition.position;
            seekSteeringBehavior.Target = _target.gameObject;
            seekSteeringBehavior.ArrivalDistance = 0.2f;
            _alignGameObject.transform.position = _alignStartPosition.position;
            var alignSteeringBehavior = _alignGameObject.GetComponent<AlignSteeringBehavior>();
            alignSteeringBehavior.Target = _seekGameObject;
            _alignGameObject.SetActive(true);
            _seekGameObject.SetActive(true);
            
            // Move seeker to face the first target.
            _target.TargetPosition = _targetPosition.position;
            yield return new WaitForSeconds(2.0f);
            Assert.True(
                Mathf.Abs(
                    _seekGameObject.GetComponent<AgentMover>().Orientation-
                     _alignGameObject.GetComponent<AgentMover>().Orientation) <= 
                        alignSteeringBehavior.ArrivingMargin);
            
            // Move seeker to face the second target.
            _target.TargetPosition = _targetPosition2.position;
            yield return new WaitForSeconds(2.0f);
            Assert.True(
                Mathf.Abs(
                    _seekGameObject.GetComponent<AgentMover>().Orientation- 
                      _alignGameObject.GetComponent<AgentMover>().Orientation) <=
                        alignSteeringBehavior.ArrivingMargin);
            
            // Move seeker to face the third target.
            _target.TargetPosition = _targetPosition3.position;
            yield return new WaitForSeconds(2.0f);
            Assert.True(
                Mathf.Abs(
                    _seekGameObject.GetComponent<AgentMover>().Orientation- 
                      _alignGameObject.GetComponent<AgentMover>().Orientation) <=
                      alignSteeringBehavior.ArrivingMargin);
            
            // Cleanup.
            _seekGameObject.SetActive(false);
            _alignGameObject.SetActive(false);
            _target.Enabled = false;
        }
        
        /// <summary>
        /// Test that FaceBehavior can face towards a target while it moves.
        /// </summary>
        [UnityTest]
        public IEnumerator FaceBehaviourTest()
        {
            // Test setup.
            _seekGameObject.transform.position = _seekStartPosition.position;
            var seekSteeringBehavior = _seekGameObject.GetComponent<SeekSteeringBehavior>();
            var agentMover = _seekGameObject.GetComponent<AgentMover>();
            agentMover.MaximumSpeed = 2.0f;
            _target.Enabled = true;
            _target.TargetPosition = _targetPosition.position;
            seekSteeringBehavior.Target = _target.gameObject;
            seekSteeringBehavior.ArrivalDistance = 0.2f;
            _faceGameObject.transform.position = _faceStartPosition.position;
            var faceSteeringBehavior = _faceGameObject.GetComponent<FaceMatchingSteeringBehavior>();
            faceSteeringBehavior.Target = _seekGameObject;
            _faceGameObject.SetActive(true);
            _seekGameObject.SetActive(true);
            
            // Sample the tested agent alignment in some moments of the
            // seeker travel, to check if it is still facing the seeker.
            float totalTestTimeInSeconds = 5.0f;
            int numberOfSamples = 5;
            float sampleInterval = totalTestTimeInSeconds / numberOfSamples;
            foreach (int _ in Enumerable.Range(1, numberOfSamples))
            {
                yield return new WaitForSeconds(sampleInterval);
                float currentAngle = Vector3.Angle(Vector3.up,
                    _seekGameObject.transform.position -
                    _faceGameObject.transform.position);
                // 5 degrees tolerance, because target is constantly moving.
                Assert.True(
                    Mathf.Abs(
                        currentAngle - 
                        _faceGameObject.GetComponent<AgentMover>().Orientation
                        ) <= 5); 
            }
            
            // Cleanup.
            _seekGameObject.SetActive(false);
            _faceGameObject.SetActive(false);
            _target.Enabled = false;
        }
    }
}
