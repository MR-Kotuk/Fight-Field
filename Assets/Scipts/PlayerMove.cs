using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Animator))]
public class PlayerMove : MonoBehaviour
{
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Joystick _moveJoystick;
    [SerializeField] private Animator _anim;

    [SerializeField] private float _speedPlayer;
    [SerializeField] private float _powerJumpPlayer;
    [SerializeField] private float _maxSpeedPlayer, _maxJumpPlayer;

    private float _dirX, _dirY;
    private bool isJump;


    private void OnValidate()
    {
        _anim ??= GetComponent<Animator>();
        _rb ??= GetComponent<Rigidbody>();

        if (_rb == GetComponent<Rigidbody>())
            _rb.freezeRotation = true;

        _speedPlayer = TestValues.CheckNewValue(_speedPlayer, _maxSpeedPlayer);
        _powerJumpPlayer = TestValues.CheckNewValue(_powerJumpPlayer, _maxJumpPlayer);
    }
    
    private void FixedUpdate()
    {
        _dirX = _moveJoystick.Horizontal;
        _dirY = _moveJoystick.Vertical;

        //transform.localPosition += transform.forward * _dirY * _speedPlayer;
        //transform.localPosition += transform.right * _dirX * _speedPlayer;

        _anim.SetFloat("MoveX", _dirX);
        _anim.SetFloat("MoveY", _dirY);
    }
    public void OnJump()
    {
        if (isJump)
            _rb.AddForce(new Vector3(0, 1, 0) * _powerJumpPlayer);
    }
    
    private void OnCollisionStay(Collision collision) => isJump = true;
    private void OnCollisionExit(Collision collision) => isJump = false;
}
