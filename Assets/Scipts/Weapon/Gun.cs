using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WeaponsUI))]
public class Gun : MonoBehaviour
{
    public int MaxBulletCount;

    [HideInInspector] public int BulletCount;

    [SerializeField] private GameObject _bullet;

    [SerializeField] private Transform _shootTrn;
    [SerializeField] private Camera _camera;

    [SerializeField] private float _waitTime;
    [SerializeField] private float _returnTime;
    [SerializeField] private float _bulletSpeed;

    private bool isReturn = false;

    private void Start()
    {
        BulletCount = MaxBulletCount;
    }
    public void Shoot()
    {
        if (BulletCount > 0 && !isReturn)
        {
            Ray rayToShoot = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit raycastHit;

            Vector3 toPoint;
            if (Physics.Raycast(rayToShoot, out raycastHit))
                toPoint = raycastHit.point;
            else
                toPoint = rayToShoot.GetPoint(75);

            Vector3 dirTo = toPoint - _shootTrn.position;

            GameObject bullet = Instantiate(_bullet, _shootTrn.position, Quaternion.identity);

            bullet.transform.forward = dirTo.normalized;

            bullet.GetComponent<Rigidbody>().AddForce(dirTo.normalized * _bulletSpeed, ForceMode.Impulse);

            BulletCount--;

            StartCoroutine(ReturnWait(_waitTime));
        }
        else if(!isReturn)
            StartCoroutine(ReturnWait(_returnTime));
    }

    private IEnumerator ReturnWait(float wait)
    {
        isReturn = true;
        yield return new WaitForSeconds(wait);
        isReturn = false;

        if(wait == _returnTime)
            BulletCount = MaxBulletCount;
    }
}
