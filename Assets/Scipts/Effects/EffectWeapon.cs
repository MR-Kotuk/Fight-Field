using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectWeapon : MonoBehaviour
{
    public static void OneTimeEffect(GameObject effect, Vector3 pos, float destroyTime)
    {
        GameObject createdEffect = Instantiate(effect, pos, Quaternion.identity);
        Destroy(createdEffect, destroyTime);
    }
}
