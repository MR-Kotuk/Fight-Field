using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Granade : Weapon
{
    [Header("Throw Settings")]
    public float _powerThrow;

    public Transform _createTrn;

    [HideInInspector] public bool isThrow;

    [SerializeField] private GameObject _granade;
    [Space]

    [Header("Graffic Settings")]
    [SerializeField] private GranadeLineUI _lineUI;

    private void Start()
    {
        WeaponSettings.AttackCount = WeaponSettings.MaxAttackCount;

        isThrow = false;
        WeaponSettings.isReturn = false;
    }
    private void FixedUpdate()
    {
        if (!AttackWeapon.isAttack && isThrow)
        {
            isThrow = false;
            WeaponSettings.isReturn = true;

            _lineUI.OnAimGranade(false);
            AnimWeapon.AttackAnim();

            Invoke("Throw", 0.3f);
        }
    }
    public override void Attack() => Aiming();
    private void Aiming()
    {
        isThrow = true;

        _lineUI.OnAimGranade(true);
    }
    private void Throw()
    {
        GameObject granade = Instantiate(_granade, _createTrn.position, _createTrn.rotation);
        granade.SetActive(true);
        granade.GetComponent<Rigidbody>().velocity = _powerThrow * _createTrn.up;
        granade.GetComponent<Explosion>().Damage = WeaponSettings.Damage;

        WeaponSettings.AttackCount--;

        StartCoroutine(ReturnWait(WeaponSettings._waitTime));
    }
}