using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    [Header("Sensitivity")]
    [SerializeField] private float _sensitivity, _scopeSens;
    [SerializeField] private float _maxSens, _minSens;
    [SerializeField] private float _maxScopeSens, _minScopeSens;
    [Space]

    [Header("Angls")]
    [SerializeField] private float _minCamAngle, _maxCamAngle;
    [SerializeField] private float _crouchMinAngle, _crouchMaxAngle;
    [Space]

    [Header("Crouch Settings")]
    [SerializeField] private float _crouchScale;
    [Space]

    [Header("Game Objects")]
    [SerializeField] private GameObject _player;
    [SerializeField] private Joystick _cameraMoveJoy, _shootJoy;
    [Space]

    [Header("Scripts")]
    [SerializeField] private PlayerMove _playerMove;

    private ScopeWeapon _scopeWeapon;

    private float _camDirX, _camDirY;
    private float _currentMinAngle, _currentMaxAngle;
    private float _currentSens;

    private float _lastAngleX, _lastAngleY;

    private void OnValidate()
    {
        if (_sensitivity < _minSens)
            _sensitivity = _minSens;
        else if (_sensitivity > _maxSens)
            _sensitivity = _maxSens;
    }
    private void OnEnable()
    {
        _playerMove.Crouched += CrouchAngle;
    }
    private void Start() 
    {
        _scopeWeapon ??= GetComponent<ScopeWeapon>();

        _currentMinAngle = _minCamAngle;
        _currentMaxAngle = _maxCamAngle;

        _currentSens = _sensitivity;
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
        if (_playerMove.isCrouch)
        {
            _currentMinAngle = _crouchMinAngle;
            _currentMaxAngle = _crouchMaxAngle;

            _player.transform.localScale = new Vector3(_crouchScale, _crouchScale, _crouchScale);
            transform.localScale = new Vector3(transform.localScale.x / _crouchScale, transform.localScale.y / _crouchScale, transform.localScale.z / _crouchScale);
        }
        else
        {
            _currentMinAngle = _minCamAngle;
            _currentMaxAngle = _maxCamAngle;

            _player.transform.localScale = new Vector3(1f, 1f, 1f);
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
    }
    private void Rotate()
    {
        _camDirX = _cameraMoveJoy.Horizontal + (_shootJoy.Horizontal / _sensitivity);
        _camDirY = _cameraMoveJoy.Vertical + (_shootJoy.Vertical / _sensitivity);

        if ((_camDirX != _lastAngleX || _camDirY != _lastAngleY) || (_shootJoy.Horizontal != 0 || _shootJoy.Vertical != 0))
        {
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

            _lastAngleX = _camDirX;
            _lastAngleY = _camDirY;
        }
    }
    private void OnDisable()
    {
        _playerMove.Crouched -= CrouchAngle;
    }
}
