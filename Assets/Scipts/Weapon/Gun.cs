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

    private void Start()
    {
        PlayerAttack.Reloaded += Reload;

        AttackCount = MaxAttackCount;

        isReturn = false;
    }
    public override void Attack()
    {
        Ray rayToShoot = _scopeWeapon.CurrentCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit raycastHit;

        Vector3 toPoint;
        if (Physics.Raycast(rayToShoot, out raycastHit))
            toPoint = raycastHit.point;
        else
            toPoint = rayToShoot.GetPoint(75);

        Vector3 dirTo = toPoint - _createTrn.position;

        GameObject bullet = Instantiate(_bullet, _createTrn.position, Quaternion.identity);

        bullet.transform.forward = dirTo.normalized;

        bullet?.GetComponent<Rigidbody>().AddForce(dirTo.normalized * AttackSpeed, ForceMode.Impulse);

        AttackCount--;

        StartCoroutine(ReturnWait(_waitTime));
    }
}
