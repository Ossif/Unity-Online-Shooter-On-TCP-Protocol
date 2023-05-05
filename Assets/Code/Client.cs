using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using System.Net.Sockets;
using PacketHeaders;
using System.Collections.Concurrent;

public class Client : MonoBehaviour
{
    public bool readyToWork = false;



    private ConcurrentQueue<Tuple<Packet>> sendQueue = new ConcurrentQueue<Tuple<Packet>>(); //Packet send queue
    private ConcurrentQueue<Tuple<PacketDecryptor>> messageQueue = new ConcurrentQueue<Tuple<PacketDecryptor>>(); //Packet queue
    public int port = 6321;
    public bool socketReady;
    public TcpClient socket;
    public NetworkStream stream;
    byte[] buffer;
    private static int HeaderSize = 6; //(UInt16(2) - PacketID, Uint32(4) - PacketSize)
    int CountGetPacketData = 0;
    

    public GameObject playerPrefab;
    public GameObject bulletPrefab;

    //От ФИМЫ
    public string ClientName;
    public bool IsHost;
    public string playerId;

    static ConcurrentDictionary<string, GameObject> enemies = new ConcurrentDictionary<string, GameObject>();

    private Animator animator;
    //От ФИМЫ
    private int ClientId;

    // Start is called before the first frame update
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }


    private void Update()
    {
        Tuple<PacketDecryptor> packet = null;
        Tuple<Packet> SendPacket = null;

        if (messageQueue.TryDequeue(out packet))
        {
            OnIncomingData(packet.Item1); //Отправляем его на обработку

        }

        if(sendQueue.TryDequeue(out SendPacket))
        {
            byte[] packets = SendPacket.Item1.GetBytes();
            if(packets.Length > 0)
                stream.Write(packets);
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
                CloseSocket();
                SceneManager.LoadScene("Menu");
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
            CloseSocket();
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
                SceneManager.LoadScene("Menu");
                CloseSocket();
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
            CloseSocket();
        }
    }

    public void Send(Packet pack)
    {
        sendQueue.Enqueue(Tuple.Create(pack));
        return;
    }

    //Получение данных от сервера
    private void OnIncomingData(PacketDecryptor InComePacket)
    {
        int packetid = InComePacket.GetPacketId();
        //Debug.Log(packetid);
        switch ((WorldCommand) packetid)
        {
            case WorldCommand.SMSG_OFFER_ENTER: //Сервер предлагает авторизоваться
            {
                string NickName = PlayerPrefs.GetString("PlayerNick");
                int playerid = InComePacket.ReadInt();
                Debug.Log("CLIENT: На клиент передали его id - " + playerid);
                
                Packet packet = new Packet((int) WorldCommand.CMSG_OFFER_ENTER_ANSWER);
                packet.Write(playerid);
                packet.Write(NickName);
                Send(packet);

                ClientId = playerid;
                readyToWork = true;
                break;
            }
            case WorldCommand.SMSG_START_GAME: //Вход в игровой мир
            {
                playerId = InComePacket.ReadString();
                int hostInt = InComePacket.ReadInt();
                if(hostInt == 1)IsHost = true;
                else IsHost = false;
                Debug.Log($"IsPlayerHost: {IsHost}");
                GameObject.Find("MenuLogic").GetComponent<MenuLogic>().StartGame();
                break;
            }
            case WorldCommand.SMSG_PLAYER_LOGIN: //Создание вновь подключившегося игрока
            {
                
                string uniqueId;
                string PlayerName;
                Vector3 position = new Vector3();
                float rot;

                uniqueId = InComePacket.ReadString();
                PlayerName = InComePacket.ReadString();

                position.x = InComePacket.ReadFloat();
                position.y = InComePacket.ReadFloat();
                position.z = InComePacket.ReadFloat();

                rot = InComePacket.ReadFloat();

                GameObject newPlayer = Instantiate(playerPrefab, position, Quaternion.identity);

                Quaternion rotation = newPlayer.transform.rotation;
                rotation.z = rot;
                newPlayer.transform.rotation = rotation;
                newPlayer.GetComponent<EnemyInfo>().playerId = uniqueId;
                newPlayer.transform.Find("NickName").GetComponent<TMP_Text>().text = PlayerName;

                enemies.TryAdd(uniqueId, newPlayer);
                break;
            }
            case WorldCommand.SMSG_CREATE_PLAYERS: //Создание игорьков при подключении
            {
                string id;
                string PlayerName;
                Vector3 position = new Vector3();
                Quaternion rotation = new Quaternion();

                int counter = InComePacket.ReadInt();

                for (int i = 0; i < counter; i ++){

                    id = InComePacket.ReadString();
                    PlayerName = InComePacket.ReadString();
                    position.x = InComePacket.ReadFloat();
                    position.y = InComePacket.ReadFloat();
                    position.z = InComePacket.ReadFloat();
                    rotation.z = InComePacket.ReadFloat();

                    GameObject go = Instantiate(playerPrefab, position, Quaternion.identity);
                    go.transform.rotation = rotation;
                    go.GetComponent<EnemyInfo>().playerId = id;
                    go.transform.Find("NickName").GetComponent<TMP_Text>().text = PlayerName;
                    
                    enemies.TryAdd(id, go);
                }

                break;
            }
            case WorldCommand.SMSG_OBJ_INFO: //Синхронизация объектов и игроков
            {
                
                string objectId = InComePacket.ReadString();
                //Debug.Log(objectId);
                GameObject enemy = null;
                foreach (GameObject obj in enemies.Values){
                    //Debug.Log(uniqueId);
                    if(obj.GetComponent<EnemyInfo>().playerId == objectId) 
                    {
                        enemy = obj;
                        break;
                    }
                }
                if (enemy == null) break;
                int animId = InComePacket.ReadInt();
                if(enemy.GetComponent<EnemyInfo>().playerAnimId != animId) 
                {
                    animator = enemy.GetComponent<Animator>();
                    enemy.GetComponent<EnemyInfo>().playerAnimId = animId;
                    switch (animId)
                    {
                        case 0:
                            animator.Play("Idle");
                            break;
                        case 1:
                            animator.Play("RForward");
                            break;
                        case 2:
                            animator.Play("RBack");
                            break;
                        case 3:
                            animator.Play("RLeft");
                            break;
                        case 4:
                            animator.Play("RRight");
                            break;

                    }
                }

                int before = InComePacket.ReadByte();

                bool Position = (before & 0b100) != 0;
                bool Rotation = (before & 0b010) != 0;
                bool Speed = (before & 0b001) != 0;

                if(Position)
                {
                    Vector3 position = new Vector3(InComePacket.ReadFloat(), InComePacket.ReadFloat(), InComePacket.ReadFloat());
                    enemy.transform.position = position;
                }

                if(Rotation)
                {
                    Vector3 q = new Vector3(0, InComePacket.ReadFloat(), 0);
                    enemy.transform.rotation = Quaternion.Euler(q);
                }

                if(Speed)
                {
                    Vector3 speed = new Vector3(InComePacket.ReadFloat(), InComePacket.ReadFloat(), InComePacket.ReadFloat());
                    enemy.GetComponent<Rigidbody>().velocity = speed;
                }
                break;
            }
            case WorldCommand.SMSG_CREATE_BULLET: //Создание выстрела другого игрока
            {
                string objectId = InComePacket.ReadString();

                Vector3 position = new Vector3(InComePacket.ReadFloat(), InComePacket.ReadFloat(), InComePacket.ReadFloat());

                Quaternion rotation = new Quaternion();

                rotation.x = InComePacket.ReadFloat();
                rotation.y = InComePacket.ReadFloat();
                rotation.z = InComePacket.ReadFloat();

                Vector3 speed = new Vector3(InComePacket.ReadFloat(), InComePacket.ReadFloat(), InComePacket.ReadFloat());

                float damage = InComePacket.ReadFloat();

                GameObject bullet = Instantiate(bulletPrefab, position, Quaternion.identity);
                bullet.GetComponent<Bullet>().creatorId = objectId;
                bullet.transform.rotation = rotation;
                bullet.GetComponent<Rigidbody>().velocity = speed;
                bullet.GetComponent<Bullet>().damage = damage;
                bullet.GetComponent<Bullet>().creatorId = objectId;
                break;
            }
        
            case WorldCommand.SMSG_PLAYER_DAMAGE: { 
                Debug.Log($"CLIENT: received info about damage");
                float health = InComePacket.ReadFloat();
                GameObject.Find("Player(Clone)").GetComponent<HealthSystem>().SetHealth(health);
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
        SceneManager.LoadScene("Menu");
    }
}

public class GameClient
{
    public string name;
    public bool isHost;
}