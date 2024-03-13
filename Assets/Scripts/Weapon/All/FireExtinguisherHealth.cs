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
        MyHealt = MaxHealt;
    }
    public override void Damage(float damage)
    {
        if (MyHealt > 0)
        {
            if (MyHealt - damage > 0)
                MyHealt -= damage;
            else
                Deaded();
        }
        else
            Deaded();
    }

    protected override void Deaded()
    {
        _effectWeapon.OneTimeFX(_smoke, gameObject.transform.position, _smokeTime);
        Destroy(this);
    }
}
