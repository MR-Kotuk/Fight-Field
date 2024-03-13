using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [HideInInspector] public float Damage = 10;

    [Header("Life Settings")]
    [SerializeField] private float _destroyTime;

    private void Awake() => Destroy(gameObject, _destroyTime);

    private void OnCollisionEnter(Collision collision)
    {
        Health health = collision.gameObject.GetComponent<Health>();

        if(health != null)
            health.Damage(Damage);

        Destroy(gameObject);
    }
}
