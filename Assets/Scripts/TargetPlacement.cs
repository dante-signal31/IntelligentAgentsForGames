using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


/// <summary>
/// Listen for mouse clicks and updates target position to position clicked.
/// </summary>
public class TargetPlacement : MonoBehaviour
{
    [Header("WIRING:")] 
    [SerializeField] private Transform targetTransform;
    
    private Camera _mainCamera;
    
    public void OnPointAndClick(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector3 newPosition = new Vector3(Mouse.current.position.x.ReadValue(),
                Mouse.current.position.y.ReadValue(), 
                _mainCamera.nearClipPlane);
            targetTransform.position = _mainCamera.ScreenToWorldPoint(newPosition);
        }
    }

    private void Awake()
    {
        _mainCamera = Camera.main;
    }
}
