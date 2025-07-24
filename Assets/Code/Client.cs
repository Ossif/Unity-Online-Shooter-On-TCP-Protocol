using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using System.Net.Sockets;
using PacketHeaders;
using System.Collections.Concurrent;
using WeaponEnumIds;


public class Client : MonoBehaviour
{
    public bool readyToWork = false;



    public GameObject AK;
    public GameObject SO;
    public GameObject pistol;



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

    public string ClientName;
    public bool IsHost;
    public string playerId;
    public Vector3 SpawnPos;

    static ConcurrentDictionary<string, GameObject> enemies = new ConcurrentDictionary<string, GameObject>();
    static List<(GameObject, NPC)> NPCarray = new List<(GameObject, NPC)>();

    private Animator animator;

    private int ClientId;

    public GameObject TrailEffectBullet;
    public AudioClip ShotClip;

    public AudioClip[] steps = new AudioClip[4];
    public GameObject NPCprefab;

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
                
                    SpawnPos = new Vector3(InComePacket.ReadFloat(), InComePacket.ReadFloat(), InComePacket.ReadFloat());
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
                    int weaponId;

                    int counter = InComePacket.ReadInt();

                    for (int i = 0; i < counter; i ++){

                        id = InComePacket.ReadString();
                        PlayerName = InComePacket.ReadString();
                        position.x = InComePacket.ReadFloat();
                        position.y = InComePacket.ReadFloat();
                        position.z = InComePacket.ReadFloat();
                        rotation.z = InComePacket.ReadFloat();
                        weaponId = InComePacket.ReadInt();

                        GameObject go = Instantiate(playerPrefab, position, Quaternion.identity);
                        go.transform.rotation = rotation;
                        go.GetComponent<EnemyInfo>().playerId = id;
                        go.GetComponent<EnemyInfo>().PlayerName = PlayerName;
                        go.transform.Find("NickName").GetComponent<TMP_Text>().text = PlayerName;
                    
                        go.GetComponent<EnemyInfo>().weaponId = (WeaponId) weaponId;

                        switch (weaponId) { 
                            case ((int) WeaponId.AK): { 
                                go.GetComponent<EnemyInfo>().WeaponObject = Instantiate(AK, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0));
                                go.GetComponent<EnemyInfo>().WeaponObject.transform.parent =  go.GetComponent<EnemyInfo>().WeaponParentBone.transform;
                                go.GetComponent<EnemyInfo>().WeaponObject.transform.localPosition = new Vector3(0, 0, 0);
                                go.GetComponent<EnemyInfo>().WeaponObject.transform.rotation = new Quaternion(0, 0, 0, 0);
                                //ei.WeaponObject.transform.
                                break;
                            }
                            case ((int) WeaponId.PISTOL): { 
                                go.GetComponent<EnemyInfo>().WeaponObject = Instantiate(pistol, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0));
                                go.GetComponent<EnemyInfo>().WeaponObject.transform.parent = go.GetComponent<EnemyInfo>().WeaponParentBone.transform;
                                go.GetComponent<EnemyInfo>().WeaponObject.transform.localPosition = new Vector3(0, 0, 0);
                                go.GetComponent<EnemyInfo>().WeaponObject.transform.rotation = new Quaternion(0, 0, 0, 0);
                                break;
                            }
                            case ((int) WeaponId.SAWNED_OFF): { 
                                go.GetComponent<EnemyInfo>().WeaponObject = Instantiate(SO, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0));
                                go.GetComponent<EnemyInfo>().WeaponObject.transform.parent =  go.GetComponent<EnemyInfo>().WeaponParentBone.transform;
                                go.GetComponent<EnemyInfo>().WeaponObject.transform.localPosition = new Vector3(0, 0, 0);
                                go.GetComponent<EnemyInfo>().WeaponObject.transform.rotation = new Quaternion(0, 0, 0, 0);
                                break;
                            }
                        }


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
                                //animator.Play("Idle");
                                animator.SetTrigger("idle2");
                                break;
                            case 1:
                                //animator.Play("RForward");
                                animator.SetTrigger("forward");
                                break;
                            case 2:
                                //animator.Play("RBack");
                                animator.SetTrigger("back");
                                break;
                            case 3:
                                //animator.Play("RLeft");
                                animator.SetTrigger("left");
                                break;
                            case 4:
                                //animator.Play("RRight");
                                animator.SetTrigger("right");
                                break;
                            case 5:
                                animator.SetTrigger("jump");
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
                    bullet.GetComponent<Bullet>().creatorId = objectId;
                    break;
                } 
            case WorldCommand.SMSG_PLAYER_TAKE_DAMAGE: 
                { 
                    Debug.Log($"CLIENT: received info about damage");
                    float health = InComePacket.ReadFloat();
                    GameObject.Find("Player(Clone)").GetComponent<HealthSystem>().SetHealth(health);
                    break;
                }
            case WorldCommand.SMSG_SET_PLAYER_HEALTH:
                {
                    byte type = InComePacket.ReadByte();
                    float health = InComePacket.ReadFloat();
                    GameObject.Find("Player(Clone)").GetComponent<HealthSystem>().SetHealth(health);
                    if(type == 0) //Если лечение с помощью аптечки
                    {
                        AudioClip sound = Resources.Load<AudioClip>("Sounds/InjectionSound");
                        GameObject.Find("Player(Clone)").transform.Find("Audio Source").transform.GetComponent<AudioSource>().PlayOneShot(sound);
                    }
                    break;
                }
            case WorldCommand.SMSG_PLAYER_DEATH:
                {
                    string objectId = InComePacket.ReadString();
                    foreach (GameObject obj in enemies.Values)
                    {
                        //Debug.Log(uniqueId);
                        if (obj.GetComponent<EnemyInfo>().playerId == objectId)
                        {
                            obj.GetComponent<EnemyInfo>().RaggDollOn();
                            break;
                        }
                    }
                    break;
                }
            case WorldCommand.SMSG_PLAYER_RESPAWN:
                {
                    string objectId = InComePacket.ReadString();
                    foreach (GameObject obj in enemies.Values)
                    {
                        //Debug.Log(uniqueId);
                        if (obj.GetComponent<EnemyInfo>().playerId == objectId)
                        {
                            obj.GetComponent<EnemyInfo>().RaggDollOff();
                            break;
                        }
                    }
                    break;
                }
            case WorldCommand.SMSG_REMOVE_PLAYER:
                {
                    string objectId = InComePacket.ReadString();
                    
                    foreach (GameObject obj in enemies.Values)
                    {
                        if (obj.GetComponent<EnemyInfo>().playerId == objectId)
                        {
                            Destroy(obj);
                            break;
                        }
                    }
                    enemies.TryRemove(objectId, out _);
                    break;
                }
            case WorldCommand.SMSG_PLAYER_WEAPON_INFO: 
                { 
                    string objectId = InComePacket.ReadString();

                    foreach (GameObject obj in enemies.Values)
                    {
                        if (obj.GetComponent<EnemyInfo>().playerId == objectId)
                        {
                            //Меняем оружие в руках данному игроку
                            EnemyInfo ei = obj.GetComponent<EnemyInfo>();

                            if(ei.WeaponObject != null){
                                Destroy(ei.WeaponObject);
                                ei.WeaponObject = null;
                            }

                            switch (InComePacket.ReadInt()) { 
                                case ((int) WeaponId.AK): { 
                                    ei.WeaponObject = Instantiate(AK, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0));
                                    ei.WeaponObject.transform.parent = ei.WeaponParentBone.transform;
                                    ei.WeaponObject.transform.localPosition = new Vector3(0, 0, 0);
                                    ei.WeaponObject.transform.rotation = new Quaternion(0, 0, 0, 0);
                                    //ei.WeaponObject.transform.
                                    break;
                                }
                                case ((int) WeaponId.PISTOL): { 
                                    ei.WeaponObject = Instantiate(pistol, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0));
                                    ei.WeaponObject.transform.parent = ei.WeaponParentBone.transform;
                                    ei.WeaponObject.transform.localPosition = new Vector3(0, 0, 0);
                                    ei.WeaponObject.transform.rotation = new Quaternion(0, 0, 0, 0);
                                    break;
                                }
                                case ((int) WeaponId.SAWNED_OFF): { 
                                    ei.WeaponObject = Instantiate(SO, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0));
                                    ei.WeaponObject.transform.parent = ei.WeaponParentBone.transform;
                                    ei.WeaponObject.transform.localPosition = new Vector3(0, 0, 0);
                                    ei.WeaponObject.transform.rotation = new Quaternion(0, 0, 0, 0);
                                    break;
                                }
                            }
                            break;
                        }
                    }
                    break;    
                }
            case WorldCommand.SMSG_CREATE_PICKUP_COMPRESS:
                {
                    LevelLogic LG = GameObject.Find("LevelLogic").GetComponent<LevelLogic>();
                    int PickupCount = InComePacket.ReadInt(); //Получаем количество пикапов из пакета
                    for(int i = 0; i < PickupCount; i++)
                    {
                        int id = InComePacket.ReadInt();
                        byte type = InComePacket.ReadByte();
                        string modelName = InComePacket.ReadString();
                        Vector3 pickupPos = new Vector3(InComePacket.ReadFloat(), InComePacket.ReadFloat(), InComePacket.ReadFloat());
                        if (type == 0)
                        {
                            Vector3 pickupRote = new Vector3(InComePacket.ReadFloat(), InComePacket.ReadFloat(), InComePacket.ReadFloat());
                            LG.CreatePicup(id, type, modelName, pickupPos, pickupRote);
                        }
                    }
                    break;
                }            
            case WorldCommand.SMSG_CREATE_PICKUP:
                {
                    LevelLogic LG = GameObject.Find("LevelLogic").GetComponent<LevelLogic>();
                    int id = InComePacket.ReadInt();
                    byte type = InComePacket.ReadByte();
                    string modelName = InComePacket.ReadString();
                    Vector3 pickupPos = new Vector3(InComePacket.ReadFloat(), InComePacket.ReadFloat(), InComePacket.ReadFloat());
                    if (type == 0)
                    {
                        Vector3 pickupRote = new Vector3(InComePacket.ReadFloat(), InComePacket.ReadFloat(), InComePacket.ReadFloat());
                        LG.CreatePicup(id, type, modelName, pickupPos, pickupRote);
                    }
                    break;
                }
            case WorldCommand.SMSG_DESTROY_PICKUP:
                {
                    int pickupid = InComePacket.ReadInt(); //Получаем id пикапа для удаления
                    LevelLogic LG = GameObject.Find("LevelLogic").GetComponent<LevelLogic>();
                    LG.DestroyPickup(pickupid);
                    break;
                }
            case WorldCommand.SMSG_CREATE_BULLET_EFFECT: 
                { 
                    Quaternion angle = new Quaternion(InComePacket.ReadFloat(), InComePacket.ReadFloat(), InComePacket.ReadFloat(), InComePacket.ReadFloat());
                    Vector3 impulse = new Vector3(InComePacket.ReadFloat(), InComePacket.ReadFloat(), InComePacket.ReadFloat());
                    string pid = InComePacket.ReadString();

                    foreach (GameObject obj in enemies.Values)
                    {
                        if (obj.GetComponent<EnemyInfo>().playerId == pid) 
                        { 
                            GameObject effect = Instantiate(TrailEffectBullet, new Vector3(0, 0, 0), angle);
                            effect.transform.localPosition = obj.GetComponent<EnemyInfo>().WeaponObject.transform.Find("model").Find("flashPlace").transform.position;
                            effect.transform.GetComponent<Bullet>().creatorId = pid;
                            effect.transform.GetComponent<ConstantForce>().force = impulse * 5000;

                            obj.transform.Find("Audio Source").gameObject.GetComponent<AudioSource>().PlayOneShot(ShotClip);
                            obj.GetComponent<Animator>().SetTrigger("shot");
                            break;
                        }
                    }
                    break;
                }
            case WorldCommand.SMSG_SET_PLAYER_IMPYLSE:
                {
                    GameObject.Find("Player(Clone)").GetComponent<Movement>().SetImpulse(new Vector3(InComePacket.ReadFloat(),InComePacket.ReadFloat(),InComePacket.ReadFloat()), InComePacket.ReadFloat());
                    break;
                }
            case WorldCommand.SMSG_SEND_MESSAGE:
                {
                    Transform chat = GameObject.Find("Canvas").transform.Find("Chat");
                    if(chat != null)
                    {
                        string message = InComePacket.ReadString();
                        chat.GetComponent<ChatUI>().AddChatMessage(message);
                    }
                    break;
                }
            case WorldCommand.SMSG_CLEAR_PLAYER_CHAT:
                {
                    Transform chat = GameObject.Find("Canvas").transform.Find("Chat");
                    if(chat != null)
                    {
                        chat.GetComponent<ChatUI>().ClearChat();
                    }
                    break;
                }
            case WorldCommand.SMSG_ADD_PLAYER_AMMO:
                {
                    GameObject.Find("Player(Clone)").GetComponent<WeaponSystem>().AddPlayerAmmo();
                    break;
                }
            case WorldCommand.SMSG_SEND_KILL_MESSAGE:
                {
                    Transform KillList = GameObject.Find("Canvas").transform.Find("KillList");
                    if (KillList != null)
                    {
                        KillList.GetComponent<KillList>().AddKillMessage(InComePacket.ReadString(), InComePacket.ReadString(), InComePacket.ReadByte());
                    }
                    break;
                }
            case WorldCommand.SMSG_STEP: 
                {
                    string objectId = InComePacket.ReadString();
                    
                    foreach (GameObject obj in enemies.Values)
                    {
                        if (obj.GetComponent<EnemyInfo>().playerId == objectId)
                        {
                            obj.transform.Find("Audio Source2").gameObject.GetComponent<AudioSource>().PlayOneShot(steps[UnityEngine.Random.Range(0,3)]);
                            break;
                        }
                    }
                    break;
                }
            case WorldCommand.SMSG_CREATE_OBJECT:
                {
                    Vector3 objpos = new Vector3(InComePacket.ReadFloat(), InComePacket.ReadFloat(), InComePacket.ReadFloat());
                    Instantiate(bulletPrefab, objpos, Quaternion.identity);
                    break;
                }
            case WorldCommand.SMSG_CREATE_NPC:
                {
                    int count = InComePacket.ReadInt16();
                    for(int i = 0; i < count; i++)
                    {
                        int npcID = InComePacket.ReadInt16();
                        Vector3 npcPos = new Vector3(InComePacket.ReadFloat(), InComePacket.ReadFloat(), InComePacket.ReadFloat());
                        string npcName = InComePacket.ReadString();
                        Debug.Log($"NPC {npcName}({npcID}): {npcPos}");
                        GameObject npc = Instantiate(NPCprefab, npcPos, Quaternion.identity);
                        NPC NPCScript = npc.transform.GetComponent<NPC>();
                        NPCScript.npcID = npcID;
                        NPCScript.npcName = npcName;
                        NPCarray.Add((npc, NPCScript));
                    }
                    break;
                }
            case WorldCommand.SMSG_NPC_MOVE_PATH:
                {
                    Debug.Log("SMSG_NPC_MOVE_PATH");
                    int npcID = InComePacket.ReadInt16();
                    Vector3 npcPos = new Vector3(InComePacket.ReadFloat(), InComePacket.ReadFloat(), InComePacket.ReadFloat()); //Начальная точка движения
                    float moveSpeed = InComePacket.ReadFloat(); //Скорость движения по маршруту
                    Vector3 endPoint = new Vector3(InComePacket.ReadFloat(), InComePacket.ReadFloat(), InComePacket.ReadFloat()); //Конечная точка движения
                    for(int i = 0; i < NPCarray.Count; i++)
                    { 
                        Debug.Log($"item = {NPCarray[i].Item2.npcID}, npcid = {npcID}");
                        if(NPCarray[i].Item2.npcID == npcID)
                        {
                            NPCarray[i].Item2.StartMovePath(npcPos, endPoint, moveSpeed);
                            break;
                        }
                    }
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
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}

public class GameClient
{
    public string name;
    public bool isHost;
}