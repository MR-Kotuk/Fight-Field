using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAudio : MonoBehaviour
{
    [SerializeField] private PlayerAttack _playerAttack;

    [SerializeField] private List<AudioClip> _shoot;

    [SerializeField] private AudioSource _gunPosSFX;

    private Weapon _currentWeapon;
    private void Start()
    {
        _playerAttack.SwitchedWeapon += SwitchWeapon;
        _playerAttack.Attacked += Attack;
    }

    private void SwitchWeapon(Weapon weapon)
    {
        _currentWeapon = weapon;
    }

    private void Attack()
    {
        if (_currentWeapon is Gun)
        {
            _gunPosSFX.clip = _shoot[Random.Range(0, _shoot.Count)];
            _gunPosSFX.Play();
        }
    }
}
