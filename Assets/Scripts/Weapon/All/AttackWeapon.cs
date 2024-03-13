using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AttackWeapon : MonoBehaviour
{
    public event Action Attacked, Reloaded;
    public event Action<Weapon> SwitchedWeapon;

    [HideInInspector] public bool isAttack, isCanAttack;

    [Header("Scripts")]
    [SerializeField] private Weapon StartWeapon;

    private Weapon _weapon;

    private void OnEnable()
    {
        Attacked += Attack;
    }
    private void Start()
    {
        SwitchWeapon(StartWeapon);
    }
    private void FixedUpdate()
    {
        if(_weapon != null && isAttack && isCanAttack)
        {
            if (_weapon.WeaponSettings.AttackCount > _weapon.WeaponSettings.MinAttackCount && !_weapon.WeaponSettings.isReturn)
                Attacked?.Invoke();
            else if (!_weapon.WeaponSettings.isReturn)
                Reloaded?.Invoke();
        }
    }
    public void OnAttack(bool attacked) => isAttack = attacked;
    private void Attack() => _weapon.Attack();
    public void SwitchWeapon(Weapon weapon)
    {
        SwitchedWeapon?.Invoke(weapon);
        _weapon = weapon;
    }

    public void Reload()
    {
        if (!_weapon.WeaponSettings.isReturn)
            Reloaded?.Invoke();
    }
    private void OnDisable()
    {
        Attacked -= Attack;
    }
}
