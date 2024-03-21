using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : Weapon
{
    [Header("Create Bullet Settings")]
    [SerializeField] private GameObject _bullet;
    [SerializeField] private Transform _createTrn;
    [Space]

    [Header("Effects Settings")]
    public GameObject ShootFX, CatridgeCaseFX;

    [SerializeField] private ScopeWeapon _scopeWeapon;
    [Space]

    [Header("Enemy Target")]
    [SerializeField] private EnemyAI _enemyAI;

    private Vector3 _dirTo;

    private void OnEnable() => AttackWeapon.Reloaded += Reload;

    private void Start()
    {
        WeaponSettings.AttackCount = WeaponSettings.MaxAttackCount;

        WeaponSettings.isReturn = false;
    }

    public override void Attack()
    {
        if(_scopeWeapon != null)
        {
            Ray rayToShoot;

            rayToShoot = _scopeWeapon.CurrentCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            RaycastHit raycastHit;

            Vector3 toPoint;
            if (Physics.Raycast(rayToShoot, out raycastHit))
                toPoint = raycastHit.point;
            else
                toPoint = rayToShoot.GetPoint(75);

            _dirTo = toPoint - _createTrn.position;
        }
        else
        {
            Vector3 toPoint = _enemyAI.CurrentTarget;
            toPoint.y = transform.position.y;

            _dirTo = toPoint - _createTrn.position;

        }

        GameObject bullet = Instantiate(_bullet, _createTrn.position, Quaternion.identity);
        bullet.transform.forward = _dirTo.normalized;

        bullet.GetComponent<Bullet>().Damage = WeaponSettings.Damage;
        bullet?.GetComponent<Rigidbody>().AddForce(_dirTo.normalized * WeaponSettings.AttackSpeed, ForceMode.Impulse);

        WeaponSettings.AttackCount--;

        ReturnWait(WeaponSettings._waitTime);
    }

    private void OnDisable() => AttackWeapon.Reloaded -= Reload;
}
