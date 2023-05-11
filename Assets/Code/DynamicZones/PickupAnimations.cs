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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        Vector3 actPos = transform.position;
        actPos.y = Mathf.Sin(Mathf.PI/2)*height * Mathf.Sin(horSpeed * time)*height ;
        transform.position = actPos;
        transform.Rotate(0,Time.deltaTime * rotSpeed, 0);
    }
}
