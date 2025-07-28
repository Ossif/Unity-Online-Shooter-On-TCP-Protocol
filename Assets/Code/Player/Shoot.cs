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
    public float maxAngle = 15f;
    private Client client = null;

    private WeaponSystem ws;
    private AudioSource AS;
    public AudioClip GiveDamageSound;
    public GameObject TrailEffectBullet;
    public List<Weapon> weaponList = new List<Weapon>();
    #nullable enable
    struct PlayerDamage
    {
        public string? playerid;
        public float damage;
    }
    // Start is called before the first frame update
    void Start()
    {
        try{ 
            client = FindObjectOfType<Client>().GetComponent<Client>();
            AS = this.transform.Find("Audio Source").GetComponent<AudioSource>();
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
            Transform MuzzleFlash = ws.weaponObject.transform.Find("model").Find("Bone").Find("Body").Find("MuzzleFlash");
            MuzzleFlash.GetComponent<ParticleSystem>().Play();

            Vector3 vec = cam.transform.position + cam.transform.forward;
            switch (weaponList[index].weaponId)
            {
                case WeaponId.PISTOL:
                case WeaponId.AK:
                {
                    Debug.Log(cam.transform.forward);
                    Ray ray = new Ray(cam.transform.position, cam.transform.forward);
                    RaycastHit hit;
                    GameObject bullet = Instantiate(TrailEffectBullet, MuzzleFlash.position, cam.transform.rotation);
                    bullet.transform.GetComponent<Bullet>().creatorId = client.playerId;
                    bullet.transform.GetComponent<ConstantForce>().force = cam.transform.forward * 5000;

                    Packet shotPacket = new Packet((int)PacketHeaders.WorldCommand.CMSG_PLAYER_WEAPON_SHOT);
                    shotPacket.Write((float)cam.transform.position.x);
                    shotPacket.Write((float)cam.transform.position.y);
                    shotPacket.Write((float)cam.transform.position.z);

                    shotPacket.Write((float)cam.transform.forward.x);
                    shotPacket.Write((float)cam.transform.forward.y);
                    shotPacket.Write((float)cam.transform.forward.z);
                    client.Send(shotPacket);

                    Packet bpacket = new Packet((int)PacketHeaders.WorldCommand.CMSG_CREATE_BULLET_EFFECT);
                    bpacket.Write((float)cam.transform.rotation.w);
                    bpacket.Write((float)cam.transform.rotation.x);
                    bpacket.Write((float)cam.transform.rotation.y);
                    bpacket.Write((float)cam.transform.rotation.z);

                    bpacket.Write((float)cam.transform.forward.x);
                    bpacket.Write((float)cam.transform.forward.y);
                    bpacket.Write((float)cam.transform.forward.z);

                    client.Send(bpacket);

                    // Проверяем, столкнулся ли луч с каким-либо объектом
                    if (Physics.Raycast(ray, out hit))
                    {
                        // Получаем информацию об объекте, с которым столкнулся луч
                        if (hit.collider.gameObject.tag == "Enemy")
                        {
                            Invoke("GiveDamage", 0.1f);
                            EnemyInfo enemny = hit.collider.transform.GetComponent<EnemyInfo>();
                            Debug.Log($"Попал в игрока {enemny.PlayerName}");
                            Packet packet = new Packet((int)PacketHeaders.WorldCommand.CMSG_PLAYER_GIVE_DAMAGE);
                            packet.Write(enemny.playerId);
                            packet.Write(weaponList[index].damage);
                            packet.Write((byte)ws.currentSlot);
                            client.Send(packet);
                        }
                        Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
                        if (rb != null && hit.collider.gameObject.tag != "Enemy")
                        {
                            rb.AddForceAtPosition((hit.point - transform.position).normalized * 100, hit.point, ForceMode.Impulse);
                        }
                    }
                    break;
                }
                case WeaponId.SAWNED_OFF:
                {
                    PlayerDamage[] playerDamages = new PlayerDamage[5];
                    int counter = 0;
                    for (int i = 0; i < 5; i++)
                    {
                        playerDamages[0].playerid = null;
                    }
                    for (int i = 0; i < 5; i++)
                    {
                        Vector3 direction = Quaternion.Euler(Random.Range(-maxAngle, maxAngle), Random.Range(-maxAngle, maxAngle), 0) * cam.transform.forward;
                        Ray ray = new Ray(cam.transform.position, direction);
                        RaycastHit hit;
                        GameObject bullet = Instantiate(TrailEffectBullet, MuzzleFlash.position, cam.transform.rotation);
                        bullet.transform.GetComponent<Bullet>().creatorId = client.playerId;
                        bullet.transform.GetComponent<ConstantForce>().force = direction * 5000;

                        ///посылаем информацию о траектории пули для отображения
                        Packet bpacket = new Packet((int)PacketHeaders.WorldCommand.CMSG_CREATE_BULLET_EFFECT);
                        bpacket.Write((float)cam.transform.rotation.w);
                        bpacket.Write((float)cam.transform.rotation.x);
                        bpacket.Write((float)cam.transform.rotation.y);
                        bpacket.Write((float)cam.transform.rotation.z);

                        bpacket.Write((float)direction.x);
                        bpacket.Write((float)direction.y);
                        bpacket.Write((float)direction.z);

                        client.Send(bpacket);

                        // Проверяем, столкнулся ли луч с каким-либо объектом
                        if (Physics.Raycast(ray, out hit))
                        {
                            // Получаем информацию об объекте, с которым столкнулся луч
                            if (hit.collider.gameObject.tag == "Enemy")
                            {
                                EnemyInfo enemny = hit.collider.transform.GetComponent<EnemyInfo>();
                                for (int a = 0; a < 5; a++)
                                {
                                    if (playerDamages[a].playerid == null)
                                    {
                                        playerDamages[a].playerid = enemny.playerId;
                                        playerDamages[a].damage = weaponList[index].damage;
                                        counter++;
                                        break;
                                    }
                                    if (playerDamages[a].playerid == enemny.playerId)
                                    {
                                        playerDamages[a].damage += weaponList[index].damage;
                                        break;
                                    }
                                }
                            }
                            Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
                            if (rb != null && hit.collider.gameObject.tag != "Enemy")
                            {
                                rb.AddForceAtPosition((hit.point - transform.position).normalized * 500, hit.point, ForceMode.Impulse);
                            }
                        }
                        Debug.DrawRay(cam.transform.position + cam.transform.forward, direction * 5, Color.green, 5);

                    }
                    if (counter > 0)
                    {
                        Invoke("GiveDamage", 0.1f);
                        for (int i = 0; i < counter; i++)
                        {
                            Packet packet = new Packet((int)PacketHeaders.WorldCommand.CMSG_PLAYER_GIVE_DAMAGE);
                            packet.Write(playerDamages[i].playerid);
                            packet.Write(playerDamages[i].damage);
                            packet.Write((byte)ws.currentSlot);
                            client.Send(packet);
                            Debug.Log($"damage {playerDamages[i].damage} to {playerDamages[i].playerid}");
                        }
                    }
                    break;
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
    /*public void CreateBullet(int index){ 
        if(TimeToNextShot >= weaponList[index].shotTime){
            TimeToNextShot = 0;
            ws.slotAmmo[ws.currentSlot] --;
            ws.canvasController.SetAmmoLeft(ws.slotAmmo[ws.currentSlot]);
            /*TimeToNextShot = Time.time + 1f / ShotRate;
            ShotParticle.Play();
            ShotAudioSource.PlayOneShot(ShotClip);

            ws.handsAnimator.SetTrigger("H_" + weaponList[index].shotAnim);
            ws.weaponObject.transform.Find("model").GetComponent<Animator>().SetTrigger("shot");

            Vector3 vec = cam.transform.position + cam.transform.forward;
            if(weaponList[index].weaponId != WeaponEnumIds.WeaponId.SAWNED_OFF){ 
                GameObject insBull = Instantiate(bullet, vec, Quaternion.identity);
                insBull.GetComponent<Rigidbody>().velocity = cam.transform.forward * speed;
                insBull.GetComponent<Bullet>().damage = weaponList[index].damage;
                insBull.GetComponent<Bullet>().creatorId = FindObjectOfType<Client>().GetComponent<Client>().playerId;
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

                    packet.Write((float) weaponList[index].damage);
                
                    client.Send(packet);
                }
            }
            else{ 
                for(int i = 0; i < 5; i ++){ 
                    var rotationY = Quaternion.AngleAxis(Random.Range(-5.0f,5.0f), transform.up);
                    var rotationX = Quaternion.AngleAxis(Random.Range(-5.0f,5.0f), transform.right);

                    GameObject insBull = Instantiate(bullet, vec, cam.transform.rotation * rotationX * rotationY);
                    insBull.GetComponent<Rigidbody>().velocity = insBull.transform.forward * speed;
                    insBull.GetComponent<Bullet>().damage = weaponList[index].damage;
                    insBull.GetComponent<Bullet>().creatorId = FindObjectOfType<Client>().GetComponent<Client>().playerId;
        
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

                        packet.Write((float) weaponList[index].damage);
                
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
    }*/

    void GiveDamage()
    {
        AS.PlayOneShot(GiveDamageSound, 1);
    }
    void Update()
    {
        // Блокируем стрельбу во время паузы
        if (PauseMenuLogic.IsGamePaused)
            return;
            
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
