using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAudio : MonoBehaviour
{
    [SerializeField] private AudioClip _reload;

    [SerializeField] private List<AudioClip> _handAttacks;
    [SerializeField] private List<AudioClip> _shoot;

    [SerializeField] private AudioSource _gunPosSFX;

    [SerializeField] private AudioSource _audioSourceCamera;

    private PlayerAttack _playerAttack;

    private Weapon _currentWeapon;
    private void Start()
    {
        _playerAttack ??= GetComponent<PlayerAttack>();

        _playerAttack.SwitchedWeapon += SwitchWeapon;
        _playerAttack.Attacked += Attack;
        _playerAttack.Reloaded += Reload;
    }
    private void Reload()
    {
        if (_currentWeapon is Gun)
        {
            _gunPosSFX.clip = _reload;
            _gunPosSFX.Play();
        }
    }
    public void HandAttack()
    {
        if (_currentWeapon is Hands)
        {
            _audioSourceCamera.clip = _handAttacks[Random.Range(0, _handAttacks.Count)];
            _audioSourceCamera.Play();
        }
    }
    private void Attack()
    {
        if (_currentWeapon is Gun)
        {
            _gunPosSFX.clip = _shoot[Random.Range(0, _shoot.Count)];
            _gunPosSFX.Play();
        }
    }
    private void SwitchWeapon(Weapon weapon)
    {
        _currentWeapon = weapon;
    }
}
