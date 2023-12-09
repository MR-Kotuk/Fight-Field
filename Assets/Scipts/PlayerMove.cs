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

    private List<string> _animTypeNames = new List<string> { "isWalk", "isWalkBack", "isRun", "isRunBack", "isWait"};
    private float _dirX, _dirY;
    private bool isJump;
    private string _typeMove = "TypeMove";


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

        transform.localPosition += transform.forward * _dirY * _speedPlayer;
        transform.localPosition += transform.right * _dirX * _speedPlayer;

        Debug.Log($"{_dirY}, {_anim.GetFloat(_typeMove)}");
        if (_dirY < 0.5f && _dirY > 0)
            AnimationState(_animTypeNames[0], 0);
        else if (_dirY >= 0.5 && _dirY > 0)
            AnimationState(_animTypeNames[2], 0);
        else if (_dirY > -0.5f && _dirY < 0)
            AnimationState(_animTypeNames[1], 0);
        else if (_dirY <= -0.5f && _dirY < 0)
            AnimationState(_animTypeNames[3], 0);
        else
            AnimationState(_animTypeNames[4], 0);
    }

    private void AnimationState(string animName, int numTypeMove)
    {
        for (int i = 0; i < _animTypeNames.Count; i++)
        {
            if (_animTypeNames[i] == animName)
            {
                _anim.SetBool(animName, true);
                _anim.SetFloat(_typeMove, numTypeMove);
            }
            else
                _anim.SetBool(_animTypeNames[i], false);
        }
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
