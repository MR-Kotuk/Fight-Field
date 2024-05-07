using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Weapon Settings")]
    public WeaponSettings WeaponSettings;
    [Space]

    [Header("Scripts")]
    [SerializeField] protected AttackWeapon AttackWeapon;
    [SerializeField] protected AnimationWeapon AnimWeapon;

    private float _frames;

    private float _currentWait;

    public virtual void Attack() => Debug.Log("Attack");

    public void Reload() => ReturnWait(WeaponSettings.ReturnTime);

    private void Update()
    {
        if (WeaponSettings.isReturn && _frames >= _currentWait)
        {
            WeaponSettings.isReturn = false;
            _frames = 0f;

            if (_currentWait == WeaponSettings.ReturnTime)
                WeaponSettings.AttackCount = WeaponSettings.MaxAttackCount;
        }
        else if (WeaponSettings.isReturn)
            _frames += Time.deltaTime;
    }

    protected void ReturnWait(float wait)
    {
        WeaponSettings.isReturn = true;

        _currentWait = wait;
    }
}
