using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    [SerializeField] private float _sensY, _sensX;

    [SerializeField] private float _minCamAngle, _maxCamAngle;
    [SerializeField] private float _crouchMinAngle, _crouchMaxAngle;
    [SerializeField] private float _maxSensY, _maxSensX;

    [SerializeField] private GameObject _player, _bodyPlayer;
    [SerializeField] private Joystick _cameraMoveJoy, _shootJoy;

    private float _camDirX, _camDirY;
    private float _currentMinAngle, _currentMaxAngle;

    private float _distCamToBody;

    private PlayerMove _playerMove;

    private void OnValidate()
    {
        _sensY = TestValues.CheckNewValue(_sensY, _maxSensY);
        _sensX = TestValues.CheckNewValue(_sensX, _maxSensX);
    }

    private void Start() 
    {
        _playerMove = _player.GetComponent<PlayerMove>();

        _distCamToBody = transform.position.y - _bodyPlayer.transform.position.y;

        _currentMinAngle = _minCamAngle;
        _currentMaxAngle = _maxCamAngle;
    }


    private void FixedUpdate()
    {
        _camDirX = _cameraMoveJoy.Horizontal + (_shootJoy.Horizontal / 5);
        _camDirY = _cameraMoveJoy.Vertical + (_shootJoy.Vertical / 5);

        transform.Rotate(new Vector3(-_camDirY, 0, 0) * _sensY);
        _player.transform.Rotate(new Vector3(0, _camDirX, 0) * _sensX);

        var angleX = transform.localEulerAngles.x;

        if (angleX > _currentMinAngle && angleX < 180)
            angleX = _currentMinAngle;
        if (angleX < _currentMaxAngle && angleX > 180)
            angleX = _currentMaxAngle;

        transform.localEulerAngles = new Vector3(angleX, 0, 0);

        Vector3 camAngles = transform.eulerAngles;
        transform.eulerAngles = new Vector3(camAngles.x, camAngles.y, 0);

        Vector3 crouchCamPos = transform.position;
        crouchCamPos.y = _bodyPlayer.transform.position.y + _distCamToBody;
        transform.position = crouchCamPos;
    }
    private void Update()
    {
        if (_playerMove.isCrouch)
        {
            _currentMinAngle = _crouchMinAngle;
            _currentMaxAngle = _crouchMaxAngle;
        }
        else
        {
            _currentMinAngle = _minCamAngle;
            _currentMaxAngle = _maxCamAngle;
        }
    }
}
