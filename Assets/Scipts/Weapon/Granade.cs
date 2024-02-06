using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Granade : Weapon
{
    [Header("Throw Settings")]
    public float _powerThrow;

    public Transform _createTrn;

    [HideInInspector] public bool isThrow;

    [SerializeField] private GameObject _granade;
    [Space]

    [Header("Graffic Settings")]
    [SerializeField] private GranadeLineUI _lineUI;

    private void Start()
    {
        AttackCount = MaxAttackCount;

        isReturn = false;
        isThrow = false;
    }
    private void FixedUpdate()
    {
        if (!PlayerAttack.isAttack && isThrow)
        {
            isThrow = false;
            isReturn = true;

            _lineUI.OnAimGranade(false);
            _animWeapon.AttackAnim();

            Invoke("Throw", 0.3f);
        }
    }
    public override void Attack() => Aiming();
    private void Aiming()
    {
        isThrow = true;

        _lineUI.OnAimGranade(true);
    }
    private void Throw()
    {
        GameObject granade = Instantiate(_granade, _createTrn.position, _createTrn.rotation);
        granade.SetActive(true);
        granade.GetComponent<Rigidbody>().velocity = _powerThrow * _createTrn.up;

        AttackCount--;

        StartCoroutine(ReturnWait(_waitTime));
    }
}
