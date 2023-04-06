using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEditor.Animations;

public class Movement : MonoBehaviour
{
     public float Speed = 0.3f;
    public float JumpForce = 1f;

    private Rigidbody _rb;
    private CapsuleCollider _collider; 

    public bool _isGrounded = true;

    //Анимация
    public GameObject girl;

    /*public AnimatorController idle;
    public AnimatorController forward;
    public AnimatorController back;
    public AnimatorController left;
    public AnimatorController right;*/

    private Animator animator;
    private int animId = 0;

    void OnCollisionEnter(Collision collision)
    {
        _isGrounded = true;
    }
    
    void OnCollisionStay(Collision coll)
    {     
        _isGrounded = true;
    }

     void OnCollisionExit(Collision collision)
     {
        _isGrounded = false;
    }

    void Start()
    {
        //animator = girl.GetComponent<Animator>();

        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<CapsuleCollider>();
    }

    void FixedUpdate()
    {
        MoveLogic();
        JumpLogic();
    }

    void MoveLogic(){
        float currVelY = _rb.velocity.y;
        Vector3 actualVel = new Vector3();
        Vector3 horV = new Vector3();
        Vector3 verV = new Vector3();

        //int newAnimId = 0;

        if (Input.GetKey(KeyCode.D))
        {
            //newAnimId = 4;
            horV = transform.right * Speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A))
        {
            //newAnimId = 3;
            horV = -transform.right * Speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.W))
        {
            //newAnimId = 1;
            verV = transform.forward * Speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S))
        {
            //newAnimId = 2;
            verV = -transform.forward * Speed * Time.deltaTime;
        }
        
        actualVel = verV + horV;

        actualVel.y = currVelY;
        _rb.velocity = actualVel;

        /*if(!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.D)){
            //newAnimId = 0;
            _rb.velocity = new Vector3(0, _rb.velocity.y, 0);
        }*/

        /*if(animId != newAnimId){
            animId = newAnimId;
            switch (animId)
            {
                case 0:
                    animator.Play("Idle");
                    break;
                case 1:
                    animator.Play("RForward");
                    break;
                case 2:
                    animator.Play("RBack");
                    break;
                case 3:
                    animator.Play("RLeft");
                    break;
                case 4:
                    animator.Play("RRight");
                    break;

            }
        }*/
    }

    private void JumpLogic()
    {
        if (_isGrounded && (Input.GetAxis("Jump") > 0))
        {
            //_rb.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
            Vector3 actualVector = Vector3.up * JumpForce;
            actualVector.x = _rb.velocity.x;
            actualVector.z = _rb.velocity.z;
            _rb.velocity = actualVector;
        }
    }
}
