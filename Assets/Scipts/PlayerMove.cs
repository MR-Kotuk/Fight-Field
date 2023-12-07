using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMove : MonoBehaviour
{
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Joystick _moveJoystick;

    [SerializeField] private float _speedPlayer;
    [SerializeField] private float _powerJumpPlayer;
    [SerializeField] private float _maxSpeedPlayer, _maxJumpPlayer;

    private float _dirX, _dirZ;
    private bool isJump;

    private void OnValidate()
    {
        _rb ??= GetComponent<Rigidbody>();

        if (_rb == GetComponent<Rigidbody>())
            _rb.freezeRotation = true;

        _speedPlayer = TestValues.CheckNewValue(_speedPlayer, _maxSpeedPlayer);
        _powerJumpPlayer = TestValues.CheckNewValue(_powerJumpPlayer, _maxJumpPlayer);
    }
    
    private void FixedUpdate()
    {
        _dirX = _moveJoystick.Horizontal;
        _dirZ = _moveJoystick.Vertical;

        transform.localPosition += transform.forward * _dirZ * _speedPlayer;
        transform.localPosition += transform.right * _dirX * _speedPlayer;
    }

    public void OnJump()
    {
        if (isJump)
            _rb.AddForce(new Vector3(0, 1, 0) * _powerJumpPlayer);
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
