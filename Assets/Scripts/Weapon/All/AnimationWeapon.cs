using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AnimationWeapon : MonoBehaviour
{
    [Header("Weapons Settings")]
    [SerializeField] private List<Weapon> _weapons;
    [SerializeField] private List<GameObject> _weaponsObjects;
    [Space]

    [Header("Player")]
    [SerializeField] private Animator _anim;

    private List<string> AnimWeaponNames = new List<string>() { "Hand", "Granade", "Pistol", "M4" };

    private Dictionary<string, Weapon> _weaponsScr = new Dictionary<string, Weapon>();
    private Dictionary<string, GameObject> _weaponsObj = new Dictionary<string, GameObject>();

    private AttackWeapon _attackWeapon;
    private Weapon _currentWeapon;

    private const string _isReloadN = "isReload", _isAttackN = "isAttack", _granadeN = "Granade";

    private void OnEnable()
    {
        _attackWeapon ??= GetComponent<AttackWeapon>();

        _attackWeapon.SwitchedWeapon += SwitchAnimState;
        _attackWeapon.Reloaded += ReloadWeapon;
        _attackWeapon.Attacked += Attack;
    }

    private void Awake()
    {
        _anim ??= GetComponent<Animator>();

        for (int i = 0; i < _weapons.Count; i++)
        {
            _weaponsScr.Add(_weapons[i].WeaponSettings.Name, _weapons[i]);
            _weapons[i].enabled = false;

            if (_weaponsObjects[i] != null)
            {
                _weaponsObj.Add(_weapons[i].WeaponSettings.Name, _weaponsObjects[i]);
                _weaponsObjects[i].SetActive(false);
            }
        }
    }

    private void Attack()
    {
        if (_currentWeapon.WeaponSettings.Name != _granadeN)
            AttackAnim();
    }

    public void AttackAnim() => StartCoroutine(WithWait(_isAttackN));

    public void CanAttack(string can)
    {
        bool isCan = Convert.ToBoolean(can);
        _attackWeapon.isCanAttack = isCan;
    }

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

                if(_weaponsObj.ContainsKey(name) && _weaponsObj[name] != null)
                    _weaponsObj[name].SetActive(isTake);
            }
        }
        
    }

    private void ReloadWeapon() => StartCoroutine(WithWait(_isReloadN));

    private void SwitchAnimState(Weapon weapon)
    {
        if (_currentWeapon != null && !_currentWeapon.WeaponSettings.isNoScope && _currentWeapon.ScopeCamera != null)
            _currentWeapon.ScopeCamera.enabled = false;

        _currentWeapon = _weaponsScr[weapon.WeaponSettings.Name];

        string name = weapon.WeaponSettings.Name;

        for (int i = 0; i < AnimWeaponNames.Count; i++)
            if (_anim.GetBool($"is{AnimWeaponNames[i]}"))
                _anim.SetBool($"is{AnimWeaponNames[i]}", false);

        _anim.SetBool($"is{name}", true);
    }

    private IEnumerator WithWait(string name)
    {
        if (!_anim.GetBool(name))
        {
            _anim.SetBool(name, true);
            yield return new WaitForSeconds(0.5f);
            _anim.SetBool(name, false);
        }
    }

    private void OnDisable()
    {
        _attackWeapon.SwitchedWeapon -= SwitchAnimState;
        _attackWeapon.Reloaded -= ReloadWeapon;
        _attackWeapon.Attacked -= Attack;
    }
}
