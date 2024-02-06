using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healt : MonoBehaviour
{
    [Header("Healt Settings")]
    public int MyHealt;

    [SerializeField] private int _maxHealt, _minHealt;

    private void OnValidate()
    {
        if (MyHealt < _minHealt)
            MyHealt = _minHealt;
        else if (MyHealt > _maxHealt)
            MyHealt = _maxHealt;
    }
}
