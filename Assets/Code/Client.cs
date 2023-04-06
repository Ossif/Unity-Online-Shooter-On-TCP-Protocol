using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;
using System;
using System.Net.Sockets;
using System.Threading;
using PacketHeaders;
using System.Collections.Concurrent;

public class Client : MonoBehaviour
{
    public bool readyToWork = false;



    private Queue<Tuple<PacketDecryptor>> messageQueue = new Queue<Tuple<PacketDecryptor>>(); //Packet queue
    private string host = "127.0.0.1";
    public int port = 6321;
    public bool socketReady;
    private TcpClient socket;
    public NetworkStream stream;
    byte[] buffer;
    private static int HeaderSize = 6; //(UInt16(2) - PacketID, Uint32(4) - PacketSize)
    int CountGetPacketData = 0;
    

    public GameObject playerPrefab;
    public GameObject bulletPrefab;

    //От ФИМЫ
    public string ClientName;
    public bool IsHost;

    static ConcurrentDictionary<string, GameObject> enemies = new ConcurrentDictionary<string, GameObject>();
    //От ФИМЫ
    private int ClientId;

    // Start is called before the first frame update
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        //ConnectToServer(host, port);
    }


    private void Update()
    {
        PacketDecryptor packet = null;
        lock (messageQueue)
        {
            if (messageQueue.Count > 0)
            {
                Tuple<PacketDecryptor> packetTuple = messageQueue.Dequeue();
                packet = packetTuple.Item1;
                
            }
        }

        if (packet != null) //Если нашли какой-то пакетик
        {
            OnIncomingData(packet); //Отправляем его на обработку
        }
        else //Если нет
        {
            Thread.Sleep(10); //Спим 10 мс
        }
    }

    public async void ConnectToServer(string host, int port)//Присоединение к TCP серверу
    {
        if(socketReady)
            return;
        
        try
        {
            Debug.Log($"ConnectToServer {host}");
            socket = new TcpClient();
            await socket.ConnectAsync(host, port);

            if (socket.Connected)
            {
                stream = socket.GetStream();
                buffer = new byte[HeaderSize];
                socketReady = true;
                stream.BeginRead(buffer, 0, HeaderSize, ReadHeaderCallback, new Tuple<NetworkStream, byte[]>(stream, buffer));
            }
        }
        catch (Exception e)
        {
            Debug.Log("Socket error: " + e.Message);
        }
    }
    public static byte[] TruncateByteArray(byte[] array, int numBytes, int startIndex = 0) //Обрубает массив байтов от startIndex до startIndex + nymBytes
    {
        if (array.Length <= numBytes + startIndex)
        {
            return array;  // no truncation necessary
        }
        else
        {
            byte[] truncatedArray = new byte[numBytes];
            Array.Copy(array, startIndex, truncatedArray, 0, numBytes);
            return truncatedArray;
        }
    }
    void ReadHeaderCallback(IAsyncResult ar)
    {
        var state = (Tuple<NetworkStream, byte[]>) ar.AsyncState;
        var stream = state.Item1;
        var buffer = state.Item2;

        try
        {
            int bytesRead = stream.EndRead(ar);

            if (bytesRead == 0)
            {
                // Соединение было закрыто сервером
                Debug.Log("Сервер разорвал соединение(1)");
                stream.Close();
                return;
            }
            CountGetPacketData += bytesRead;
            
            int headerSize = (int) BitConverter.ToUInt32(buffer, 2);
            Array.Resize(ref buffer, CountGetPacketData + headerSize);
            // Обработка принятых данных
            stream.BeginRead(buffer, CountGetPacketData, headerSize, ReadDataCallback, new Tuple<NetworkStream, byte[]>(stream, buffer));
            //
        }
        catch (Exception ex)
        {
            Debug.Log($"Error: {ex.Message}");
            stream.Close();
        }
    }

    void ReadDataCallback(IAsyncResult ar)
    {
        var state = (Tuple<NetworkStream, byte[]>)ar.AsyncState;
        var stream = state.Item1;
        var buffer = state.Item2;
        try
        {
            int bytesRead = stream.EndRead(ar);

            if (bytesRead == 0)
            {
                // Соединение было закрыто сервером
                Debug.Log("Соединение закрыто(2)");
                stream.Close();
                return;
            }
            PacketDecryptor packet = new PacketDecryptor(buffer);
            CountGetPacketData = 0;

            messageQueue.Enqueue(Tuple.Create(packet));

            buffer = new byte[HeaderSize];
            stream.BeginRead(buffer, 0, HeaderSize, ReadHeaderCallback, new Tuple<NetworkStream, byte[]>(stream, buffer));
            //
        }
        catch (Exception ex)
        {
            Debug.Log($"Error: {ex.Message}");
            stream.Close();
        }
    }

    /*public void Send(string data)
    {
        if(!socketReady){
            Debug.Log("CLIENT Сообщение не удалось отправить, так как клиент не подключен к серверу, или флаг, отвечающий за это, не изменился.");
            return;
        }
        //Логика отправки
    }*/

    //Получение данных от сервера
    private void OnIncomingData(PacketDecryptor InComePacket)
    {
        int packetid = InComePacket.GetPacketId();
        //Debug.Log(packetid);
        switch ((WorldCommand) packetid)
        {
            case WorldCommand.MSG_NULL_ACTION: //Теперь используй PacketHeader.cs чтобы создать новый пакет
            {
                int playerid = InComePacket.ReadInt();
                Debug.Log("CLIENT: На клиент передали его id - " + playerid);
                Packet packet = new Packet(0);
                packet.Write(playerid);
                stream.WriteAsync(packet.GetBytes(), 0, packet.GetBytes().Length);
                ClientId = playerid;


                readyToWork = true;

                break;
            }
            case WorldCommand.SMSG_START_GAME:
            {
                GameObject.Find("MenuLogic").GetComponent<MenuLogic>().StartGame();
                break;
            }

            case WorldCommand.SMSG_PLAYER_LOGIN:{
                
                string uniqueId;
                Vector3 position = new Vector3();
                float rot;

                uniqueId = InComePacket.ReadString();

                position.x = InComePacket.ReadFloat();
                position.y = InComePacket.ReadFloat();
                position.z = InComePacket.ReadFloat();

                rot = InComePacket.ReadFloat();

                GameObject newPlayer = Instantiate(playerPrefab, position, Quaternion.identity);

                Quaternion rotation = newPlayer.transform.rotation;
                rotation.z = rot;
                newPlayer.transform.rotation = rotation;
                newPlayer.GetComponent<PlayerId>().playerId = uniqueId;

                enemies.TryAdd(uniqueId, newPlayer);
                break;
            }

            case WorldCommand.SMSG_CREATE_PLAYERS:
            {
                string id;
                Vector3 position = new Vector3();
                Quaternion rotation = new Quaternion();

                int counter = InComePacket.ReadInt();

                for (int i = 0; i < counter; i ++){

                    id = InComePacket.ReadString();
                    position.x = InComePacket.ReadFloat();
                    position.y = InComePacket.ReadFloat();
                    position.z = InComePacket.ReadFloat();
                    rotation.z = InComePacket.ReadFloat();

                    GameObject go = Instantiate(playerPrefab, position, Quaternion.identity);
                    go.transform.rotation = rotation;
                    go.GetComponent<PlayerId>().playerId = id;

                    enemies.TryAdd(id, go);
                }

                break;
            }

            case WorldCommand.SMSG_OBJ_INFO:
            {
                
                string objectId = InComePacket.ReadString();
                //Debug.Log(objectId);
                GameObject enemy = null;
                foreach (GameObject obj in enemies.Values){
                    //Debug.Log(uniqueId);
                    if(obj.GetComponent<PlayerId>().playerId == objectId) 
                    {
                        enemy = obj;
                        break;
                    }
                }

                int before = InComePacket.ReadInt();

                for(int i = 0; i < 3; i ++){
                    switch (i)
                    {
                        case 0:
                        {
                            
                            if(before / 100 >= 1){
                                //Debug.Log($"POS: {InComePacket.ReadFloat()}; {InComePacket.ReadFloat()}; {InComePacket.ReadFloat()}");
                                Vector3 position = new Vector3(InComePacket.ReadFloat(), InComePacket.ReadFloat(), InComePacket.ReadFloat());
                                enemy.transform.position = position; 
                            }
                            break;
                        }
                        case 1:
                        {
                            if((before % 100) / 10 >= 1)
                            {
                                Quaternion q = new Quaternion();
                                q.y = InComePacket.ReadFloat();
                                enemy.transform.rotation = q; 
                                //Debug.Log($"ROT: {InComePacket.ReadFloat()};");
                            
                            }
                            break;
                        }
                        case 2:
                        {
                            if(before % 10 >= 1)
                            {
                                Vector3 speed = new Vector3(InComePacket.ReadFloat(), InComePacket.ReadFloat(), InComePacket.ReadFloat());
                                enemy.GetComponent<Rigidbody>().velocity = speed;
                                //Debug.Log($"SPEED: {InComePacket.ReadFloat()}; {InComePacket.ReadFloat()}; {InComePacket.ReadFloat()}");
                            }
                            break;
                        }
                    }
                }

                break;
            }
            case WorldCommand.SMSG_CREATE_BULLET:
            {
                Vector3 position = new Vector3(InComePacket.ReadFloat(), InComePacket.ReadFloat(), InComePacket.ReadFloat());

                Quaternion rotation = new Quaternion();

                rotation.x = InComePacket.ReadFloat();
                rotation.y = InComePacket.ReadFloat();
                rotation.z = InComePacket.ReadFloat();

                Vector3 speed = new Vector3(InComePacket.ReadFloat(), InComePacket.ReadFloat(), InComePacket.ReadFloat());

                GameObject bullet = Instantiate(bulletPrefab, position, Quaternion.identity);
                bullet.transform.rotation = rotation;
                bullet.GetComponent<Rigidbody>().velocity = speed;

                break;
            }
        }
    }

    private void OnApplicationQuit()
    {
        CloseSocket();
    }
    private void OnDisable()
    {
        CloseSocket();
    }

    private void CloseSocket()
    {
        if(!socketReady)
            return;
        socket.Close();
        socketReady = false;
    }
}

public class GameClient
{
    public string name;
    public bool isHost;
}