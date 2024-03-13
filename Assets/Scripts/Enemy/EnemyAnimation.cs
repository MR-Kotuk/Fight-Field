using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    private Animator _anim;

    private const string _moveX = "MoveX", _moveZ = "MoveY";
    private const string _isAttack = "isAttack";

    private void Start()
    {
        _anim = GetComponent<Animator>();
    }
    public void Move(float dirX, float dirZ)
    {
        _anim.SetFloat(_moveX, dirX);
        _anim.SetFloat(_moveZ, dirZ);
    }
    public void Attack()
    {
        StartCoroutine(OneTimeAnim(_isAttack, 0.2f));
    }

    private IEnumerator OneTimeAnim(string animName, float waitTime)
    {
        _anim.SetBool(animName, true);
        yield return new WaitForSeconds(waitTime);
        _anim.SetBool(animName, false);
    }
}
