using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectWeapon : MonoBehaviour
{
    public void OnEffect(GameObject effect)
    {
        effect.SetActive(true);
    }
}
