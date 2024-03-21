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
        if (MyHealth - damage > 0)
            MyHealth -= damage;
        else
            Dieded();
    }

    private void Dieded() => Debug.Log($"{gameObject.name} died");
}
