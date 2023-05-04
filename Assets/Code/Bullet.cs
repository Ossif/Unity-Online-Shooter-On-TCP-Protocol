using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public string creatorId;
    // Start is called before the first frame update
    void Start()
    {
        Invoke("DestroyBullet", 10.0f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag != "bullet"){
            if (other.tag == "Enemy")
            {
            }
            else 
            {
                Destroy(gameObject);
            }
        }
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
