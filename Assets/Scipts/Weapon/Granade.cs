using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Granade : Weapon
{
    public float _powerThrow;

    [HideInInspector] public bool isThrow;

    [SerializeField] private float _waitTime;

    [SerializeField] private GameObject _granade;

    [SerializeField] private GranadeLineUI _lineUI;

    private void Start()
    {
        AttackCount = MaxAttackCount;

        isReturn = false;
        isThrow = false;
    }
    private void Update()
    {
        if (!PlayerAttack.isAttack && isThrow)
        {
            isThrow = false;
            isReturn = true;

            _lineUI.OnAimGranade(false);
            _animWeapon.GranadeAnim();

            Invoke("Throw", 0.3f);
        }
    }
    public override void Attack() => Aiming();
    private void Aiming()
    {
        if (AttackCount != 0 && !isReturn)
        {
            isThrow = true;

            _lineUI.OnAimGranade(true);
        }
    }
    private void Throw()
    {
        GameObject granade = Instantiate(_granade, _lineUI._lauchPoint.position, _lineUI._lauchPoint.rotation);
        granade.GetComponent<Rigidbody>().velocity = _powerThrow * _lineUI._lauchPoint.up;

        AttackCount--;

        StartCoroutine(ReturnWait(_waitTime));
    }
}
