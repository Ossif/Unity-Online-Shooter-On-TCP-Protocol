using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trampline : MonoBehaviour
{

    public Vector3 impulseDirection;// Направление импульса
    public float disableTime;//Время, на которое нужно деактивировать контроль над игроком

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision other)
    {
        /*Debug.Log("TRAMPLINE - test");
        if (other.gameObject.tag == "Player")
        {
            Debug.Log("TRAMPLINE");
            other.gameObject.GetComponent<Movement>().SetImpulse(impulseDirection, 2.0f);
        }*/

    }
}
