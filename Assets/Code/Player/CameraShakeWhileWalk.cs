using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShakeWhileWalk : MonoBehaviour
{

    public Camera cam;
    private float time = 0;
    public float A = 10.0f, B = 10.0f;
    public float speed;

    private Vector3 startPos;
    private bool _stopCamera;

    private CharacterController cc;

    // Start is called before the first frame update
    void Start()
    {
        startPos = cam.transform.localPosition;
        cc = gameObject.GetComponent<CharacterController>();
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        if(cc.isGrounded == true && (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)))
        {
            Vector3 translation = new Vector3( A * Mathf.Cos(time + Mathf.PI/2), B * Mathf.Sin(Mathf.PI/2 - 2*time), 0 );
            cam.transform.localPosition = startPos + translation;
            time += Time.deltaTime * speed;
            _stopCamera = true;
        }
        else{
            if(_stopCamera == true){
                _stopCamera = false;
                time = 0;
                cam.transform.localPosition = startPos;
            }
        }
        
    }
}
