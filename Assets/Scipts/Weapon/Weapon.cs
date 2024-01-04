using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WeaponsUI))]
public class Weapon : MonoBehaviour
{
    public int MaxAttackCount;
    public int AttackSpeed;

    public string Name;

    public bool isReturn = false;

    [HideInInspector] public int AttackCount;

    [SerializeField] protected PlayerAttack PlayerAttack;

    private void Start()
    {
        PlayerAttack.Reloaded += Reload;

        AttackCount = MaxAttackCount;
    }
    public virtual void Attack()
    {
        Debug.Log("Attack");
    }

    public virtual void Reload()
    {
        Debug.Log("Reload");
    }
}
