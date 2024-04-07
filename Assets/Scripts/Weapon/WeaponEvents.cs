using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WeaponEvents : MonoBehaviour
{
    [SerializeField] private AttackWeapon _attackWeapon;

    private void Awake()
    {
        ISubjectWeapon[] subjectWeapons = GetComponentsInChildren<ISubjectWeapon>(true);

        foreach (var observer in subjectWeapons)
        {
            _attackWeapon.Attacked += observer.Attack;
            _attackWeapon.SwitchedWeapon += observer.SwitchWeapon;
            _attackWeapon.Reloaded += observer.Reload;
        }
    }
}
