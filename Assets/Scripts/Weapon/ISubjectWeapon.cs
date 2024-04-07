using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISubjectWeapon
{
    public void SwitchWeapon(Weapon weapon);

    public void Attack();

    public void Reload();
}
