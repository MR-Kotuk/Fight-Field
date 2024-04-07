using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectWeapon : MonoBehaviour, ISubjectWeapon
{
    private Weapon _currentWeapon;

    public void SwitchWeapon(Weapon weapon) => _currentWeapon = weapon;

    public void Attack()
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

    public void Reload() { }
}
