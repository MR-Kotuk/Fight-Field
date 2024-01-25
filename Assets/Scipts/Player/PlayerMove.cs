using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(PlayerAnimations), typeof(PlayerAudio))]
public class PlayerMove : MonoBehaviour
{
    public bool isCrouch { get; private set; }
    public float DirX { get; private set; }
    public float DirY { get; private set; }

    public event Action Moved, Crouched, Jumped;

    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Joystick _moveJoystick;

    [SerializeField] private Transform _player, _foot;

    [SerializeField] private float _speedPlayer;
    [SerializeField] private float _powerJumpPlayer;
    [SerializeField] private float _distGround;

    private float _currentSpeed;

    private void Start()
    {
        _rb ??= GetComponent<Rigidbody>();

        if (_rb == GetComponent<Rigidbody>())
            _rb.freezeRotation = true;

        Moved += MoveStickPlayer;

        isCrouch = false;
        _currentSpeed = _speedPlayer;
    }
        
    private void FixedUpdate() => Moved?.Invoke();

    private void MoveStickPlayer()
    {
        DirX = _moveJoystick.Horizontal;
        DirY = _moveJoystick.Vertical;

        _player.localPosition += _player.transform.forward * DirY * _currentSpeed;
        _player.localPosition += _player.transform.right * DirX * _currentSpeed;
    }

    public void OnCrouchButton()
    {
        isCrouch = !isCrouch;

        Crouched?.Invoke();

        if (isCrouch)
        {
            _currentSpeed = _speedPlayer / 2;
        }
        else
            _currentSpeed = _speedPlayer;
    }

    public void OnJumpButton()
    {
        if (!isCrouch && TryRay(_foot.position, -_foot.up, _distGround))
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
