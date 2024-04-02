using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    [Header("Game Objects")]
    [SerializeField] private GameObject _camera, _player;
    [Space]

    [Header("Player")]
    [SerializeField] private PlayerMove _playerMove;
    [SerializeField] private Animator _anim;

    private const string _moveXN = "MoveX", _moveYN = "MoveY";

    private void OnEnable() => _playerMove.Moved += Move;

    private void Move()
    {
        _anim.SetFloat(_moveXN, _playerMove.DirX);
        _anim.SetFloat(_moveYN, _playerMove.DirY);
    }

    private void OnDisable() => _playerMove.Moved -= Move;
}
