using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLook : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Transform _head;

    private bool isPlayerInSeeZone;

    private EnemyAI _enemyAI;

    private Transform _player;

    private void Start()
    {
        _enemyAI = GetComponent<EnemyAI>();
    }
    private void Update()
    {
        if (isPlayerInSeeZone)
        {
            Vector3 playerPosition = _player.position - _head.position;
            playerPosition.y = 0f;

            Ray ray = new Ray(_head.position, playerPosition);
            RaycastHit raycastHit;

            if (Physics.Raycast(ray, out raycastHit))
            {
                if (raycastHit.collider.gameObject != _player.gameObject)
                    _enemyAI.OnPlayerExit(_player.position);
                else
                    _enemyAI.OnPlayerEnter(_player);
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerHealth>() != null)
        {
            isPlayerInSeeZone = true;
            _player = other.transform;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerHealth>() != null)
        {
            isPlayerInSeeZone = false;
            _enemyAI.OnPlayerExit(_player.position);
        }
    }
}
