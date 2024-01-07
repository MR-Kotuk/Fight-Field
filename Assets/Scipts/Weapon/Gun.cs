using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : Weapon
{
    [SerializeField] private ScopeWeapon _scopeWeapon;

    [SerializeField] private GameObject _bullet;

    [SerializeField] private Transform _fireTrn;

    [SerializeField] private float _waitTime;

    private void Start()
    {
        PlayerAttack.Reloaded += Reload;

        AttackCount = MaxAttackCount;

        isReturn = false;
    }
    public override void Attack()
    {
        if (AttackCount > 0 && !isReturn)
        {
            Ray rayToShoot = _scopeWeapon.CurrentCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit raycastHit;

            Vector3 toPoint;
            if (Physics.Raycast(rayToShoot, out raycastHit))
                toPoint = raycastHit.point;
            else
                toPoint = rayToShoot.GetPoint(75);

            Vector3 dirTo = toPoint - _fireTrn.position;

            GameObject bullet = Instantiate(_bullet, _fireTrn.position, Quaternion.identity);

            bullet.transform.forward = dirTo.normalized;

            bullet?.GetComponent<Rigidbody>().AddForce(dirTo.normalized * AttackSpeed, ForceMode.Impulse);

            AttackCount--;

            StartCoroutine(ReturnWait(_waitTime));
        }
        else if (!isReturn)
            PlayerAttack.Reload();
    }

    public override void Reload()
    {
        StartCoroutine(ReturnWait(ReturnTime));
    }
    private IEnumerator ReturnWait(float wait)
    {
        isReturn = true;

        yield return new WaitForSeconds(wait);

        isReturn = false;

        if (wait == ReturnTime)
            AttackCount = MaxAttackCount;
    }
}
