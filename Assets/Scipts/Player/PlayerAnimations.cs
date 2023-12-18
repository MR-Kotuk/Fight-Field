using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(PlayerMove))]
public class PlayerAnimations : MonoBehaviour
{
    private PlayerMove _playerMove;
    private Animator _anim;

    private const string _moveXText = "MoveX", _moveYText = "MoveY";
    private const string _isMoveText = "isMove", _isJumpText = "isJump";
    private const string _isCrouchText = "isCrouch";

    private void OnValidate()
    {
        _anim ??= GetComponent<Animator>();
        _playerMove ??= GetComponent<PlayerMove>();
    }
    private void Start()
    {
        _playerMove.MovePlayer += AnimMove;
        _playerMove.PlayerCrouch += AnimCrouch;
        _playerMove.PlayerJump += AnimJump;
    }

    public void AnimMove()
    {
        _anim.SetFloat(_moveXText, _playerMove._dirX);
        _anim.SetFloat(_moveYText, _playerMove._dirY);

        _anim.SetBool(_isMoveText, !_playerMove.isCrouch);
    }
    public void AnimCrouch()
    {
        _anim.SetBool(_isMoveText, !_playerMove.isCrouch);
        _anim.SetBool(_isCrouchText, _playerMove.isCrouch);
    }
    public void AnimJump() => StartCoroutine(Jump());
    private IEnumerator Jump()
    {
        _anim.SetBool(_isJumpText, true);
        yield return new WaitForSeconds(0.5f);
        _anim.SetBool(_isJumpText, false);
    }
}
