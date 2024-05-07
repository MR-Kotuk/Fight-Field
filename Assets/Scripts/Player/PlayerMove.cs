using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;

[RequireComponent(typeof(PlayerAnimations), typeof(MoveAudio), typeof(AttackWeapon))]
public class PlayerMove : MonoBehaviour
{
    public bool isCrouch { get; private set; }
    public float DirX { get; private set; }
    public float DirY { get; private set; }

    public event Action Moved, Crouched, Jumped;

    [Header("Commponents")]
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Transform _player, _foot;
    [Space]

    [Header("Player Settings")]
    [SerializeField] private float _speed, _addRunSpeed;
    [SerializeField] private float _pushPowerJump;
    [SerializeField] private float _distGround;
    [Space]

    [Header("Audio")]
    [SerializeField] private MoveAudio _moveAudio;

    private float _currentSpeed;

    private PhotonView _myView;

    private void Start()
    {
        if (_rb == GetComponent<Rigidbody>())
            _rb.freezeRotation = true;

        _myView = GetComponent<PhotonView>();

        isCrouch = false;
        _currentSpeed = _speed;
    }

    private void Update()
    {
        if(_myView.IsMine)
            InputKeys();
    }

    private void InputKeys()
    {
        DirX = Input.GetAxis("Horizontal");
        DirY = Input.GetAxis("Vertical");

        if (DirX != 0f || DirY != 0f)
            MovePlayer();
    }

    public void Acceleration(bool isRun)
    {
        if(isRun)
            _currentSpeed += _addRunSpeed;
        else
            _currentSpeed -= _addRunSpeed;
    }

    private void MovePlayer()
    {
        DirX = Input.GetAxis("Horizontal");
        DirY = Input.GetAxis("Vertical");

        _player.localPosition += _player.transform.forward * DirY * _currentSpeed * Time.deltaTime;
        _player.localPosition += _player.transform.right * DirX * _currentSpeed * Time.deltaTime;

        if(DirX != 0f || DirY != 0f)
            _moveAudio.Move(isCrouch);
    }

    public void OnCrouch()
    {
        isCrouch = !isCrouch;

        Crouched?.Invoke();

        if (isCrouch)
            _currentSpeed = _speed / 2;
        else
            _currentSpeed = _speed;
    }

    public void OnJump()
    {
        Debug.Log("Jump");

        if (!isCrouch && TryRay(_foot.position, -_foot.up, _distGround))
        {
            Jumped?.Invoke();
            _rb.AddForce(new Vector3(0, 1, 0) * _pushPowerJump * Time.fixedDeltaTime);

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
}
