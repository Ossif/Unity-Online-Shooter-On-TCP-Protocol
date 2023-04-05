using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public Camera cam;
    private Vector3 orignalCameraPos;

    // Shake Parameters
    public float shakeDuration = 2f;
    public float shakeAmount = 0.7f;

    private bool canShake = false;
    private float _shakeTimer;

    private Movement movement;

    // Start is called before the first frame update
    void Start()
    {
        movement = this.GetComponent<Movement>();
        orignalCameraPos = cam.transform.localPosition;
    }

    void OnCollisionEnter(Collision collision)
    {
        if(movement._isGrounded == false && !Input.GetKey(KeyCode.Space)){
            ShakeCamera();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F)){
            ShakeCamera();
        }
        if (canShake)
        {
            StartCameraShakeEffect();
        }
    }

    public void ShakeCamera()
    {
        canShake = true;
        _shakeTimer = shakeDuration;
    }

    public void StartCameraShakeEffect()
    {
        if (_shakeTimer > 0)
        {
            Vector3 currPos = cam.transform.localPosition;
            Vector3 extraVec = orignalCameraPos + Random.insideUnitSphere * shakeAmount;
            currPos.y = extraVec.y; 
            cam.transform.localPosition = currPos;
            _shakeTimer -= Time.deltaTime;
        }
        else
        {
            _shakeTimer = 0f;
            cam.transform.localPosition = orignalCameraPos;
            canShake = false;
        }
    }
}
