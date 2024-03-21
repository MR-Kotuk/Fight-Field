using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponsUI : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private int _minAttackCount;
    [Space]

    [Header("UI Objects Settings")]
    [SerializeField] private Text _attackCount;
    [SerializeField] private Button _weaponButton;

    [SerializeField] private GameObject _scopeImage, _scopeButton;
    [Space]

    [Header("Scripts")]
    [SerializeField] private AttackWeapon _attackWeapon;
    [SerializeField] private ScopeWeapon _scopeWeapon;

    [SerializeField] private SpriteManager _spriteManager;

    private Weapon _weapon, _currentWeapon;

    private void OnEnable()
    {
        _weapon ??= GetComponent<Weapon>();

        _attackWeapon.SwitchedWeapon += SwitchWeapon;
    }

    private void Update()
    {
        if (_currentWeapon == _weapon)
        {
            _attackCount.text = $"{_weapon.WeaponSettings.AttackCount}/{_weapon.WeaponSettings.MaxAttackCount}";

            if (_weapon.WeaponSettings.AttackCount <= _minAttackCount)
                _attackCount.color = Color.red;
            else
                _attackCount.color = Color.white;

            if (_scopeWeapon.isScope || (_weapon.WeaponSettings.Name == "Granade" && _attackWeapon.isAttack && _weapon.WeaponSettings.AttackCount > 0 && !_weapon.WeaponSettings.isReturn))
                _scopeImage.SetActive(false);
            else
                _scopeImage.SetActive(true);
        }
    }

    public void SwitchWeapon(Weapon weapon)
    {
        _currentWeapon = weapon;

        if (_weapon == _currentWeapon)
        {
            if (_weaponButton.image.color == Color.blue && _weapon.WeaponSettings.AttackCount != _weapon.WeaponSettings.MaxAttackCount)
                _attackWeapon.Reload();
            else
                _weaponButton.image.color = Color.blue;

            if (_weapon.WeaponSettings.isNoScope)
                _scopeButton.SetActive(false);
            else
                _scopeButton.SetActive(true);
        }
        else
            _weaponButton.image.color = Color.white;
    }

    private void OnDisable() => _attackWeapon.SwitchedWeapon -= SwitchWeapon;
}
