using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShakeAfterFall : MonoBehaviour
{

    public float time;
    private float deltaTime;
    public float speed;
    public float height;

    public Camera cam;
    private Vector3 startPos;
    
    private bool _canChangeGrounded;
    private bool _isGrounded;
    private bool _isAnimate;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        startPos = cam.transform.localPosition;
    }

    void OnCollisionStay(Collision collision)
    {
        if(_canChangeGrounded == true){
            _isGrounded = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        _canChangeGrounded = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(_isGrounded && !Input.GetKey(KeyCode.Space)){
            _isGrounded = false;
            _canChangeGrounded = false;
            _isAnimate = true;
        }
        if(_isAnimate == true){
            if(deltaTime >= time){
                cam.transform.localPosition = startPos;
                _isAnimate = false;
                deltaTime = 0;
            }
            else{
                Vector3 translate = new Vector3(0, height * Mathf.Sin(Mathf.PI + deltaTime * speed), 0); 
                cam.transform.localPosition = startPos + translate;
                deltaTime += Time.deltaTime;
                
            }
        }
    }
}
