using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScopeWeapon : MonoBehaviour
{
    [HideInInspector] public bool isScope;

    [HideInInspector] public Camera CurrentCamera, CurrentScope;

    [SerializeField] private PlayerAttack _playerAttack;

    [SerializeField] private Camera _playerCamera;

    [SerializeField] private Canvas _gameCanvas;

    private Weapon _curentWeapon;

    private void Start()
    {
        _playerAttack.Scoped += Scope;
        _playerAttack.SwitchedWeapon += SwitchScope;
        _playerAttack.Reloaded += OnReload;

        SwitchCamera(_playerCamera);

        CurrentScope = _playerCamera;

        CurrentCamera.enabled = true;

        isScope = false;
    }
    private void SwitchScope(Weapon weapon)
    {
        _curentWeapon = weapon;

        CurrentScope = _curentWeapon.ScopeCamera;

        SwitchCamera(_playerCamera);
    }
    private void OnReload() => StartCoroutine(Reload());

    private IEnumerator Reload()
    {
        _playerAttack.Scoped -= Scope;

        if (isScope)
        {
            Scope();
            yield return new WaitForSeconds(_curentWeapon.ReturnTime);
            Scope();
        }
        else
            yield return new WaitForSeconds(_curentWeapon.ReturnTime);

        _playerAttack.Scoped += Scope;
    }
    private void Scope()
    {
        isScope = !isScope;

        if (isScope)
            SwitchCamera(CurrentScope);
        else
            SwitchCamera(_playerCamera);
    }
    private void SwitchCamera(Camera newCamera)
    {
        if (_playerCamera == newCamera)
        {
            if(CurrentScope != null)
                CurrentScope.enabled = false;
        }
        else
            _playerCamera.enabled = false;

        newCamera.enabled = true;

        CurrentCamera = newCamera;

        _gameCanvas.worldCamera = CurrentCamera;
    }
}
