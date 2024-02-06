using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(PlayerMove), typeof(PlayerAttack))]
public class PlayerAnimations : MonoBehaviour
{
    [Header("Game Objects")]
    [SerializeField] private GameObject _camera, _player;

    private PlayerMove _playerMove;
    private Animator _anim;

    private const string _moveXN = "MoveX", _moveYN = "MoveY", _camYN = "CamY";

    private void Start()
    {
        _anim ??= GetComponent<Animator>();
        _playerMove ??= GetComponent<PlayerMove>();

        _playerMove.Moved += Move;
    }
    private void Move()
    {
        _anim.SetFloat(_moveXN, _playerMove.DirX);
        _anim.SetFloat(_moveYN, _playerMove.DirY);

        float camRotY = _camera.transform.eulerAngles.x;
        if (camRotY < 200)
            camRotY += 360;

        _anim.SetFloat(_camYN, camRotY);
    }
}
