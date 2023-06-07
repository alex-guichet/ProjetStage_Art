using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunRotation : MonoBehaviour
{
    public float rotationSpeed;
    
    void Update()
    {
        float rotation = rotationSpeed * Time.deltaTime;
        transform.Rotate(rotation, -rotation, rotation);
    }
}
