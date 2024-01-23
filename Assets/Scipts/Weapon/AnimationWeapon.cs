using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AnimationWeapon : MonoBehaviour
{
    [SerializeField] private List<Weapon> _weapons;
    [SerializeField] private List<GameObject> _weaponsObjects;

    private List<string> _animWeaponNames = new List<string>() { "Hand", "Granade", "Revolver", "Thompson" };

    private Dictionary<string, Weapon> _weaponsScr = new Dictionary<string, Weapon>();
    private Dictionary<string, GameObject> _weaponsObj = new Dictionary<string, GameObject>();

    private PlayerAttack _playerAttack;

    private Animator _anim;

    private Weapon _currentWeapon;

    private const string _isReloadN = "isReload", _isAttackN = "isAttack", _granadeN = "Granade";

    private void Start()
    {
        _anim ??= GetComponent<Animator>();
        _playerAttack ??= GetComponent<PlayerAttack>();

        _playerAttack.SwitchedWeapon += SwitchAnimState;
        _playerAttack.Reloaded += ReloadWeapon;
        _playerAttack.Attacked += Attack;

        for (int i = 0; i < _weapons.Count; i++)
        {
            _weaponsScr.Add(_weapons[i].Name, _weapons[i]);
            _weaponsObj.Add(_weapons[i].Name, _weaponsObjects[i]);

            _weapons[i].enabled = false;
            _weaponsObjects[i].SetActive(false);
        }
    }
    private void Attack()
    {
        if (_currentWeapon.Name != _granadeN)
            AttackAnim();
    }
    public void AttackAnim() => StartCoroutine(WithWait(_isAttackN));
    public void SwitchAnimWeapon(string name)
    {
        bool isTake;

        for (int i = 0; i < name.Length; i++)
        {
            if (name[i] == '/')
            {
                i++;

                isTake = Convert.ToBoolean(name.Substring(i, name.Length - i));
                name = name.Substring(0, name.Length - (name.Length - --i));

                _weaponsScr[name].enabled = isTake;
                _weaponsObj[name].SetActive(isTake);
            }
        }
        
    }
    private void ReloadWeapon() => StartCoroutine(WithWait(_isReloadN));

    private void SwitchAnimState(Weapon weapon)
    {
        if (_currentWeapon != null && !_currentWeapon.isNoScope)
            _currentWeapon.ScopeCamera.enabled = false;

        _currentWeapon = _weaponsScr[weapon.Name];

        string name = weapon.Name;

        for (int i = 0; i < _animWeaponNames.Count; i++)
            if (_anim.GetBool($"is{_animWeaponNames[i]}"))
                _anim.SetBool($"is{_animWeaponNames[i]}", false);

        _anim.SetBool($"is{name}", true);
    }

    private IEnumerator WithWait(string name)
    {
        _anim.SetBool(name, true);
        yield return new WaitForSeconds(0.5f);
        _anim.SetBool(name, false);
    }
}
