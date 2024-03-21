using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISubjectWeapon
{
    public void OnSwitchWeapon(Weapon weapon);

    public void OnAttack();

    public void OnReload();
}
