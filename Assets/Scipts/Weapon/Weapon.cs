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
    [SerializeField] protected AnimationWeapon _animWeapon;
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
