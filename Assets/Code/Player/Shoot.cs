using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot : MonoBehaviour
{

    public GameObject bullet;
    public Camera cam;
    public float speed;

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
        if(Input.GetKeyDown(KeyCode.Mouse0)){
            Vector3 vec = cam.transform.position + cam.transform.forward;
            GameObject insBull = Instantiate(bullet, vec, Quaternion.identity);
            insBull.GetComponent<Rigidbody>().velocity = cam.transform.forward * speed;
        
            if(client != null){
                Packet packet = new Packet((int) PacketHeaders.WorldCommand.CMSG_CREATE_BULLET);
                

                Vector3 position = gameObject.transform.position;
                
                packet.Write((float) position.x);
                packet.Write((float) position.y);
                packet.Write((float) position.z);

                Quaternion rotation = gameObject.transform.rotation;
                packet.Write((float) rotation.x);
                packet.Write((float) rotation.y);
                packet.Write((float) rotation.z);

                Vector3 speed = gameObject.GetComponent<Rigidbody>().velocity;
                packet.Write((float) speed.x);
                packet.Write((float) speed.y);
                packet.Write((float) speed.z);
                
                client.stream.WriteAsync(packet.GetBytes());
            }
        }
    }
}
