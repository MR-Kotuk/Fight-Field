using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _timeMove, _crouchTimeMove;
    [Space]

    [Header("SFX")]
    [SerializeField] private AudioClip _jumpSFX;

    [SerializeField] private List<AudioClip> _moveSFX;

    [SerializeField] private AudioSource _foot;

    private PlayerMove _playerMove;

    private float _moveVolume;
    private float _currentTimeMove;

    private bool isMove;

    private void OnEnable()
    {
        _playerMove ??= GetComponent<PlayerMove>();

        _playerMove.Jumped += Jump;
    }
    private void Start()
    {
        _moveVolume = _foot.volume;
        _currentTimeMove = _timeMove;

        StartCoroutine(MoveAudio());
    }

    private void FixedUpdate()
    {
        if ((_playerMove.DirX != 0 || _playerMove.DirY != 0) && isMove)
            StartCoroutine(MoveAudio());
    }
    private void Jump()
    {
        _foot.clip = _jumpSFX;
        _foot.Play();
    }

    private IEnumerator MoveAudio()
    {
        isMove = false;

        if (_playerMove.isCrouch && _foot.volume == _moveVolume)
        {
            _foot.volume = _moveVolume / 3;
            _currentTimeMove = _crouchTimeMove;
        }
        else if (!_playerMove.isCrouch && _foot.volume != _moveVolume)
        {
            _foot.volume = _moveVolume;
            _currentTimeMove = _timeMove;
        }

        _foot.clip = _moveSFX[Random.Range(0, _moveSFX.Count)];
        _foot.Play();

        yield return new WaitForSeconds(_currentTimeMove);

        isMove = true;
    }
    private void OnDisable()
    {
        _playerMove.Jumped -= Jump;
    }
}
