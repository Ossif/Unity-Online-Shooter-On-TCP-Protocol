using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PacketHeaders;

public class Bullet : MonoBehaviour
{
    private Client client;
    public string creatorId;
    public float damage;
    // Start is called before the first frame update
    void Start()
    {
        Invoke("DestroyBullet", 10.0f);
        try{ 
            client = FindObjectOfType<Client>().GetComponent<Client>();
        }    
        catch{ 
            
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag != "bullet"){
            if (other.tag == "Enemy" && client.IsHost)
            {
                string playerId = other.GetComponent<EnemyInfo>().playerId;
                Packet apacket = new Packet((int)PacketHeaders.WorldCommand.CMSG_PLAYER_DAMAGE);
                apacket.Write(playerId);
                apacket.Write(damage);
                client.Send(apacket);
            }
            else if (other.tag == "Player" && client.IsHost)
            {
                if(client.playerId != creatorId) { 
                    string playerId = client.playerId;
                    Packet apacket = new Packet((int)PacketHeaders.WorldCommand.CMSG_PLAYER_DAMAGE);
                    apacket.Write(playerId);
                    apacket.Write(damage);
                    client.Send(apacket);
                }
            }
            Destroy(gameObject);
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
