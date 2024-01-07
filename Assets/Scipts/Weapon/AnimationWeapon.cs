using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationWeapon : MonoBehaviour
{
    [SerializeField] private List<Weapon> _weapons;
    [SerializeField] private List<GameObject> _weaponsObjects;

    private List<string> _animWeaponNames = new List<string>() { "Hand", "Granade", "Revolver", "Thompson" };

    private Dictionary<Weapon, GameObject> _weaponsObj = new Dictionary<Weapon, GameObject>();

    private PlayerAttack _playerAttack;
    private Animator _anim;
    private Weapon _currentWeapon;

    private const string _isReloadN = "isReload";

    private void Start()
    {
        _anim ??= GetComponent<Animator>();
        _playerAttack ??= GetComponent<PlayerAttack>();

        _playerAttack.SwitchedWeapon += SwitchWeapon;
        _playerAttack.Reloaded += ReloadWeapon;

        for (int i = 0; i < _weapons.Count; i++)
        {
            _weaponsObj.Add(_weapons[i], _weaponsObjects[i]);
            _weapons[i].enabled = false;
            _weaponsObjects[i].SetActive(false);
        }
    }

    private void SwitchWeapon(Weapon weapon)
    {
        if (_currentWeapon != null)
        {
            _weaponsObj[_currentWeapon].SetActive(false);
            _currentWeapon.enabled = false;
        }

        _currentWeapon = weapon;

        if (_weaponsObj.ContainsKey(_currentWeapon))
        {
            _weaponsObj[_currentWeapon].SetActive(true);
            _currentWeapon.enabled = true;
        }

        SwitchAnimState(_currentWeapon.Name);
    }

    private void ReloadWeapon() => StartCoroutine(WithWait(_isReloadN));

    private void SwitchAnimState(string name)
    {
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
