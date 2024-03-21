using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(AnimationWeapon), typeof(EnemyWeaponAI))]
public class EnemyAI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform _safe;
    [Space]

    [Header("Move Settings")]
    [SerializeField] private float _distToSafe, _distToPlayer, _distLastPos;
    [SerializeField] private float _framesToMoveRates;
    [Space]

    [Header("Check Position Settings")]
    [SerializeField] private float _checkPosTime;
    [SerializeField] private float _rotSidesCheck;
    [SerializeField] private float _lookAroundCount;
    [Space]

    [Header("Scripts")]
    [SerializeField] private EnemyAnimation _enemyAnimation;

    [HideInInspector] public bool isPlayerInSeeZone;
    [HideInInspector] public bool isSafeTarget;

    [HideInInspector] public Vector3 CurrentTarget;

    private Vector3 _lastEnemyPos;

    private NavMeshAgent _navAgent;

    private int _frames;

    private bool isCheck;

    private void Start()
    {
        _navAgent = GetComponent<NavMeshAgent>();

        CurrentTarget = _safe.position;
        _navAgent.stoppingDistance = _distToSafe;
        _lastEnemyPos = transform.localPosition;

        isCheck = false;
        isSafeTarget = true;
    }

    private void FixedUpdate() => Move();

    private void Move()
    {
        _navAgent.SetDestination(CurrentTarget);

        if (!isPlayerInSeeZone && _navAgent.remainingDistance <= _navAgent.stoppingDistance && CurrentTarget != _safe.position && !isCheck)
        {
            isCheck = true;
            StartCoroutine(CheckLastPlayerPos());
        }

        MoveAnim();
    }

    private void MoveAnim()
    {
        _frames++;

        if (_frames >= _framesToMoveRates)
        {
            _frames = 0;

            Vector3 moved = transform.localPosition - _lastEnemyPos;

            float dirX = moved.x * _framesToMoveRates;
            float dirZ = moved.z * _framesToMoveRates;

            _enemyAnimation.Move(dirX, dirZ);

            _lastEnemyPos = transform.localPosition;
        }
    }

    private IEnumerator CheckLastPlayerPos()
    {
        for (int i = 0; i < _lookAroundCount; i++)
        {
            yield return new WaitForSeconds(_checkPosTime);
            transform.Rotate(new Vector3(0f, _rotSidesCheck, 0f));
            yield return new WaitForSeconds(_checkPosTime);

            for(int j = 0; j < 2; j++)
            {
                float rot = _rotSidesCheck * -1f;

                transform.Rotate(new Vector3(0f, rot, 0f));
                yield return new WaitForSeconds(_checkPosTime);
            }

            transform.Rotate(new Vector3(0f, _rotSidesCheck, 0f));
        }

        CurrentTarget = _safe.position;
        _navAgent.stoppingDistance = _distToSafe;

        isSafeTarget = true;
        isCheck = false;
    }

    public void OnPlayerEnter(Transform target)
    {
        isPlayerInSeeZone = true;
        isCheck = false;
        isSafeTarget = false;

        StopCoroutine(CheckLastPlayerPos());

        CurrentTarget = target.position;
        _navAgent.stoppingDistance = _distToPlayer;
    }

    public void OnPlayerExit(Vector3 lastPosition)
    {
        isPlayerInSeeZone = false;
        isSafeTarget = false;

        CurrentTarget = lastPosition;
        _navAgent.stoppingDistance = _distLastPos;
    }
}