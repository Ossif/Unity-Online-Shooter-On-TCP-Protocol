using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot : MonoBehaviour
{

    public GameObject bullet;
    public Camera cam;
    public ParticleSystem ShotParticle;
    public AudioSource ShotAudioSource;
    public AudioClip ShotClip;
    public float ShotRate = 15f;
    public float speed;
    private float TimeToNextShot;

    private Client client = null;

    // Start is called before the first frame update
    void Start()
    {
        client = FindObjectOfType<Client>().GetComponent<Client>();
        if(client == null) Debug.Log("Ошибка:Клиент не найден!");
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.Mouse0) && Time.time >= TimeToNextShot) 
        {
            TimeToNextShot = Time.time + 1f / ShotRate;
            ShotParticle.Play();
            ShotAudioSource.PlayOneShot(ShotClip);





            /*Vector3 vec = cam.transform.position + cam.transform.forward;
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
            }*/
        }
    }
}
