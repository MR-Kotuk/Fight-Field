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

    private Gun _gun;

    private void OnValidate()
    {
        _gun = GetComponent<Gun>();
    }

    private void Update()
    {
        _bulletCount.text = $"{_gun.BulletCount}/{_gun.MaxBulletCount}";

        if (_gun == _playerShoot._gun)
            _weaponButton.image.color = Color.blue;
        else
            _weaponButton.image.color = Color.white;
    }
}
