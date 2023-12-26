using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WeaponsUI))]
public class Gun : MonoBehaviour
{
    public int MaxBulletCount;

    [HideInInspector] public int BulletCount;

    [SerializeField] private GameObject Bullet;

    [SerializeField] private Transform BulletCreatePos, BulletLookPos;

    [SerializeField] private float WaitTime;
    [SerializeField] private float ReturnTime;
    [SerializeField] private float BulletSpeed;

    private bool isReturn = false;

    private void Start()
    {
        BulletCount = MaxBulletCount;
    }
    public void Shoot()
    {
        if (BulletCount > 0 && !isReturn)
        {
            GameObject bullet = Instantiate(Bullet, BulletCreatePos.position, Quaternion.identity);
            bullet.transform.LookAt(BulletLookPos);
            //bullet.GetComponent<Rigidbody>().AddForce(BulletLookPos.forward * BulletSpeed);

            BulletCount--;

            StartCoroutine(ReturnWait(WaitTime));
        }
        else
            StartCoroutine(ReturnWait(ReturnTime));
    }

    private IEnumerator ReturnWait(float wait)
    {
        isReturn = true;
        yield return new WaitForSeconds(wait);
        isReturn = false;
        if(wait == ReturnTime)
            BulletCount = MaxBulletCount;
    }
}
