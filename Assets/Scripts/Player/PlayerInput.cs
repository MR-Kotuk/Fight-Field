using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [Header("Keys")]
    [SerializeField] private KeyCode _shoot, _aim, _reload;
    [SerializeField] private KeyCode _acceleration, _crouch, _jump;

    [SerializeField] private List<KeyCode> _keysWeapons = new List<KeyCode>();
    [Space]

    [Header("Weapons")]
    [SerializeField] private List<Weapon> _weapons = new List<Weapon>();
    [Space]

    [Header("Scripts")]
    [SerializeField] private AttackWeapon _attackWeapon;
    [SerializeField] private PlayerMove _playerMove;

    private void Update()
    {
        for (int i = 0; i < _keysWeapons.Count; i++)
        {
            if (Input.GetKeyDown(_keysWeapons[i]))
            {
                Debug.Log("Button");
                _attackWeapon.SwitchWeapon(_weapons[i]);
            }
        }

        if (Input.GetKeyDown(_shoot))
            _attackWeapon.OnAttack(true);
        else if(Input.GetKeyUp(_shoot))
            _attackWeapon.OnAttack(false);

        if (Input.GetKey(_reload))
            _attackWeapon.Reload();

        if (Input.GetKeyDown(_crouch))
            _playerMove.OnCrouch();
        else if (Input.GetKeyUp(_crouch))
            _playerMove.OnCrouch();

        if (Input.GetKeyDown(_jump))
            _playerMove.OnJump();

        if (Input.GetKeyDown(_acceleration))
            _playerMove.Acceleration(true);
        else if (Input.GetKeyUp(_acceleration))
            _playerMove.Acceleration(false);

        if (Input.GetKeyDown(_aim))
            Debug.Log("Aiming");
        else if (Input.GetKeyUp(_aim))
            Debug.Log("No aiming");
    }
}
