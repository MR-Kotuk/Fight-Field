using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponSettings", menuName = "Settings/WeaponSettings")]
public class WeaponSettings : ScriptableObject
{
    [Header("Weapon Type Settings")]
    public string Name;

    public bool isNoScope;
    [Space]

    [Header("Attack Settings")]
    public int AttackSpeed;
    public int MaxAttackCount, MinAttackCount;
    [HideInInspector] public int AttackCount;

    public float Damage;
    [Space]

    [Header("Return Settings")]
    public float _waitTime;

    public float ReturnTime;

    [HideInInspector] public bool isReturn;

}