using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAudio : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _moveDelay, _crouchMoveDelay;
    [Space]

    [Header("SFX")]
    [SerializeField] private AudioClip _jumpSFX;

    [SerializeField] private List<AudioClip> _moveSFX;

    [SerializeField] private AudioSource _foot;

    private float _moveVolume;
    private float _currentMoveDelay;

    private bool isMove;
    private float _frames;

    private void Start()
    {
        isMove = true;

        _moveVolume = _foot.volume;
        _currentMoveDelay = _moveDelay;
    }

    public void Jump()
    {
        _foot.clip = _jumpSFX;
        _foot.Play();
    }

    private void Update()
    {
        if (!isMove && _frames < _currentMoveDelay)
        {
            _frames += Time.deltaTime;

            if (_frames >= _currentMoveDelay)
            {
                isMove = true;
                _frames = 0f;
            }
        }
    }

    public void Move(bool isCrouch)
    {
        if (isMove)
        {
            isMove = false;

            if (isCrouch && _foot.volume == _moveVolume)
            {
                _foot.volume = _moveVolume / 3;
                _currentMoveDelay = _crouchMoveDelay;
            }
            else if (!isCrouch && _foot.volume != _moveVolume)
            {
                _foot.volume = _moveVolume;
                _currentMoveDelay = _moveDelay;
            }

            _foot.clip = _moveSFX[Random.Range(0, _moveSFX.Count)];
            _foot.Play();
        }
    }
}
