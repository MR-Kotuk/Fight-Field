using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerAttack : MonoBehaviour
{
    public event Action Attacked, Reloaded;
    public event Action<Weapon> SwitchedWeapon;

    [HideInInspector] public bool isAttack, isCanAttack;

    [Header("Scripts")]
    [SerializeField] private Weapon StartWeapon;

    private Weapon _weapon;

    private void Start()
    {
        Attacked += Attack;

        Invoke("TakeStartWeapon", 0.01f);
    }
    private void TakeStartWeapon() => SwitchWeapon(StartWeapon);
    private void FixedUpdate()
    {
        if(_weapon != null && isAttack && isCanAttack)
        {
            if (_weapon.AttackCount > 0 && !_weapon.isReturn)
                Attacked?.Invoke();
            else if (!_weapon.isReturn)
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
        if (!_weapon.isReturn)
            Reloaded?.Invoke();
    }
}
