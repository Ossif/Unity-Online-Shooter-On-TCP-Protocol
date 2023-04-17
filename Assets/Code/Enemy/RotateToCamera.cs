using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateToCamera : MonoBehaviour
{
    private Transform mainCameraTransform;

    private void Start()
    {
        mainCameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        transform.LookAt(mainCameraTransform);
    }
}
