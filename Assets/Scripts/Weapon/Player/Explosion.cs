using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [Header("Effect Objects")]
    [SerializeField] private GameObject _effect;

    [SerializeField] private EffectWeapon _effectWeapon;
    [Space]

    [Header("Effect Settings")]
    [SerializeField] private float _timeToExplosion, _effectTime;
    [Space]

    [Header("Explosion Settings")]
    public float Damage;

    [SerializeField] private float _radius;
    [SerializeField] private float _pushPower;

    private void Awake() => Invoke("Explode", _timeToExplosion);

    private void Explode()
    {
        _effectWeapon.OneTimeFX(_effect, transform.position, _effectTime);

        Collider[] explodeRadius = Physics.OverlapSphere(transform.position, _radius);

        for(int i = 0; i < explodeRadius.Length; i++)
        {
            Rigidbody rb = explodeRadius[i].attachedRigidbody;

            if (rb)
                rb.AddExplosionForce(_pushPower, transform.position, _radius);

            IHealth health = explodeRadius[i].gameObject.GetComponent<IHealth>();

            if(health != null)
                health.Damage(Damage);
        }

        Destroy(gameObject);
    }
}
