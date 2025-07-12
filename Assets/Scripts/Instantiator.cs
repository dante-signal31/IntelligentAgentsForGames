using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Component to instantiate multiple objects around a gameobject.
/// </summary>
public class Instantiator : MonoBehaviour
{
    /// <summary>
    /// Class to represent an instantiable object spawn data.
    /// </summary>
    [Serializable]
    public struct Instantiable
    {
        /// <summary>
        /// Name of the object.
        /// </summary>
        [Tooltip("Name of the object.")]
        public string name;
        
        /// <summary>
        /// GameObject to instantiate.
        /// </summary>
        [Tooltip("GameObject to instantiate.")]
        public GameObject gameObject;

        /// <summary>
        /// Position to instantiate at.
        /// </summary>
        [Tooltip("Position to instantiate at, in local space.")]
        public Vector3 position;

        /// <summary>
        /// Rotation to instantiate at.
        /// </summary>
        [Tooltip("Initial rotation for spawned object, in degrees. Origin is up " +
                 "direction in local space.")]
        [Range(-180, 180)] public float rotation;
    }
    
    [Header("CONFIGURATION:")]
    [Tooltip("Prefab to instantiate.")]
    [SerializeField] private Instantiable[] _objectsToInstantiate;
    
    [Header("DEBUG:")]
    [Tooltip("Whether to show gizmos.")]
    [SerializeField] private bool showGizmos = true;
    [Tooltip("Radius for gizmo markers of position for instantiable objects spawn " +
             "points.")]
    [SerializeField] private float gizmoRadius = 1.0f;
    [Tooltip("Arrow length to show instantiable objects initial rotation at spawn " +
             "point.")]
    [SerializeField] private float gizmoArrowLength = 1.0f;
    [Tooltip("Rotation arrow head size.")]
    [SerializeField] private float gizmoArrowHeadSize = 0.25f;
    [Tooltip("Color to show spawn points.")]
    [SerializeField] private Color gizmoColor = Color.yellow;
    
    public Dictionary<string, GameObject> InstantiatedObjects { get; private set; }
    private void Awake()
    {
        InstantiateObjects();
    }

    /// <summary>
    /// Instantiates the objects.
    /// </summary>
    public void InstantiateObjects()
    {
        InstantiatedObjects = new Dictionary<string, GameObject>();
        foreach (var instantiableGameObject in _objectsToInstantiate)
        {
            GameObject instantiated = Instantiate(instantiableGameObject.gameObject, 
                transform.TransformPoint(instantiableGameObject.position), 
                Quaternion.AngleAxis(instantiableGameObject.rotation, 
                    Vector3.forward));
            InstantiatedObjects[instantiableGameObject.name] = instantiated;
        }
    }
    
#if UNITY_EDITOR
    
    private void DrawSpawnPoints()
    {
        foreach (Instantiable instantiableGameObject in _objectsToInstantiate)
        {
            Gizmos.DrawWireSphere(
                transform.TransformPoint(instantiableGameObject.position), 
                gizmoRadius);
        }
    }
    
    private void DrawInitialRotations()
    {
        foreach (Instantiable instantiableGameObject in _objectsToInstantiate)
        {
            GizmoTools.DrawArrow(
                transform.TransformPoint(instantiableGameObject.position), 
                Quaternion.AngleAxis(instantiableGameObject.rotation, 
                    Vector3.forward) * Vector3.up * gizmoArrowLength, 
                gizmoArrowHeadSize, 20.0f); 
        }
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        if (showGizmos)
        {
            DrawSpawnPoints();
            DrawInitialRotations();
        }
    }
#endif
}
