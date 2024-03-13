using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectWeapon : MonoBehaviour
{
    [Header("Scripts")]
    [SerializeField] private AttackWeapon _attackWeapon;

    private Weapon _currentWeapon;

    private void OnEnable()
    {
        _attackWeapon.Attacked += Shoot;
        _attackWeapon.SwitchedWeapon += SwitchWeapon;
    }
    private void SwitchWeapon(Weapon weapon)
    {
        _currentWeapon = weapon;
    }
    private void Shoot()
    {
        if(_currentWeapon != null && _currentWeapon is Gun)
        {
            Gun gun = _currentWeapon as Gun;

            DontDestroyFX(gun.ShootFX);
            DontDestroyFX(gun.CatridgeCaseFX);
        }
    }
    private void DontDestroyFX(GameObject effect)
    {
        effect.SetActive(false);
        effect.SetActive(true);
    }
    public void OneTimeFX(GameObject effect, Vector3 pos, float destroyTime)
    {
        GameObject createdEffect = Instantiate(effect, pos, Quaternion.identity);
        Destroy(createdEffect, destroyTime);
    }
    private void OnDisable()
    {
        _attackWeapon.Attacked -= Shoot;
        _attackWeapon.SwitchedWeapon -= SwitchWeapon;
    }
}
