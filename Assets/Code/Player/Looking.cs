using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Looking : MonoBehaviour
{
    public enum RotationAxes{
        mouseXAndY = 0,
        MouseX = 1,
        MouseY = 2

    }

    private Camera cam;

    public RotationAxes axes = RotationAxes.MouseX;

    public float sensivityHor = 9.0f;
    public float sensivityVer = 9.0f;

    public float minVert = -45.0f;
    public float maxVert = 45.0f;

    private float _rotationX = 0;


    private CharacterController _charController;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Rigidbody>().freezeRotation  = true;

        cam = Camera.main;
        
        Rigidbody body = GetComponent<Rigidbody>();
        if(body != null)
            body.freezeRotation = true;

        //_charController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {

        if(axes == RotationAxes.MouseX){
            transform.Rotate(0, Input.GetAxis("Mouse X") * sensivityHor, 0);
        }
        else if(axes == RotationAxes.MouseY){
            _rotationX -= Input.GetAxis("Mouse Y") * sensivityVer;
            _rotationX = Mathf.Clamp(_rotationX, minVert, maxVert);

            float rotationY = transform.localEulerAngles.y;
            transform.localEulerAngles = new Vector3(_rotationX, rotationY, 0);
        }
        else{
            _rotationX -= Input.GetAxis("Mouse Y") * sensivityVer;
            _rotationX = Mathf.Clamp(_rotationX, minVert, maxVert);

            float delta = Input.GetAxis("Mouse X") * sensivityHor;
            float rotationY = transform.localEulerAngles.y + delta;
            transform.localEulerAngles = new Vector3(0, rotationY, 0);
            cam.transform.localEulerAngles = new Vector3(_rotationX, 0, 0);
        }
    }
}
