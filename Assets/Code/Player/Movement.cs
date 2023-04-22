using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEditor.Animations;

public class Movement : MonoBehaviour
{
    private CharacterController cc;
    private Rigidbody rb;

    public float Speed = 0.3f;
    public float JumpForce = 1f;
    public float Gravity = 9.8f;
    
    private Vector3 actualVel = Vector3.zero;
    //Анимация
    public GameObject girl;

    void Start()
    {
        cc = gameObject.GetComponent<CharacterController>();
    }

    void FixedUpdate()
    {

        
        Vector3 horV = new Vector3();
        Vector3 verV = new Vector3();

        horV = transform.right * Speed * Input.GetAxis("Horizontal");
        verV = transform.forward * Speed * Input.GetAxis("Vertical");
        
        float movementDirectionY = actualVel.y;
        actualVel = verV + horV;
        
        if (cc.isGrounded && (Input.GetAxis("Jump") > 0))
        {
            actualVel.y = JumpForce;
        }
        else {
            actualVel.y = movementDirectionY;
        }
        actualVel.y -= Gravity * Time.deltaTime;
        cc.Move(actualVel * Time.deltaTime);
    }
}
