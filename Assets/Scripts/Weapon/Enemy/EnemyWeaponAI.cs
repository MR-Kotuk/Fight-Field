using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(AttackWeapon), typeof(EnemyLook))]
public class EnemyWeaponAI : MonoBehaviour
{
    [Header("Switch Weapon Settings")]
    [SerializeField] private float _handsAttackDist;
    [SerializeField] private float _pistolAttackDist;
    [SerializeField] private float _rifleAttackDist;
    [Space]

    [Header("Weapons")]
    [SerializeField] private Weapon _hands;
    [SerializeField] private Weapon _pistol;
    [SerializeField] private Weapon _rifle;

    private NavMeshAgent _navAgent;
    private EnemyAI _enemyAI;
    private AttackWeapon _attackWeapon;

    private void Start()
    {
        _navAgent = GetComponent<NavMeshAgent>();
        _enemyAI = GetComponent<EnemyAI>();
        _attackWeapon = GetComponent<AttackWeapon>();
    }

    private void FixedUpdate()
    {
        if (_enemyAI.isPlayerInSeeZone || _enemyAI.isSafeTarget)
        {
            Vector3 target = _navAgent.destination;

            transform.LookAt(target);

            _attackWeapon.OnAttack(true);
        }
        else
            _attackWeapon.OnAttack(false);

        if (_navAgent.remainingDistance <= _handsAttackDist)
            _attackWeapon.SwitchWeapon(_hands);
        else if (_navAgent.remainingDistance <= _pistolAttackDist)
            _attackWeapon.SwitchWeapon(_pistol);
        else if (_navAgent.remainingDistance <= _rifleAttackDist)
            _attackWeapon.SwitchWeapon(_rifle);
    }
}
