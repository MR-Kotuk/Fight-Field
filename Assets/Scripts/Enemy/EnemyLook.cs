using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLook : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Transform _head;
    [SerializeField] private Transform _playerHead;
    [Space]

    [Header("Look Settings")]
    [SerializeField] private float _distToSmoke;

    private bool isPlayerInSeeZone;
    private bool isSmokeInSeeZone;

    private EnemyAI _enemyAI;

    private Vector3 _smokePos;

    private void Start() => _enemyAI = GetComponent<EnemyAI>();

    private void Update()
    {
        if (isSmokeInSeeZone && Vector3.Distance(transform.position, _smokePos) <= _distToSmoke)
        {
            Debug.Log("Player lost");

            isPlayerInSeeZone = false;
            isSmokeInSeeZone = false;

            _enemyAI.OnPlayerExit(transform.position);
        }

        if (isPlayerInSeeZone)
        {
            Vector3 playerPosition = _playerHead.position - _head.position;

            if (TryRay(_head.position, playerPosition).transform.gameObject != _playerHead.gameObject)
                _enemyAI.OnPlayerExit(_playerHead.position);
            else
                _enemyAI.OnPlayerEnter(_playerHead);

            Debug.DrawRay(_head.position, playerPosition);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerHealth>() != null)
        {
            Vector3 playerPosition = other.gameObject.transform.position - _head.position;

            if (TryRay(_head.position, playerPosition).transform.gameObject == other.gameObject)
                isPlayerInSeeZone = true;
        }

        if (other.gameObject.GetComponent<ParticleSystem>() != null && isPlayerInSeeZone)
        {
            isSmokeInSeeZone = true;
            _smokePos = other.transform.position;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<ParticleSystem>() != null && isPlayerInSeeZone)
            isSmokeInSeeZone = false;
    }

    private RaycastHit TryRay(Vector3 origin, Vector3 direction)
    {
        Ray ray = new Ray(origin, direction);
        RaycastHit raycastHit;

        Physics.Raycast(ray, out raycastHit);

        return raycastHit;
    }
}
