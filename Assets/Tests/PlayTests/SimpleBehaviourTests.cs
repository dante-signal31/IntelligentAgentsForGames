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
        private Transform _targetPosition;

        private TargetPlacement _target;

        private GameObject _seekerGameObject;
        private GameObject _alignerGameObject;
    
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return TestLevelManagement.ReLoadScene(_currentScene);

            if (_targetPosition == null)
                _targetPosition = GameObject.Find("TargetPosition").transform;
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
            
            if (_seekerGameObject == null)
            {
                _seekerGameObject = GameObject.Find("SeekMovingAgent");
                _seekerGameObject.SetActive(false);
            }
            if (_alignerGameObject == null)
            {
                _alignerGameObject = GameObject.Find("AlignMovingAgent");
                _alignerGameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// Test that SeekBehavior can reach a target.
        /// </summary>
        [UnityTest]
        public IEnumerator SeekBehaviourTest()
        {
            // Test setup.
            _seekerGameObject.transform.position = _seekStartPosition.position;
            var seekSteeringBehavior = _seekerGameObject.GetComponent<SeekSteeringBehavior>();
            var agentMover = _seekerGameObject.GetComponent<AgentMover>();
            agentMover.MaximumSpeed = 10.0f;
            _target.Enabled = true;
            _target.TargetPosition = _targetPosition.position;
            seekSteeringBehavior.Target = _target.gameObject;
            seekSteeringBehavior.ArrivalDistance = 0.2f;
            _seekerGameObject.SetActive(true);
            
            // Give time for the seeker to get to the target.
            yield return new WaitForSeconds(2.0f);
            
            // Assert the target was reached.
            Assert.True(Vector3.Distance(_targetPosition.position, 
                _seekerGameObject.transform.position) <= 
                        (seekSteeringBehavior.ArrivalDistance + 0.1));
            
            // Cleanup.
            _seekerGameObject.SetActive(false);
            _target.Enabled = false;
        }
        
        /// <summary>
        /// Test that AlignBehavior can face towards a target while it moves.
        /// </summary>
        [UnityTest]
        public IEnumerator AlignBehaviourTest()
        {
            // Test setup.
            _seekerGameObject.transform.position = _seekStartPosition.position;
            var seekSteeringBehavior = _seekerGameObject.GetComponent<SeekSteeringBehavior>();
            var agentMover = _seekerGameObject.GetComponent<AgentMover>();
            agentMover.MaximumSpeed = 10.0f;
            _target.Enabled = true;
            _target.TargetPosition = _targetPosition.position;
            seekSteeringBehavior.Target = _target.gameObject;
            seekSteeringBehavior.ArrivalDistance = 0.2f;
            _alignerGameObject.transform.position = _alignStartPosition.position;
            var alignSteeringBehavior = _alignerGameObject.GetComponent<AlignSteeringBehavior>();
            alignSteeringBehavior.Target = _seekerGameObject;
            _alignerGameObject.SetActive(true);
            _seekerGameObject.SetActive(true);
            
            // Sample the tested agent alignment in some moments of the
            // seeker travel, to check if it is still facing the seeker.
            float totalTestTimeInSeconds = 5.0f;
            int numberOfSamples = 10;
            float sampleInterval = totalTestTimeInSeconds / numberOfSamples;
            foreach (int _ in Enumerable.Range(1, numberOfSamples))
            {
                yield return new WaitForSeconds(sampleInterval);
                float currentAngle = Vector3.Angle(Vector3.up,
                    _seekerGameObject.transform.position -
                    _alignerGameObject.transform.position);
                Assert.True(Mathf.Approximately(currentAngle, 
                    _alignerGameObject.GetComponent<AgentMover>().Orientation));
            }
            
            // Assert the target was reached by seeker.
            // Assert the target was reached.
            Assert.True(Vector3.Distance(_targetPosition.position, 
                            _seekerGameObject.transform.position) <= 
                        (seekSteeringBehavior.ArrivalDistance + 0.1));
            
            // Cleanup.
            _seekerGameObject.SetActive(false);
            _alignerGameObject.SetActive(false);
            _target.Enabled = false;
        }
    }
}
