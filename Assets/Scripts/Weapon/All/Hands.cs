using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hands : Weapon
{
    public event Action Hitted;

    [Header("Hit Settings")]
    [SerializeField] private Camera _camera;
    [SerializeField] private float _hitRange;

    private void Awake()
    {
        WeaponSettings.AttackCount = WeaponSettings.MaxAttackCount;
        WeaponSettings.isReturn = false;
    }

    public override void Attack() => base.Attack();

    public void Hit()
    {
        Hitted?.Invoke();

        Ray rayToHit;

        if (_camera != null)
            rayToHit = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        else
            rayToHit = new Ray(transform.position, transform.forward);

        RaycastHit raycastHit;

        if (Physics.Raycast(rayToHit, out raycastHit))
        {
            if (Vector3.Distance(transform.position, raycastHit.point) <= _hitRange)
            {
                IHealth health = raycastHit.collider.gameObject.GetComponent<IHealth>();

                if (health != null && raycastHit.collider.gameObject != gameObject)
                    health.Damage(WeaponSettings.Damage);
            }
        }
    }
}
