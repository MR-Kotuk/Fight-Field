using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Gun))]
public class WeaponsUI : MonoBehaviour
{
    [SerializeField] private Text _bulletCount;
    [SerializeField] private Button _weaponButton;

    [SerializeField] private PlayerShoot _playerShoot;

    [SerializeField] private int _minRedBulletCount;
    private Gun _gun;

    private void Start()
    {
        _gun = GetComponent<Gun>();
        _playerShoot.SwitchedGun += SwitchGun;
        _playerShoot.Shooted += BulletCount;
    }
    private void BulletCount()
    {
        _bulletCount.text = $"{_gun.BulletCount}/{_gun.MaxBulletCount}";

        if (_gun.BulletCount <= _minRedBulletCount)
            _bulletCount.color = Color.red;
        else
            _bulletCount.color = Color.white;

    }
    private void SwitchGun(Gun gun)
    {
        if (_gun == gun)
            _weaponButton.image.color = Color.blue;
        else
            _weaponButton.image.color = Color.white;
    }
}
