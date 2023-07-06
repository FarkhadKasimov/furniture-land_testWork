using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MainInput : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float targetSpeed;
    [SerializeField] private float mouseSensitivity;
    [SerializeField] private float touchSensitivity;

    [SerializeField] private Vector2 minClampedRotation;
    [SerializeField] private Vector2 maxClampedRotation;

    [SerializeField] private Vector3 dieCameraRotation;
    
    public Vector2 MouseInput => _mouseInput;
    private Vector2 _mouseInput;

    private Camera _mainCamera;
    private Vector2 _inputRotation;
    private Vector2 _startRotation;

    public bool PlayerKilled;

    private void Start()
    {
        _mainCamera = Camera.main;
        PlayerKilled = false;

        _startRotation.x = target.localEulerAngles.y;
        _startRotation.y = target.localEulerAngles.x;

        _inputRotation = _startRotation;
    }

    void FixedUpdate()
    {
        if (PlayerKilled)
        {
            return;
        }
        
        _mainCamera.transform.DORotate(new Vector3(target.eulerAngles.x, target.eulerAngles.y, 0), targetSpeed * Time.deltaTime);
        target.localRotation = Quaternion.Euler(_inputRotation.y, _inputRotation.x, target.localRotation.z);
    }

    public void CameraViewInput()
    {
        if (PlayerKilled)
        {
            return;
        }

        if (Input.touchCount > 0)
        {
            _mouseInput.x = -Input.touches[0].deltaPosition.x * touchSensitivity * Time.deltaTime;
            _mouseInput.y = Input.touches[0].deltaPosition.y * touchSensitivity * Time.deltaTime;
        }
        else
        {
            _mouseInput.x = -Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            _mouseInput.y = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        }
        
        _inputRotation.x -= _mouseInput.x;
        _inputRotation.y -= _mouseInput.y;

        _inputRotation.x = Mathf.Clamp(_inputRotation.x, _startRotation.x + minClampedRotation.x, _startRotation.x + maxClampedRotation.x);
        _inputRotation.y = Mathf.Clamp(_inputRotation.y, _startRotation.y + minClampedRotation.y, _startRotation.y + maxClampedRotation.y);
    }

    public void BlockCameraAfterPlayerDied()
    {
        _mainCamera.transform.DORotate(dieCameraRotation, 0.5f);
    }
}
