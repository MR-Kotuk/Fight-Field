using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody), typeof(PlayerAnimations))]
public class PlayerMove : MonoBehaviour
{
    public bool isCrouch { get; private set; }
    public float _dirX { get; private set; }
    public float _dirY { get; private set; }

    public event Action Moved, Crouched, Jumped;

    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Joystick _moveJoystick;
    [SerializeField] private Transform _originHead;

    [SerializeField] private float _speedPlayer;
    [SerializeField] private float _powerJumpPlayer;
    [SerializeField] private float _maxSpeedPlayer, _maxJumpPlayer;
    [SerializeField] private float _distGround, _distAboveHead;

    private float _currentSpeed;

    private void OnValidate()
    {
        _speedPlayer = TestValues.CheckNewValue(_speedPlayer, _maxSpeedPlayer);
        _powerJumpPlayer = TestValues.CheckNewValue(_powerJumpPlayer, _maxJumpPlayer);
    }
    private void Start()
    {
        _rb ??= GetComponent<Rigidbody>();

        if (_rb == GetComponent<Rigidbody>())
            _rb.freezeRotation = true;

        Moved += MoveStickPlayer;
        Crouched += MoveCrouch;

        isCrouch = false;
        _currentSpeed = _speedPlayer;
    }
        
    private void FixedUpdate() => Moved?.Invoke();

    private void MoveStickPlayer()
    {
        _dirX = _moveJoystick.Horizontal;
        _dirY = _moveJoystick.Vertical;

        transform.localPosition += transform.forward * _dirY * _currentSpeed;
        transform.localPosition += transform.right * _dirX * _currentSpeed;
    }

    public void OnCrouchButton() => Crouched?.Invoke();

    private void MoveCrouch()
    {
        isCrouch = !isCrouch;

        if (TryRay(transform.position, -transform.up, _distGround))
        {
            if (!isCrouch && TryRay(_originHead.position, _originHead.up, _distAboveHead))
                isCrouch = true;
        }
        else
            isCrouch = false;
        

        if (isCrouch)
            _currentSpeed = _speedPlayer / 2;
        else
            _currentSpeed = _speedPlayer;
    }

    public void OnJumpButton()
    {
        if (!isCrouch && TryRay(transform.position, -transform.up, _distGround))
        {
            Jumped?.Invoke();
            _rb.AddForce(new Vector3(0, 1, 0) * _powerJumpPlayer);
        }
    }
    
    private bool TryRay(Vector3 origin, Vector3 direction, float dist)
    {
        Ray ray = new Ray(origin, direction);
        RaycastHit raycastHit;

        if (Physics.Raycast(ray, out raycastHit))
            return raycastHit.distance <= dist;
        else
            return false;
    }
}
