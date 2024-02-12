using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [HideInInspector] public float Dammage = 10;

    [Header("Life Settings")]
    [SerializeField] private float _destroyTime;

    private void Awake() => Destroy(gameObject, _destroyTime);

    private void OnCollisionEnter(Collision collision)
    {
        PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
        FireExtinguisherHealth fireExtinguisherHealth = collision.gameObject.GetComponent<FireExtinguisherHealth>();

        if (playerHealth != null)
            playerHealth.Dammage(Dammage);
        else if (fireExtinguisherHealth != null)
            fireExtinguisherHealth.Dammage(Dammage);

        Destroy(gameObject);
    }
}
