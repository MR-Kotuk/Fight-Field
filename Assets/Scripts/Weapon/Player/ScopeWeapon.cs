using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ScopeWeapon : MonoBehaviour
{
    public event Action Scoped;

    [HideInInspector] public bool isScope;

    [Header("Game Camers Settings")]
    [HideInInspector] public Camera CurrentCamera;

    [SerializeField] private Camera _playerCamera, _playerWeaponCamera;

    private Camera _currentScope, _currentWeaponCamera;
    [Space]

    [Header("Scripts")]
    [SerializeField] private AttackWeapon _AttackWeapon;

    private Weapon _curentWeapon;
    [Space]

    [Header("UI Settings")]
    [SerializeField] private Canvas _gameCanvas;

    public void OnScope() => Scoped?.Invoke();

    private void OnEnable()
    {
        Scoped += Scope;

        _AttackWeapon.SwitchedWeapon += SwitchScope;
        _AttackWeapon.Reloaded += OnReload;
    }
    private void Start()
    {
        SwitchCamera(_playerCamera);
    }
    private void Update()
    {
        if (_AttackWeapon.isCanAttack && !_curentWeapon.WeaponSettings.isReturn)
        {
            if (Scoped == null)
                Scoped += Scope;
        }
        else if (Scoped != null)
            Scoped -= Scope;
    }
    private void SwitchScope(Weapon weapon)
    {
        _curentWeapon = weapon;

        if (isScope)
            Scope();

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
    private void OnDisable()
    {
        Scoped -= Scope;

        _AttackWeapon.SwitchedWeapon -= SwitchScope;
        _AttackWeapon.Reloaded -= OnReload;
    }
}
