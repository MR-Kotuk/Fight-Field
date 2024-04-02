using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : Weapon
{
    [Header("Effects Settings")]
    public GameObject ShootFX, CatridgeCaseFX;

    [SerializeField] private ScopeWeapon _scopeWeapon;

    private void OnEnable() => AttackWeapon.Reloaded += Reload;

    private void Start()
    {
        WeaponSettings.AttackCount = WeaponSettings.MaxAttackCount;

        WeaponSettings.isReturn = false;
    }

    public override void Attack()
    {
        Ray rayToShoot;

        rayToShoot = _scopeWeapon.CurrentCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        RaycastHit raycastHit;

        if (Physics.Raycast(rayToShoot, out raycastHit))
        {
            IHealth health = raycastHit.collider.gameObject.GetComponent<IHealth>();

            if (health != null)
                health.Damage(WeaponSettings.Damage);
        }

        WeaponSettings.AttackCount--;

        ReturnWait(WeaponSettings._waitTime);
    }

    private void OnDisable() => AttackWeapon.Reloaded -= Reload;
}
