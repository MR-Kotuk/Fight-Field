using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] protected float MaxHealt;

    protected float MyHealt;

    public virtual void Dammage(float dammage)
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

    protected virtual void Deaded()
    {
        Destroy(gameObject);
    }
}
