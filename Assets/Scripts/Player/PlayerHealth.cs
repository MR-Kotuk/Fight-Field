using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IHealth
{
    [Header("Health Settings")]
    [SerializeField] private float MaxHealth;
    [SerializeField] private float MyHealth;

    public void Damage(float damage)
    {
        MyHealth -= damage;

        if (MyHealth <= 0)
            Dieded();
    }

    private void Dieded() => Debug.Log($"{gameObject.name} died");
}
