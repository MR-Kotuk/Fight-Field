using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IWeapon
{
    public WeaponSettings WeaponSettings { get; set; }

    public Camera ScopeCamera { get; set; }
    public Camera WeaponCamera { get; set; }

    public void Attack();
    public void Reload();
    public void Enabled(bool isEnable);
}
