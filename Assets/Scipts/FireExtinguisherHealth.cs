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
    public override void Dammage(float dammage)
    {
        if (MyHealt > 0)
        {
            if (MyHealt - dammage > 0)
                MyHealt -= dammage;
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
