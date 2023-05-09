using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    private Client client;
    public float health = 100.0f;
    // Start is called before the first frame update
    void Start()
    {
        try{ 
            client = FindObjectOfType<Client>().GetComponent<Client>();
        }    
        catch{ 
            
        }
    }

    public void SetHealth(float newHealth) {
        Debug.Log($"Player {client.name} take damage. HP now: {newHealth}");
        health = newHealth;
        GameObject.Find("Canvas").GetComponent<CanvasLogic>().SetHealth((int) Mathf.Round(health));
        if(health <= 0) { 
            PlayerDeath();
        }
    }
    public void PlayerDeath() {
        //transform.Find("FPSAnimationsObject").gameObject.SetActive(false);
        GameObject.Find("Canvas").GetComponent<CanvasLogic>().HideHUD();
        transform.Find("MainCamera").Find("FPSAnimationsObject").gameObject.SetActive(false);
        transform.gameObject.GetComponent<Movement>().EnabledMovement = false;
        Invoke("PlayerRespawn", 4.0f);
    }
    public void PlayerRespawn() {
        transform.gameObject.GetComponent<Movement>().EnabledMovement = true;
        transform.gameObject.GetComponent<Movement>().SetPlayerPos(client.SpawnPos);
        
        
        GameObject.Find("Canvas").GetComponent<CanvasLogic>().ShowHUD();
        transform.Find("MainCamera").Find("FPSAnimationsObject").gameObject.SetActive(true);

        transform.Find("MainCamera").transform.Find("FPSAnimationsObject").gameObject.SetActive(true);
        
        WeaponSystem ws = gameObject.GetComponent<WeaponSystem>();

        ws.slotAmmo[0] = 30;
        ws.slotAmmo[1] = 10;
        ws.slotAmmo[2] = 2;

        ws.maxAmmo[0] = 120;
        ws.maxAmmo[1] = 70;
        ws.maxAmmo[2] = 30;

        ws.ChangeWeapon(0);
        Packet apacket = new Packet((int)PacketHeaders.WorldCommand.CMSG_PLAYER_RESTORE_HEALTH);
        apacket.Write((float) 100.0f);
        client.Send(apacket);
        health = 100;
        SetHealth(100);
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DeadZone"))
        {
            SetHealth(0);
            /*Packet apacket = new Packet((int)PacketHeaders.WorldCommand.CMSG_PLAYER_TAKE_DAMAGE);
            apacket.Write(1000f);
            client.Send(apacket);*/
        }
        else if(other.CompareTag("PickUP"))
        {
            if (other.transform.GetComponentInParent<pickups>().type == 0)
            {
                
            }
        }
    }
}
