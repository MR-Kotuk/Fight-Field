using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerAttack : MonoBehaviour
{
    public event Action Attacked, Reloaded, Scoped;
    public event Action<Weapon> SwitchedWeapon;

    [SerializeField] private Weapon StartWeapon;

    private Weapon _weapon;

    private bool isAttack;

    private void Start()
    {
        Attacked += Attack;

        SwitchWeapon(StartWeapon);
    }

    private void FixedUpdate()
    {
        if (isAttack)
            Attacked?.Invoke();
    }
    public void Scop() => Scoped?.Invoke();
    public void OnAttack(bool attacked) => isAttack = attacked;
    private void Attack() => _weapon.Attack();
    public void SwitchWeapon(Weapon weapon)
    {
        SwitchedWeapon?.Invoke(weapon);
        _weapon = weapon;
    }

    public void Reload()
    {
        if (!_weapon.isReturn)
            Reloaded?.Invoke();
    }
}
