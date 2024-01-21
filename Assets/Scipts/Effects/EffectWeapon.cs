using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectWeapon : MonoBehaviour
{
    [SerializeField] private PlayerAttack _playerAttack;

    private Weapon _currentWeapon;

    private void Start()
    {
        _playerAttack.Attacked += Shoot;
        _playerAttack.SwitchedWeapon += SwitchWeapon;
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
    public static void OneTimeFX(GameObject effect, Vector3 pos, float destroyTime)
    {
        GameObject createdEffect = Instantiate(effect, pos, Quaternion.identity);
        Destroy(createdEffect, destroyTime);
    }
}
