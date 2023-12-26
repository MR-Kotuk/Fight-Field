using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    [HideInInspector] public Gun _gun;

    public void SwitchGun(Gun gun)
    {
        _gun = gun;
    }

    public void Shoot()
    {
        if(_gun != null)
            _gun.Shoot();
    }
}
