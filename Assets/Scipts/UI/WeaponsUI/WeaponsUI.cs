using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponsUI : MonoBehaviour
{
    [SerializeField] private Text _attackCount;
    [SerializeField] private Button _weaponButton;

    [SerializeField] private PlayerAttack _playerAttack;

    [SerializeField] private int _minAttackCount;

    private Weapon _weapon;

    private void Start()
    {
        _weapon = GetComponent<Weapon>();

        _playerAttack.SwitchedWeapon += SwitchWeapon;
    }
    private void FixedUpdate()
    {
        _attackCount.text = $"{_weapon.AttackCount}/{_weapon.MaxAttackCount}";

        if (_weapon.AttackCount <= _minAttackCount)
            _attackCount.color = Color.red;
        else
            _attackCount.color = Color.white;
    }
    public void SwitchWeapon(Weapon weapon)
    {
        if (_weapon == weapon)
        {
            if (_weaponButton.image.color == Color.blue && _weapon.AttackCount != _weapon.MaxAttackCount)
                _playerAttack.Reload();
            else
                _weaponButton.image.color = Color.blue;
        }
        else
            _weaponButton.image.color = Color.white;
    }
}
