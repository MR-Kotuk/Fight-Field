using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Animator))]
public class PlayerMove : MonoBehaviour
{
    public bool isCrouch { get; private set; }

    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Joystick _moveJoystick;
    [SerializeField] private Animator _anim;

    [SerializeField] private float _speedPlayer;
    [SerializeField] private float _powerJumpPlayer;
    [SerializeField] private float _maxSpeedPlayer, _maxJumpPlayer;

    private float _dirX, _dirY;
    private bool isJump;

    private string _moveX = "MoveX", _moveY = "MoveY";
    private float _currentSpeed;

    private void OnValidate()
    {
        _anim ??= GetComponent<Animator>();
        _rb ??= GetComponent<Rigidbody>();

        if (_rb == GetComponent<Rigidbody>())
            _rb.freezeRotation = true;

        _speedPlayer = TestValues.CheckNewValue(_speedPlayer, _maxSpeedPlayer);
        _powerJumpPlayer = TestValues.CheckNewValue(_powerJumpPlayer, _maxJumpPlayer);
    }
    private void Start()
    {
        isCrouch = false;
        _currentSpeed = _speedPlayer;
    }
    private void FixedUpdate()
    {
        _dirX = _moveJoystick.Horizontal;
        _dirY = _moveJoystick.Vertical;

        transform.localPosition += transform.forward * _dirY * _currentSpeed;
        transform.localPosition += transform.right * _dirX * _currentSpeed;

        _anim.SetFloat(_moveX, _dirX);
        _anim.SetFloat(_moveY, _dirY);

        _anim.SetBool("isMove", !isCrouch);
        _anim.SetBool("isCrouch", isCrouch);
    }
    public void OnCrouch()
    {
        isCrouch = !isCrouch;

        if (isCrouch)
            _currentSpeed = _speedPlayer / 2;
        else
            _currentSpeed = _speedPlayer;
    }
    public void OnJump()
    {
        if (isJump && !isCrouch)
        {
            StartCoroutine(JumpAnim());
            _rb.AddForce(new Vector3(0, 1, 0) * _powerJumpPlayer);
        }
    }
    private IEnumerator JumpAnim()
    {
        _anim.SetBool("isJump", true);
        yield return new WaitForSeconds(0.5f);
        _anim.SetBool("isJump", false);
    }

    private void OnCollisionStay(Collision collision) => isJump = true;
    private void OnCollisionExit(Collision collision) => isJump = false;
}
