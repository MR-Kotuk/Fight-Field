using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(PlayerAnimations), typeof(MoveAudio), typeof(AttackWeapon))]
public class PlayerMove : MonoBehaviour
{
    public bool isCrouch { get; private set; }
    public float DirX { get; private set; }
    public float DirY { get; private set; }

    public event Action Moved, Crouched, Jumped;

    [Header("Commponents")]
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Joystick _moveJoystick;

    [SerializeField] private Transform _player, _foot;
    [Space]

    [Header("Player Settings")]
    [SerializeField] private float _speed;
    [SerializeField] private float _pushPowerJump;
    [SerializeField] private float _distGround;

    private MoveAudio _moveAudio;

    private float _currentSpeed;

    private void OnEnable() => Moved += MoveStickPlayer;

    private void Start()
    {
        _rb ??= GetComponent<Rigidbody>();
        _moveAudio ??= GetComponent<MoveAudio>();

        if (_rb == GetComponent<Rigidbody>())
            _rb.freezeRotation = true;

        isCrouch = false;
        _currentSpeed = _speed;
    }
        
    private void FixedUpdate() => Moved?.Invoke();

    private void MoveStickPlayer()
    {
        DirX = _moveJoystick.Horizontal + Input.GetAxis("Horizontal");
        DirY = _moveJoystick.Vertical + Input.GetAxis("Vertical");

        _player.localPosition += _player.transform.forward * DirY * _currentSpeed;
        _player.localPosition += _player.transform.right * DirX * _currentSpeed;

        if (Input.GetKeyDown(KeyCode.Space))
            OnJumpButton();

        if (Input.GetKeyDown(KeyCode.LeftShift))
            OnCrouchButton();

        if(DirX != 0f || DirY != 0f)
            _moveAudio.Move(isCrouch);
    }

    public void OnCrouchButton()
    {
        isCrouch = !isCrouch;

        Crouched?.Invoke();

        if (isCrouch)
            _currentSpeed = _speed / 2;
        else
            _currentSpeed = _speed;
    }

    public void OnJumpButton()
    {
        if (!isCrouch && TryRay(_foot.position, -_foot.up, _distGround))
        {
            Jumped?.Invoke();
            _rb.AddForce(new Vector3(0, 1, 0) * _pushPowerJump);

            _moveAudio.Jump();
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

    private void OnDisable() => Moved -= MoveStickPlayer;
}
