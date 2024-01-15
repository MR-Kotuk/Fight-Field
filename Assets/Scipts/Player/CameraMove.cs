using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    [Header("Sensitivity")]
    [SerializeField] private float _sensitivity, _scopeSens;
    [SerializeField] private float _maxSensY, _maxSensX;

    [Header("Angles")]
    [SerializeField] private float _minCamAngle, _maxCamAngle;
    [SerializeField] private float _crouchMinAngle, _crouchMaxAngle;

    [SerializeField] private float _crouchPosY;

    [Header("Objects")]
    [SerializeField] private GameObject _player;
    [SerializeField] private Joystick _cameraMoveJoy, _shootJoy;

    [SerializeField] private PlayerMove _playerMove;

    private ScopeWeapon _scopeWeapon;

    private float _camDirX, _camDirY;
    private float _currentMinAngle, _currentMaxAngle;
    private float _currentSens;

    private float _standPos;

    private void Start() 
    {
        _scopeWeapon ??= GetComponent<ScopeWeapon>();

        _playerMove.Crouched += CrouchAngle;

        _currentMinAngle = _minCamAngle;
        _currentMaxAngle = _maxCamAngle;

        _currentSens = _sensitivity;
        _standPos = transform.localPosition.y;
    }
    private void FixedUpdate()
    {
        if (_scopeWeapon.isScope)
            _currentSens = _scopeSens;
        else
            _currentSens = _sensitivity;

        Rotate();
    }
    private void CrouchAngle()
    {
        Vector3 pos = transform.localPosition;

        if (_playerMove.isCrouch)
        {
            _currentMinAngle = _crouchMinAngle;
            _currentMaxAngle = _crouchMaxAngle;

            pos.y = _crouchPosY;
        }
        else
        {
            _currentMinAngle = _minCamAngle;
            _currentMaxAngle = _maxCamAngle;

            pos.y = _standPos;
        }

        transform.localPosition = pos;
    }
    private void Rotate()
    {
        _camDirX = _cameraMoveJoy.Horizontal + (_shootJoy.Horizontal / 5);
        _camDirY = _cameraMoveJoy.Vertical + (_shootJoy.Vertical / 5);

        transform.Rotate(new Vector3(-_camDirY, 0, 0) * _currentSens);
        _player.transform.Rotate(new Vector3(0, _camDirX, 0) * _currentSens);

        var angleX = transform.localEulerAngles.x;

        if (angleX > _currentMinAngle && angleX < 180)
            angleX = _currentMinAngle;
        if (angleX < _currentMaxAngle && angleX > 180)
            angleX = _currentMaxAngle;

        transform.localEulerAngles = new Vector3(angleX, 0, 0);

        Vector3 camAngles = transform.eulerAngles;
        transform.eulerAngles = new Vector3(camAngles.x, camAngles.y, 0);
    }
}
