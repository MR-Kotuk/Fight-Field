using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ScopeWeapon : MonoBehaviour
{
    public event Action Scoped;

    [HideInInspector] public bool isScope;

    [HideInInspector] public Camera CurrentCamera, CurrentScope;

    [SerializeField] private PlayerAttack _playerAttack;

    [SerializeField] private PlayerMove _playerMove;

    [SerializeField] private Camera _playerCamera;

    [SerializeField] private Canvas _gameCanvas;

    private Weapon _curentWeapon;

    private void Start()
    {
        Scoped += Scope;

        _playerAttack.SwitchedWeapon += SwitchScope;
        _playerAttack.Reloaded += OnReload;

        SwitchCamera(_playerCamera);

        CurrentScope = _playerCamera;

        CurrentCamera.enabled = true;

        isScope = false;
    }
    public void OnScope() => Scoped?.Invoke();
    private void SwitchScope(Weapon weapon)
    {
        isScope = false;

        _curentWeapon = weapon;

        CurrentScope = _curentWeapon.ScopeCamera;

        SwitchCamera(_playerCamera);
    }
    private void OnReload() => StartCoroutine(Reload());

    private IEnumerator Reload()
    {
        Scoped -= Scope;

        if (isScope)
            Scope();

        yield return new WaitForSeconds(_curentWeapon.ReturnTime);

        Scoped += Scope;
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
