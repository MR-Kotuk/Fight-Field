using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [SerializeField] private List<AudioClip> _moveSFX;

    [SerializeField] private AudioSource _foot;

    [SerializeField] private float _timeMiddleMoveSFX;

    private PlayerMove _playerMove;

    private bool isMove;

    private void Start()
    {
        _playerMove ??= GetComponent<PlayerMove>();

        StartCoroutine(MoveAudio());
    }

    private void FixedUpdate()
    {
        if ((_playerMove.DirX != 0 || _playerMove.DirY != 0) && isMove)
            StartCoroutine(MoveAudio());
    }

    private IEnumerator MoveAudio()
    {
        isMove = false;

        if (_foot.clip != null)
        {
            _foot.clip = _moveSFX[Random.Range(0, _moveSFX.Count)];
            _foot.Play();
        }
        else
            _foot.clip = _moveSFX[Random.Range(0, _moveSFX.Count)];

        yield return new WaitForSeconds(_timeMiddleMoveSFX);

        isMove = true;
    }
}
