using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GranadeLineUI : MonoBehaviour
{
    [SerializeField] private Granade _granade;

    [SerializeField] private int _linePoints = 175;
    [SerializeField] private float _intervalPoints = 0.1f;

    private LineRenderer _line;
    private Transform _lauchPoint;

    private void Start()
    {
        _line ??= GetComponent<LineRenderer>();
    }
    public void OnAimGranade(bool isDraw)
    {
        _lauchPoint = _granade._createTrn;

        if(_line != null)
        {
            if (isDraw)
            {
                DrawTrajectory();
                _line.enabled = true;
            }
            else
                _line.enabled = false;
        }
    }
    private void DrawTrajectory()
    {
        Vector3 origin = _lauchPoint.position;
        Vector3 startVelocity = _granade._powerThrow * _lauchPoint.up;

        _line.positionCount = _linePoints;

        float time = 0;

        for(int i = 0; i < _linePoints; i++)
        {
            var x = (startVelocity.x * time) + (Physics.gravity.x / 2 * time * time);
            var y = (startVelocity.y * time) + (Physics.gravity.y / 2 * time * time);
            var z = (startVelocity.z * time) + (Physics.gravity.z / 2 * time * time);

            Vector3 point = new Vector3(x, y, z);
            _line.SetPosition(i, origin + point);

            time += _intervalPoints;
        }
    }
    
}
