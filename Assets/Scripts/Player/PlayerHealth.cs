using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : Health
{
    public override void Damage(float Damage)
    {
        base.Damage(Damage);
    }

    protected override void Deaded()
    {
        base.Deaded();
    }
}
