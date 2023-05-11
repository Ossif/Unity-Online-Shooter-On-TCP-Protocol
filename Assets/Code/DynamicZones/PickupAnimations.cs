using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupAnimations : MonoBehaviour
{

    public float height;
    public float horSpeed;
    public float rotSpeed;

    private Vector3 pos;
    private float time;
    private Vector3 initialPos;
    void Start()
    {
        initialPos = transform.position;
    }

    void Update()
    {
        time += Time.deltaTime;

        Vector3 actPos = initialPos;
        actPos.y += Mathf.Sin(Mathf.PI / 2) * height * Mathf.Sin(horSpeed * time);

        transform.position = actPos;
        transform.Rotate(0, Time.deltaTime * rotSpeed, 0);
    }
}
