using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    private Client client;
    public float health = 100.0f;
    public Vector3 spawnPos = new Vector3(0.0f, 3.0f, 0.0f);
    // Start is called before the first frame update
    void Start()
    {
        try{ 
            client = FindObjectOfType<Client>().GetComponent<Client>();
        }    
        catch{ 
            
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void SetHealth(float newHealth) { 
        if(newHealth < 0) health = 0;
        else health = newHealth;
        GameObject.Find("Canvas").GetComponent<CanvasLogic>().SetHealth((int) Mathf.Round(health));
        if(health <= 0) { 
            PlayerDeath();
        }
    }
    public void PlayerDeath() {
        //transform.Find("FPSAnimationsObject").gameObject.SetActive(false);
        GameObject.Find("Canvas").GetComponent<CanvasLogic>().HideHUD();
        transform.Find("MainCamera").Find("FPSAnimationsObject").gameObject.SetActive(false);
        transform.gameObject.GetComponent<Movement>().enabled = false;
        Invoke("PlayerRespawn", 4.0f);
    }
    public void PlayerRespawn() {
        WeaponSystem ws = gameObject.GetComponent<WeaponSystem>();

        ws.slotAmmo[0] = 30;
        ws.slotAmmo[1] = 10;
        ws.slotAmmo[2] = 2;

        ws.maxAmmo[0] = 120;
        ws.maxAmmo[1] = 70;
        ws.maxAmmo[2] = 30;

        ws.ChangeWeapon(0);

        GameObject.Find("Canvas").GetComponent<CanvasLogic>().ShowHUD();
        transform.Find("MainCamera").Find("FPSAnimationsObject").gameObject.SetActive(true);
        transform.gameObject.GetComponent<Movement>().enabled = true;
        //transform.Find("FPSAnimationsObject").gameObject.SetActive(true);
        transform.position = spawnPos;

        Packet apacket = new Packet((int)PacketHeaders.WorldCommand.CMSG_PLAYER_RESTORE_HEALTH);
        apacket.Write((string) client.playerId);
        apacket.Write((float) 100.0f);
        client.Send(apacket);
        health = 100;
        SetHealth(100);
    }
}
