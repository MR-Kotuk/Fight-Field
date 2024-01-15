using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Granade : Weapon
{
    [Header("Throw Settings")]
    [HideInInspector] public bool isThrow;
    [SerializeField] private float _powerThrow;

    [Header("Granade Settings")]
    [SerializeField] private GameObject _granade;
    [SerializeField] private Transform _createPos;
    [SerializeField] private Joystick _shootJoy;

    private float _forceY, _forceX;
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
            Throw();
        }
    }
    public override void Attack() => Aiming();
    private void Aiming()
    {
        isThrow = true;
    }
    private void Throw()
    {
        if (AttackCount != 0 && !isReturn)
        {
            _forceY = _shootJoy.Vertical;
            _forceX = _shootJoy.Horizontal;

            GameObject granade = Instantiate(_granade, _createPos.position, Quaternion.identity);
            granade.GetComponent<Rigidbody>().AddForce(new Vector3(_forceX, _forceY, 0f) * _powerThrow);

            AttackCount--;

            _animWeapon.GranadeAnim();
        }
    }
}
