using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CameraMove : MonoBehaviour
{
    [Header("Sensitivity")]
    [SerializeField] private float _smoothTime;

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
    [SerializeField] private GameObject _head;
    [Space]

    [Header("Scripts")]
    [SerializeField] private PlayerMove _playerMove;

    private PhotonView _myView;

    private ScopeWeapon _scopeWeapon;

    private float _currentMinAngle, _currentMaxAngle;
    private float _currentSens;

    private float xRot, yRot;
    private float xRotCurrent, yRotCurrent;
    private float curentVelosityX, curentVelosityY;

    private void OnValidate()
    {
        if (_sensitivity < _minSens)
            _sensitivity = _minSens;
        else if (_sensitivity > _maxSens)
            _sensitivity = _maxSens;
    }

    private void OnEnable() => _playerMove.Crouched += CrouchAngle;

    private void Start() 
    {
        _scopeWeapon ??= GetComponent<ScopeWeapon>();
        _myView ??= GetComponent<PhotonView>();

        _currentMinAngle = _minCamAngle;
        _currentMaxAngle = _maxCamAngle;

        _currentSens = _sensitivity;
    }

    private void FixedUpdate()
    {
        if (_myView.IsMine)
        {
            if (_scopeWeapon.isScope)
                _currentSens = _scopeSens;
            else
                _currentSens = _sensitivity;

            Rotate();
        }
        else
            GetComponent<Camera>().enabled = false;
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
        //Cursor.lockState = CursorLockMode.Locked;

        xRot += Input.GetAxis("Mouse X") * _currentSens;
        yRot += Input.GetAxis("Mouse Y") * _currentSens;

        yRot = Mathf.Clamp(yRot, _currentMinAngle, _currentMaxAngle);

        xRotCurrent = Mathf.SmoothDamp(xRot, xRotCurrent, ref curentVelosityX, _smoothTime);
        yRotCurrent = Mathf.SmoothDamp(yRot, yRotCurrent, ref curentVelosityY, _smoothTime);

        _head.transform.localRotation = Quaternion.Euler(0f, 0f, yRotCurrent);
        _player.transform.rotation = Quaternion.Euler(0f, xRotCurrent, 0f);
    }

    private void OnDisable() => _playerMove.Crouched -= CrouchAngle;
}
