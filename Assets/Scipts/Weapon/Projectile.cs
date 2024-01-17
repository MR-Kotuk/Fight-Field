using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float _explosionTime;
    private void Awake() => Invoke("Explosion", _explosionTime);
    private void Explosion()
    {
        Destroy(gameObject);
    }
}
