using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WeaponsUI))]
public class Weapon : MonoBehaviour
{
    public float ReturnTime;

    public int MaxAttackCount;
    public int AttackSpeed;

    public string Name;

    public bool isReturn;

    public bool isNoScope;

    public Camera ScopeCamera;

    [HideInInspector] public int AttackCount;

    [SerializeField] protected PlayerAttack PlayerAttack;

    public virtual void Attack()
    {
        Debug.Log("Attack");
    }

    public virtual void Reload()
    {
        Debug.Log("Reload");
    }
}
