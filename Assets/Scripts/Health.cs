using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] protected float MaxHealth;
    [SerializeField] protected float MyHealth;

    private void Start()
    {
        MyHealth = MaxHealth;
    }
    public virtual void Damage(float damage)
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

    protected virtual void Dieded()
    {
        Debug.LogError($"{gameObject.name} dieded");

        Destroy(gameObject);
    }
}
