using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerShoot : MonoBehaviour
{
    public event Action Shooted;
    public event Action<Gun> SwitchedGun;

    private Gun _gun;

    private bool isShoot;

    private void Start()
    {
        Shooted += Shoot;
    }

    private void FixedUpdate()
    {
        if (isShoot && _gun != null)
            Shooted?.Invoke();
    }
    public void OnShoot(bool shoot) => isShoot = shoot;
    private void Shoot() => _gun.Shoot();
    public void SwitchGun(Gun gun)
    {
        SwitchedGun?.Invoke(gun);
        _gun = gun;
    }
}
