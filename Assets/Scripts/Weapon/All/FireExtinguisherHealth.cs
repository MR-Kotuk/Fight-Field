using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireExtinguisherHealth : Health
{
    [Header("Smoke Settings")]
    [SerializeField] private GameObject _smoke;
    [SerializeField] private float _smokeTime;
    [Space]

    [Header("Scripts")]
    [SerializeField] private EffectWeapon _effectWeapon;

    private void Start()
    {
        MyHealth = MaxHealth;
    }
    public override void Damage(float damage)
    {
        if (MyHealth > 0)
        {
            if (MyHealth - damage > 0)
                MyHealth -= damage;
            else
                Dieded();
        }
        else
            Dieded();
    }

    protected override void Dieded()
    {
        _effectWeapon.OneTimeFX(_smoke, gameObject.transform.position, _smokeTime);
        Destroy(this);
    }
}
