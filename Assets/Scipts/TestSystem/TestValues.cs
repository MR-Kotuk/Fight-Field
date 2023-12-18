using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestValues
{
    public static float CheckNewValue(float newValue, float maxValue)
    {
        if (newValue < maxValue && newValue >= 0)
            return newValue;
        else
        {
            Debug.LogError($"New value is a lot of {maxValue} or a let of minimal size");
            return float.NaN;
        }
    }
}
