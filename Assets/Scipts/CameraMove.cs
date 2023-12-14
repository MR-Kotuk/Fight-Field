using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    [SerializeField] private float _sensitivity;

    [SerializeField] private float _minCamAngle, _maxCamAngle;
    [SerializeField] private float _crouchMinAngle;
    [SerializeField] private float _maxSensitivity;

    [SerializeField] private GameObject _player;
    [SerializeField] private Joystick _cameraMoveJoy;
    [SerializeField] private PlayerMove _playerMove;

    private float _camDirX, _camDirY;
    private float _currentMinAngle;

    private void OnValidate()
    {
        _sensitivity = TestValues.CheckNewValue(_sensitivity, _maxSensitivity);
    }

    private void Start() => _currentMinAngle = _minCamAngle;

    private void FixedUpdate()
    {
        _camDirX = _cameraMoveJoy.Horizontal;
        _camDirY = _cameraMoveJoy.Vertical;

        transform.Rotate(new Vector3(-_camDirY, 0, 0) * _sensitivity);
        _player.transform.Rotate(new Vector3(0, _camDirX, 0) * _sensitivity);

        var angleX = transform.localEulerAngles.x;

        if (angleX > _currentMinAngle && angleX < 180)
            angleX = _currentMinAngle;
        if (angleX < _maxCamAngle && angleX > 180)
            angleX = _maxCamAngle;

        transform.localEulerAngles = new Vector3(angleX, 0, 0);

        Vector3 camAngles = transform.eulerAngles;
        transform.eulerAngles = new Vector3(camAngles.x, camAngles.y, 0);
    }

    private void Update()
    {
        if (_playerMove.isCrouch)
            _currentMinAngle = _crouchMinAngle;
        else
            _currentMinAngle = _minCamAngle;
    }
}
