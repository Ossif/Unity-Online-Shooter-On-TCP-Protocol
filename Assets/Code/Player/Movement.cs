using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEditor.Animations;

public class Movement : MonoBehaviour
{
    private CharacterController cc;
    private Rigidbody rb;

    public bool EnabledMovement = true;
    public float Speed = 0.3f;
    public float JumpForce = 1f;
    public float Gravity = 9.8f;

    private Vector3 actualVel = Vector3.zero;
    //Анимация
    public GameObject girl;

    void Start()
    {
        cc = gameObject.GetComponent<CharacterController>();
        EnabledMovement = true;
    }

    void FixedUpdate()
    {
        Vector3 horV = new Vector3();
        Vector3 verV = new Vector3();

        if (EnabledMovement == true)
        {
            horV = transform.right * Speed * Input.GetAxis("Horizontal");
            verV = transform.forward * Speed * Input.GetAxis("Vertical");
        }

        float movementDirectionY = actualVel.y;
        actualVel = verV + horV;

        if (cc.isGrounded && (Input.GetAxis("Jump") > 0))
        {
            actualVel.y = JumpForce;
        }
        else if (!cc.isGrounded)
        {
            actualVel.y = movementDirectionY;
        }
        actualVel.y -= Gravity * Time.deltaTime;
        cc.Move(actualVel * Time.deltaTime);

    }

    public void SetPlayerPos(Vector3 newpos)
    {

        cc.enabled = false;
        this.transform.position = newpos;
        cc.enabled = true;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("PickUP"))
        {
            Debug.Log("прыжок");

        }

    }
}
