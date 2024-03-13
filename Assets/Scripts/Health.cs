using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] protected float MaxHealt;
    [SerializeField] protected float MyHealt;

    private void Start()
    {
        MyHealt = MaxHealt;
    }
    public virtual void Damage(float damage)
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

    protected virtual void Deaded()
    {
        Debug.Log($"{gameObject.name} dieded");

        Destroy(gameObject);
    }
}
