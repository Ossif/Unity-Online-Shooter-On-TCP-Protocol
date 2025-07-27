using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PacketHeaders;
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

    //Трамплин
    private Vector3 impulseDirection = Vector3.zero;
    private bool isImpulsed = false;
    private bool preventCollisionFlag = true;
    private ChatUI chat;

    //Шаги
    public AudioSource AS;
    public AudioClip[] steps = new AudioClip[4];
    private float stepTime = 0;
    public float timeToStep = 0.8f;
    private Client client;

    void Start()
    {
        client = FindObjectOfType<Client>().GetComponent<Client>();
        cc = gameObject.GetComponent<CharacterController>();
        chat = GameObject.Find("Canvas").transform.Find("Chat").GetComponent<ChatUI>();
        EnabledMovement = true;
    }

    void FixedUpdate()
    {
        Vector3 horV = new Vector3();
        Vector3 verV = new Vector3();

        if (EnabledMovement == true && chat.ChatIsOpen == false)
        {
            horV = transform.right * Speed * Input.GetAxis("Horizontal");
            verV = transform.forward * Speed * Input.GetAxis("Vertical");
        }

        float movementDirectionY = actualVel.y;
        float movementDirectionX = actualVel.x;
        float movementDirectionZ = actualVel.z;

        actualVel = verV + horV;

        if (cc.isGrounded && (Input.GetAxis("Jump") > 0) && chat.ChatIsOpen == false && EnabledMovement == true)
        {
            actualVel.y = JumpForce;
        }
        else if (!cc.isGrounded)
        {
            actualVel.y = movementDirectionY;
        }
        actualVel.y -= Gravity * Time.deltaTime;

        if (isImpulsed) { 
            if(Mathf.Abs(actualVel.x + impulseDirection.x) < Mathf.Abs(impulseDirection.x))impulseDirection.x = actualVel.x + impulseDirection.x;
            if(Mathf.Abs(actualVel.z + impulseDirection.z) < Mathf.Abs(impulseDirection.z))impulseDirection.z = actualVel.z + impulseDirection.z;  
            actualVel += impulseDirection;
            impulseDirection.y = 0;
        }
        
        float hypotenuse = Mathf.Sqrt(Input.GetAxis("Horizontal") * Input.GetAxis("Horizontal") + Input.GetAxis("Vertical") * Input.GetAxis("Vertical"));
        if(hypotenuse > 1) hypotenuse = 1;
        
        if(!cc.isGrounded) hypotenuse = 1; 

        stepTime += Time.deltaTime * hypotenuse;

        if( (actualVel.x != 0 && actualVel.z != 0) && cc.isGrounded) {
           

            if(stepTime >= timeToStep) {
                stepTime = 0;
            }
            
            if(stepTime == 0) {
                AS.PlayOneShot(steps[Random.Range(0,3)]);

                Packet p = new Packet((int) WorldCommand.CMSG_STEP);
                p.Write(0);
                client.Send(p);
            }
        }

        cc.Move(actualVel * Time.deltaTime);
    }

    public void SetPlayerPos(Vector3 newpos)
    {

        cc.enabled = false;
        this.transform.position = newpos;
        cc.enabled = true;
    }
    private void OnControllerColliderHit(ControllerColliderHit hit) {

        if(hit.gameObject.tag == "Trampline") { 
            if(isImpulsed == false) { 
                SetImpulse(hit.gameObject.GetComponent<Trampline>().impulseDirection, hit.gameObject.GetComponent<Trampline>().disableTime);
            }    
        }
        else {
            if (preventCollisionFlag && isImpulsed == true)
            {
                isImpulsed = false;
                EnableMovement();
            }
        }
    }

    public void SetImpulse(Vector3 impulse, float disableTime) { 
        impulseDirection = impulse;
        isImpulsed = true;
        preventCollisionFlag = false;

        Invoke("PreventCollision", 0.1f);
        if(disableTime > 0) { 
            EnabledMovement = false;
            Invoke("EnableMovement", disableTime);    
        }
    }

    public void PreventCollision() { 
        preventCollisionFlag = true;    
    }

    public void EnableMovement() {
            EnabledMovement = true;
            //isImpulsed = false;
    }
}
