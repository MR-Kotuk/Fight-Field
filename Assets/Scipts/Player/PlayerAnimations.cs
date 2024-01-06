using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(PlayerMove), typeof(PlayerAttack))]
public class PlayerAnimations : MonoBehaviour
{
    [SerializeField] private GameObject _camera;

    private PlayerMove _playerMove;
    private PlayerAttack _playerAttack;
    private Animator _anim;
    private Weapon _weapon;

    private const string _moveXN = "MoveX", _moveYN = "MoveY", _camYN = "CamY";
    private const string _isJumpN = "isJump";
    private const string _isCrouchN = "isCrouch";
    private const string _isReloadN = "isReload";
    private const string _isScopeN = "isScope";

    private List<string> _animWeaponNames = new List<string>() { "Hand", "Granade", "Revolver", "Thompson" };

    private void Start()
    {
        _anim ??= GetComponent<Animator>();
        _playerMove ??= GetComponent<PlayerMove>();
        _playerAttack ??= GetComponent<PlayerAttack>();

        _playerMove.Moved += Move;
        _playerMove.Crouched += Crouch;
        _playerMove.Jumped += OnJump;

        _playerAttack.Attacked += Attack;
        _playerAttack.SwitchedWeapon += SwitchWeapon;
        _playerAttack.Reloaded += ReloadWeapon;
    }

    private void Attack()
    {
        
    }

    private void SwitchWeapon(Weapon weapon)
    {
        _weapon = weapon;
        SwitchAnimState(_weapon.Name);
    }

    private void ReloadWeapon() => StartCoroutine(WithWait(_isReloadN));

    private void SwitchAnimState(string name)
    {
        for (int i = 0; i < _animWeaponNames.Count; i++)
            if (_anim.GetBool($"is{_animWeaponNames[i]}"))
                _anim.SetBool($"is{_animWeaponNames[i]}", false);

        _anim.SetBool($"is{name}", true);
    }
    private void Move()
    {
        _anim.SetFloat(_moveXN, _playerMove._dirX);
        _anim.SetFloat(_moveYN, _playerMove._dirY);

        float camRotY = _camera.transform.eulerAngles.x;
        if (camRotY < 200)
            camRotY += 360;

        _anim.SetFloat(_camYN, camRotY);
    }
    private void Crouch()
    {
        _anim.SetBool($"isCrouch", _playerMove.isCrouch);
    }
    private void OnJump() => StartCoroutine(WithWait(_isJumpN));
    private IEnumerator WithWait(string name)
    {
        _anim.SetBool(name, true);
        yield return new WaitForSeconds(0.5f);
        _anim.SetBool(name, false);
    }
}
