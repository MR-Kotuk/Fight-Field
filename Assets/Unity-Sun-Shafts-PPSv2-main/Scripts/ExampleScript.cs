using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleScript : MonoBehaviour
{
    public float spinSpeed = 4f;
    public Vector3 axis = new Vector3(0, 0, 1);

    private void Update()
    {
        transform.Rotate(axis.x * spinSpeed * Time.deltaTime, axis.y * spinSpeed * Time.deltaTime, axis.z * spinSpeed * Time.deltaTime);
    }
}
