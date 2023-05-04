using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaponEnumIds;

public class Shoot : MonoBehaviour
{

    public GameObject bullet;
    public Camera cam;
    public ParticleSystem ShotParticle;
    public float speed;
    private float TimeToNextShot;

    private Client client = null;

    private WeaponSystem ws;

    public List<Weapon> weaponList = new List<Weapon>();

    // Start is called before the first frame update
    void Start()
    {
        try{ 
            client = FindObjectOfType<Client>().GetComponent<Client>();
        }    
        catch{ 
            
        }
        if(client == null) Debug.Log("Ошибка:Клиент не найден!");
        ws = gameObject.GetComponent<WeaponSystem>();

        WeaponEnum we = gameObject.GetComponent<WeaponEnum>();
        we.InitializeAllWeapon();
        weaponList = we.weaponList;
    }

    public void CreateBullet(int index){ 
        if(TimeToNextShot >= weaponList[index].shotTime){
            TimeToNextShot = 0;
            ws.slotAmmo[ws.currentSlot] --;
            ws.canvasController.SetAmmoLeft(ws.slotAmmo[ws.currentSlot]);
            /*TimeToNextShot = Time.time + 1f / ShotRate;
            ShotParticle.Play();
            ShotAudioSource.PlayOneShot(ShotClip);*/

            ws.handsAnimator.SetTrigger("H_" + weaponList[index].shotAnim);
            ws.weaponObject.transform.Find("model").GetComponent<Animator>().SetTrigger("shot");

            Vector3 vec = cam.transform.position + cam.transform.forward;
            if(weaponList[index].weaponId != WeaponEnumIds.WeaponId.SAWNED_OFF){ 
                GameObject insBull = Instantiate(bullet, vec, Quaternion.identity);
                insBull.GetComponent<Rigidbody>().velocity = cam.transform.forward * speed;
        
                if(client != null){
                    Packet packet = new Packet((int) PacketHeaders.WorldCommand.CMSG_CREATE_BULLET);
                

                    //Vector3 position = gameObject.transform.position;
                
                    packet.Write((float)vec.x);
                    packet.Write((float)vec.y);
                    packet.Write((float)vec.z);

                    Quaternion rotation = gameObject.transform.rotation;
                    packet.Write((float) rotation.x);
                    packet.Write((float) rotation.y);
                    packet.Write((float) rotation.z);

                    Vector3 bulletSpeed = cam.transform.forward * speed;
                    packet.Write((float) bulletSpeed.x);
                    packet.Write((float) bulletSpeed.y);
                    packet.Write((float) bulletSpeed.z);
                
                    client.Send(packet);
                }
            }
            else{ 
                for(int i = 0; i < 5; i ++){ 
                    var rotationY = Quaternion.AngleAxis(Random.Range(-5.0f,5.0f), transform.up);
                    var rotationX = Quaternion.AngleAxis(Random.Range(-5.0f,5.0f), transform.right);

                    GameObject insBull = Instantiate(bullet, vec, cam.transform.rotation * rotationX * rotationY);
                    insBull.GetComponent<Rigidbody>().velocity = insBull.transform.forward * speed;
        
                    if(client != null){
                        Packet packet = new Packet((int) PacketHeaders.WorldCommand.CMSG_CREATE_BULLET);
                

                        //Vector3 position = gameObject.transform.position;
                
                        packet.Write((float)vec.x);
                        packet.Write((float)vec.y);
                        packet.Write((float)vec.z);

                        Quaternion rotation = gameObject.transform.rotation;
                        packet.Write((float) rotation.x);
                        packet.Write((float) rotation.y);
                        packet.Write((float) rotation.z);

                        Vector3 bulletSpeed = cam.transform.forward * speed;
                        packet.Write((float) bulletSpeed.x);
                        packet.Write((float) bulletSpeed.y);
                        packet.Write((float) bulletSpeed.z);
                
                        client.Send(packet);
                    }
                }
            }
            switch (weaponList[index].weaponId){ 
                case WeaponEnumIds.WeaponId.PISTOL:{
                    ws.AS.PlayOneShot(ws.PistolShotClip);
                    break;
                }
                case WeaponEnumIds.WeaponId.AK:{
                    ws.AS.PlayOneShot(ws.AKShotClip);
                    break;
                }
                case WeaponEnumIds.WeaponId.SAWNED_OFF:{
                    ws.AS.PlayOneShot(ws.SOShotClip);
                    break;
                }
            }        
        }
    }

    // Update is called once per frame
    void Update()
    {
        /*bool isAuto = false;
        float shotTime = 0;*/
        int index = 0;
        int counter = 0;
        foreach(Weapon weapon in weaponList){ 
            if(ws.weaponSlots[ws.currentSlot] == weapon.weaponId){ 
                /*isAuto = weapon.isAuto;
                shotTime = weapon.shotTime;*/
                index = counter;
                break;
            }
            counter ++;
        }
        
        if(ws.slotAmmo[ws.currentSlot] != 0){ 
            if(weaponList[index].isAuto == true)
            { 
                if(Input.GetKey(KeyCode.Mouse0)) {
                    CreateBullet(index);
                }
                if(TimeToNextShot <= weaponList[index].shotTime) TimeToNextShot += Time.deltaTime;
            }
            else
            {
                if(Input.GetKeyDown(KeyCode.Mouse0)) 
                {
                    CreateBullet(index);
                }
                if(TimeToNextShot <= weaponList[index].shotTime) TimeToNextShot += Time.deltaTime;
            }
        }
        else{ 
            if(Input.GetKeyDown(KeyCode.Mouse0)) 
            {
                ws.AS.PlayOneShot(ws.EmptyAmmo);
            }
        }
    }
}
