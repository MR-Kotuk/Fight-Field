using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimations : MonoBehaviour
{
    [Header("Game Objects")]
    [SerializeField] private GameObject _camera, _player;

    private PlayerMove _playerMove;
    private Animator _anim;

    private const string _moveXN = "MoveX", _moveYN = "MoveY";

    private void OnEnable()
    {
        _playerMove ??= GetComponent<PlayerMove>();

        _playerMove.Moved += Move;
    }

    private void Start() => _anim ??= GetComponent<Animator>();

    private void Move()
    {
        _anim.SetFloat(_moveXN, _playerMove.DirX);
        _anim.SetFloat(_moveYN, _playerMove.DirY);
    }

    private void OnDisable() => _playerMove.Moved -= Move;
}
