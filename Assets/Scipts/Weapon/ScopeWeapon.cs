using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ScopeWeapon : MonoBehaviour
{
    public event Action Scoped;

    [HideInInspector] public bool isScope;

    [HideInInspector] public Camera CurrentCamera;

    [SerializeField] private Camera _playerCamera, _playerWeaponCamera;

    [SerializeField] private PlayerAttack _playerAttack;

    [SerializeField] private Canvas _gameCanvas;

    [SerializeField] private float _switchTime;

    private Weapon _curentWeapon;

    private Camera _currentScope, _currentWeaponCamera;

    private void Start()
    {
        Scoped += Scope;

        _playerAttack.SwitchedWeapon += SwitchScope;
        _playerAttack.Reloaded += OnReload;

        SwitchCamera(_playerCamera);

        isScope = false;
    }
    private void Update()
    {
        if (_playerAttack.isCanAttack && !_curentWeapon.isReturn)
        {
            if (Scoped == null)
                Scoped += Scope;
        }
        else if (Scoped != null)
            Scoped -= Scope;
    }
    public void OnScope() => Scoped?.Invoke();
    private void SwitchScope(Weapon weapon)
    {
        _curentWeapon = weapon;

        SwitchCamera(_playerCamera);

        _currentScope = _curentWeapon.ScopeCamera;
        _currentWeaponCamera = _curentWeapon.WeaponCamera;
    }
    private void OnReload()
    {
        if(isScope)
            Scope();
    }
    private void Scope()
    {
        isScope = !isScope;

        if (isScope)
            SwitchCamera(_currentScope);
        else
            SwitchCamera(_playerCamera);
    }
    private void SwitchCamera(Camera newCamera)
    {
        if (_playerCamera == newCamera)
        {
            isScope = false;

            _playerWeaponCamera.enabled = true;

            if(_currentScope != null)
            {
                _currentScope.enabled = false;
                _currentWeaponCamera.enabled = false;
            }
        }
        else
        {
            if (_currentWeaponCamera != null)
                _currentWeaponCamera.enabled = true;

            _playerCamera.enabled = false;
            _playerWeaponCamera.enabled = false;
        }

        newCamera.enabled = true;

        CurrentCamera = newCamera;

        _gameCanvas.worldCamera = CurrentCamera;
    }
}
