using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameValues", menuName = "GameSettings/GameValues")]
public class GameValues : ScriptableObject
{
    [SerializeField] private float _speedPlayer;
    [SerializeField] private float _powerJumpPlayer;
    [SerializeField] private float _maxSpeedPlayer, _maxJumpPlayer;

    [SerializeField] private float _sensitivity;
    [SerializeField] private float _maxSensitivity;


    public void OnValidate()
    {
        SetSpeedPlayer(_speedPlayer);
        SetPowerJumpPlayer(_powerJumpPlayer);
        _sensitivity = CheckNewValue(_sensitivity, _maxSensitivity);
    }
    public float GetSpeedPlayer()
    {
        return _speedPlayer;
    }
    public float GetPowerJumpPlayer()
    {
        return _powerJumpPlayer;
    }
    public float GetSensitivCam()
    {
        return _sensitivity;
    }

    public void SetSpeedPlayer(float newSpeed)
    {
        _speedPlayer = CheckNewValue(newSpeed, _maxSpeedPlayer);
    }
    public void SetPowerJumpPlayer(float newPowerJump)
    {
        _powerJumpPlayer = CheckNewValue(newPowerJump, _maxJumpPlayer);
    }

    private float CheckNewValue(float newValue, float maxValue)
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
