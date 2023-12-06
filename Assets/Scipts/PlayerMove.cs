using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMove : MonoBehaviour
{
    [SerializeField] private Rigidbody _rb;

    [SerializeField] private Joystick _moveJoystick;
    [SerializeField] private Joystick _cameraMoveJoy;

    [SerializeField] private GameObject _playerCamera;

    [SerializeField] private GameValues _gameValues;

    private float _dirX, _dirZ;
    private float _camDirX, _camDirY;
    private bool isJump;

    private void OnValidate()
    {
        _rb ??= GetComponent<Rigidbody>();

        if (_rb == GetComponent<Rigidbody>())
            _rb.freezeRotation = true;
    }
    
    private void FixedUpdate()
    {
        Debug.Log(isJump);
        _dirX = _moveJoystick.Horizontal;
        _dirZ = _moveJoystick.Vertical;

        transform.localPosition += transform.forward * _dirZ * _gameValues.GetSpeedPlayer();
        transform.localPosition += transform.right * _dirX * _gameValues.GetSpeedPlayer();

        _camDirX = _cameraMoveJoy.Horizontal;
        _camDirY = _cameraMoveJoy.Vertical;

        transform.Rotate(new Vector3(0, -_camDirX, 0) * _gameValues.GetSensitivCam());
        _playerCamera.transform.Rotate(new Vector3(_camDirY, 0, 0) * _gameValues.GetSensitivCam());
    }

    public void OnJump()
    {
        if (isJump)
            _rb.AddForce(new Vector3(0, 1, 0) * _gameValues.GetPowerJumpPlayer());
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.position.y < gameObject.transform.position.y)
            isJump = true;
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.transform.position.y < gameObject.transform.position.y)
            isJump = false;
    }
    
}
