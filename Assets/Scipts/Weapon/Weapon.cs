using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WeaponsUI))]
public class Weapon : MonoBehaviour
{
    [Header("Weapon Type Settings")]
    public string Name;

    public bool isNoScope;
    [Space]

    [Header("Attack Settings")]
    public int AttackSpeed;
    public int MaxAttackCount, MinAttackCount;
    [HideInInspector] public int AttackCount;
    [Space]

    [Header("Return Settings")]
    public float ReturnTime;

    [HideInInspector] public bool isReturn;

    [SerializeField] protected float _waitTime;
    [Space]

    [Header("Weapon Camers Settings")]
    public Camera ScopeCamera, WeaponCamera;
    [Space]

    [Header("Scripts")]
    [SerializeField] protected PlayerAttack PlayerAttack;
    [SerializeField] protected AnimationWeapon _animWeapon;

    private void OnValidate()
    {
        if (AttackCount < MinAttackCount)
            AttackCount = MinAttackCount;
        else if (AttackCount > MaxAttackCount)
            AttackCount = MaxAttackCount;
    }
    public virtual void Attack()
    {
        Debug.Log("Attack");
    }
    public void Reload()
    {
        StartCoroutine(ReturnWait(ReturnTime));
    }
    protected IEnumerator ReturnWait(float wait)
    {
        isReturn = true;

        yield return new WaitForSeconds(wait);

        isReturn = false;

        if (wait == ReturnTime)
            AttackCount = MaxAttackCount;
    }
}
