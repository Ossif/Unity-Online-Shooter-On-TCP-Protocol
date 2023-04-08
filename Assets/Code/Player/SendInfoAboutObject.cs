using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;
using System;
using System.Net.Sockets;
using System.Threading;
using PacketHeaders;
using System.Text;

public class SendInfoAboutObject : MonoBehaviour
{
    public bool Position = true;
    public bool Rotation = true;
    public bool Speed = true;

    private Client client = null;
    private Rigidbody rb = null;

    public static string PrintByteArray(byte[] bytes, int offset = 0)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = offset; i < bytes.Length; i++)
        {
            sb.Append(bytes[i].ToString("X2"));
            sb.Append(" ");
        }
        return sb.ToString();
    }
    // Start is called before the first frame update
    void Start()
    {
        client = FindObjectOfType<Client>().GetComponent<Client>();
        rb = gameObject.GetComponent<Rigidbody>();
        if(client == null) Debug.Log("Ошибка:Клиент не найден!");
        if(rb == null) Debug.Log("Ошибка:у обьекта нет свойства Rigidbody!");

        InvokeRepeating("SendInfo", 4f, 0.1f);
    }

    // Update is called once per frame
    async public void SendInfo()
    {
        if(client.readyToWork == true){
            if(client != null){
                Packet packet = new Packet((int) PacketHeaders.WorldCommand.CMSG_OBJ_INFO);

                //Определяем анимацию игрока, отправляем ее
                int animationId = 0;
                if (Input.GetKey(KeyCode.A)) {
                    animationId = 3;
                }
                if (Input.GetKey(KeyCode.D))
                {
                    animationId = 4;
                }
                if (Input.GetKey(KeyCode.W))
                {
                    animationId = 1;
                }
                if (Input.GetKey(KeyCode.S))
                {
                    animationId = 2;
                }
                packet.Write((int)animationId);

                //Определяем, какую информацию мы отправляем
                byte movementInfoFlag = (byte)((Position ? 1 : 0) << 2 | (Rotation ? 1 : 0) << 1 | (Speed ? 1 : 0));
                packet.Write((byte)movementInfoFlag);

                if(Position)
                {
                    Vector3 position = gameObject.transform.position;
                    
                    packet.Write((float) position.x);
                    packet.Write((float) position.y);
                    packet.Write((float) position.z);
                    //Debug.Log($"Bytes: {PrintByteArray(packet.GetBytes())}");
                }

                if(Rotation){
                    Vector3 rotation = gameObject.transform.rotation.eulerAngles;
                    packet.Write((float) rotation.y);
                }

                if(Speed)
                {
                    if(rb != null){
                        Vector3 speed = rb.velocity;
                        packet.Write((float) speed.x);
                        packet.Write((float) speed.y);
                        packet.Write((float) speed.z);
                    }

                }
                
                //client.SendMessage(packet.GetBytes());
                client.Send(packet);
            }
        }
    }
}
