using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private GameObject _effect;

    [SerializeField] private float _explosionTime;
    [SerializeField] private float _radius;
    [SerializeField] private float _power;
    private void Awake() => Invoke("Explode", _explosionTime);
    private void Explode()
    {
        Instantiate(_effect, transform.position, Quaternion.identity);

        Collider[] explodeRadius = Physics.OverlapSphere(transform.position, _radius);

        for(int i = 0; i < explodeRadius.Length; i++)
        {
            Rigidbody rb = explodeRadius[i].attachedRigidbody;

            if (rb)
                rb.AddExplosionForce(_power, transform.position, _radius);
        }

        Destroy(gameObject);
    }
}