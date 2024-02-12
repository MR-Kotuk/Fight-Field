using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : Health
{
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
        Debug.Log("I Deaded");
        Destroy(gameObject);
    }
}
