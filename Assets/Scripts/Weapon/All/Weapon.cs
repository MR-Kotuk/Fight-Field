using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Weapon Settings")]
    public WeaponSettings WeaponSettings;
    [Space]

    [Header("Weapon Camers")]
    public Camera ScopeCamera, WeaponCamera;
    [Space]

    [Header("Scripts")]
    [SerializeField] protected AttackWeapon AttackWeapon;
    [SerializeField] protected AnimationWeapon AnimWeapon;
    public virtual void Attack()
    {
        Debug.Log("Attack");
    }
    public void Reload()
    {
        StartCoroutine(ReturnWait(WeaponSettings.ReturnTime));
    }
    protected IEnumerator ReturnWait(float wait)
    {
        WeaponSettings.isReturn = true;

        yield return new WaitForSeconds(wait);

        WeaponSettings.isReturn = false;

        if (wait == WeaponSettings.ReturnTime)
            WeaponSettings.AttackCount = WeaponSettings.MaxAttackCount;
    }
}
