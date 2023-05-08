using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PacketHeaders;

public class Bullet : MonoBehaviour
{
    private Client client;
    public string creatorId;
    // Start is called before the first frame update
    void Start()
    {
        Invoke("DestroyBullet", 2.0f);
        client = FindObjectOfType<Client>().GetComponent<Client>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && client.playerId != creatorId){
            Destroy(this.gameObject);
        }
        else if(other.tag != "Player" && other.transform.GetComponent<Bullet>() == null)
        {
            Destroy(this.gameObject);
        }
    }

    void DestroyBullet()
    {
        Destroy(gameObject);
    }
}
