using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hands : Weapon
{
    private void Awake()
    {
        AttackCount = MaxAttackCount;
        isReturn = false;
    }
    public override void Attack()
    {
        base.Attack();
    }
}
