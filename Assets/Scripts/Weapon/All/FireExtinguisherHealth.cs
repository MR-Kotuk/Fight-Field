using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireExtinguisherHealth : MonoBehaviour, IHealth
{
    [Header("Health Settings")]
    [SerializeField] private float MaxHealth;
    [SerializeField] private float MyHealth;
    [Space]

    [Header("Smoke Settings")]
    [SerializeField] private GameObject _smoke;
    [SerializeField] private float _smokeTime;
    [Space]

    [Header("Scripts")]
    [SerializeField] private EffectWeapon _effectWeapon;

    private void Start() => MyHealth = MaxHealth;

    public void Damage(float damage)
    {
        if (MyHealth - damage > 0)
            MyHealth -= damage;
        else
            Dieded();
    }

    private void Dieded()
    {
        _effectWeapon.OneTimeFX(_smoke, transform.position, _smokeTime);
        Destroy(this);
    }
}
