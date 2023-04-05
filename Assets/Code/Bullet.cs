using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Invoke("DestroyBullet", 10.0f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            //Логика столкновения  пули и игрока
            /*other.GetComponent<Health>().health -= 30.0f;
            if(other.GetComponent<Health>().health - 30.0f <= 0)
            {
                other.GetComponent<Rigidbody>().constraints =  RigidbodyConstraints.None;
            }*/
        } 
        Destroy(gameObject);
    }

    void DestroyBullet()
    {
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
