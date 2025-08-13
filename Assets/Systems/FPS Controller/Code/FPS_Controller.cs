using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class FPS_Controller : MonoBehaviour
{
    Rigidbody rb;
    Transform camObjectTransform;
    [SerializeField] float moveSpeed;
    [SerializeField] float jumpForce;

    Vector2 move;
    Vector3 moveForce;

    Vector2 look;
    [SerializeField] Vector2 lookVerticalConstraint;
    [SerializeField] float lookSensivity;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        camObjectTransform = this.transform.Find("PlayerCamera").gameObject.transform;
    }

    void OnMove(InputValue value)
    {
        move = value.Get<Vector2>();
        moveForce = new Vector3(move.x, 0.0f, move.y);
    }

    void OnLook(InputValue value)
    {
        look = value.Get<Vector2>();
        look = new Vector2(-look.y, look.x);
        Vector3 currentLook = camObjectTransform.localEulerAngles;
        Vector3 newLook = currentLook + new Vector3(look.x, look.y, 0.0f) * lookSensivity;
        newLook.x %= 360;
        if (Mathf.Abs(newLook.x) > 180)
        {
            if (newLook.x > 0) newLook.x -= 360;
            else newLook.x += 360;
        }
        newLook.x = (newLook.x > lookVerticalConstraint.y) ? lookVerticalConstraint.y : newLook.x;
        newLook.x = (newLook.x < lookVerticalConstraint.x) ? lookVerticalConstraint.x : newLook.x;

        camObjectTransform.localEulerAngles = newLook;
    }

    void FixedUpdate()
    {
        rb.AddForce(moveForce*moveSpeed);
    }
}
