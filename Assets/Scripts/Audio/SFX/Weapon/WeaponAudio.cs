using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAudio : MonoBehaviour, ISubjectWeapon
{
    [Header("SFX")]
    [SerializeField] private AudioClip _reload;

    [SerializeField] private List<AudioClip> _handAttacks;
    [SerializeField] private List<AudioClip> _shoot;

    [SerializeField] private AudioSource _gunPosSFX, _audioSourceCamera;
    [Space]

    [Header("Scripts")]
    [SerializeField] private Hands _hands;

    private Weapon _currentWeapon;

    private void OnEnable()
    {
        _hands.Hitted += HandAttack;
    }

    public void Reload()
    {
        _gunPosSFX.clip = _reload;
        _gunPosSFX.Play();
    }

    private void HandAttack()
    {
        if (_currentWeapon is Hands)
        {
            _audioSourceCamera.clip = _handAttacks[Random.Range(0, _handAttacks.Count)];
            _audioSourceCamera.Play();
        }
    }

    public void Attack()
    {
        if (_currentWeapon is Gun)
        {
            _gunPosSFX.clip = _shoot[Random.Range(0, _shoot.Count)];
            _gunPosSFX.Play();
        }
    }

    public void SwitchWeapon(Weapon weapon) => _currentWeapon = weapon;

    private void OnDisable()
    {
        _hands.Hitted -= HandAttack;
    }
}
