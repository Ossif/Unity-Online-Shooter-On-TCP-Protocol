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
    private CharacterController rb = null;
    
    // Дельта-компрессия: отправляем только при изменениях
    private Vector3 lastSentPosition = Vector3.zero;
    private float lastSentRotation = 0f;
    private int lastSentAnimationId = -1;
    private float positionThreshold = 0.1f; // Минимальное изменение позиции для отправки
    private float rotationThreshold = 2f; // Минимальное изменение поворота для отправки

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
        rb = gameObject.GetComponent<CharacterController>();
        if(client == null) Debug.Log("Ошибка:Клиент не найден!");
        if(rb == null) Debug.Log("Ошибка:у обьекта нет свойства Rigidbody!");

        InvokeRepeating("SendInfo", 4f, 0.033f); // 30 раз/сек проверка, но отправка только при изменениях
    }

    // Update is called once per frame
    public void SendInfo()
    {
        if(client.readyToWork == true && client != null)
        {
            // Получаем текущие данные
            Vector3 currentPosition = gameObject.transform.position;
            float currentRotation = gameObject.transform.rotation.eulerAngles.y;
            
            //Определяем анимацию игрока
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
            if (!rb.isGrounded) {
                animationId = 5;    
            }

            // ДЕЛЬТА-КОМПРЕССИЯ: проверяем нужно ли отправлять обновление
            bool shouldSendPosition = Vector3.Distance(currentPosition, lastSentPosition) > positionThreshold;
            bool shouldSendRotation = Mathf.Abs(Mathf.DeltaAngle(currentRotation, lastSentRotation)) > rotationThreshold;
            bool shouldSendAnimation = animationId != lastSentAnimationId;
            
            // Отправляем только если есть значимые изменения
            if (shouldSendPosition || shouldSendRotation || shouldSendAnimation)
            {
                Packet packet = new Packet((int) PacketHeaders.WorldCommand.CMSG_OBJ_INFO);

                packet.Write((int)animationId);

                // Определяем какие данные отправляем (только изменившиеся)
                bool sendPos = Position && shouldSendPosition;
                bool sendRot = Rotation && shouldSendRotation;
                bool sendSpd = Speed && shouldSendPosition; // Скорость отправляем при изменении позиции
                
                byte movementInfoFlag = (byte)((sendPos ? 1 : 0) << 2 | (sendRot ? 1 : 0) << 1 | (sendSpd ? 1 : 0));
                packet.Write((byte)movementInfoFlag);

                if(sendPos)
                {
                    packet.Write((float) currentPosition.x);
                    packet.Write((float) currentPosition.y);
                    packet.Write((float) currentPosition.z);
                    lastSentPosition = currentPosition; // Сохраняем отправленную позицию
                }

                if(sendRot)
                {
                    packet.Write((float) currentRotation);
                    lastSentRotation = currentRotation; // Сохраняем отправленный поворот
                }

                if(sendSpd)
                {
                    if(rb != null){
                        Vector3 speed = rb.velocity;
                        packet.Write((float) speed.x);
                        packet.Write((float) speed.y);
                        packet.Write((float) speed.z);
                    }
                }
                
                lastSentAnimationId = animationId; // Сохраняем отправленную анимацию
                client.Send(packet);
                
                // Debug информация о сжатии
                // Debug.Log($"Delta compression: pos={sendPos}, rot={sendRot}, anim={shouldSendAnimation}");
            }
            // Если изменений нет - пакет не отправляется (экономия трафика!)
        }
    }
}
