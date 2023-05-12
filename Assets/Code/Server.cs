using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;
using PacketHeaders;
using WeaponEnumIds;

enum PlayerStatus
{
    PLAYER_ON_FOOT = 0,
    PLAYER_IS_DEATH = 1
};
public class ServerClient
{
    public TcpClient tcp;
    public byte[] buffer;
    public int CountGetPacketData; //Количество уже полученных байт
    public int Remaining; //Количество запрошеных байт
    public NetworkStream stream;
    public ServerClient(TcpClient tcp)
    {
        this.tcp = tcp;
    }
    public string PlayerID;
    public string PlayerName;
    public float[] lastPos = new float[4];
    public bool authorized = false;

    public int status = (int)PlayerStatus.PLAYER_ON_FOOT;
    public float health = 100.0f;
    public long TimeOutTimer;

    public WeaponId weaponId = WeaponId.NONE;
}

public class ServerPickup
{
    private static int picupIdCounter = 0;
    public int id;
    public Vector3 PickupPos;
    public Vector3 PickupRot;
    public byte type;
    public string ModelName;
    public ServerPickup(Vector3 pos, byte type, string ModelName)
    {
        this.id = picupIdCounter;
        picupIdCounter++;
        PickupPos = pos;
        this.type = type;
        this.ModelName = ModelName;
    }
    public ServerPickup(Vector3 pos, Vector3 rot, byte type, string ModelName)
    {
        this.id = picupIdCounter;
        picupIdCounter++;
        PickupPos = pos;
        PickupRot = rot;
        this.type = type;
        this.ModelName = ModelName;
    }
}
public class Server : MonoBehaviour
{

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

    public int port = 6321;
    private static int HeaderSize = 6; //(uint16(2) - PacketID, Uint32(4) - PacketSize)
    private static int MaxDataSize = 256; //Максимальный размер пакета, который может обработать сервер
    private TcpListener server;
    private bool serverStarted;
    private Thread serverThread;
    static ConcurrentDictionary<ServerClient, object> clients = new ConcurrentDictionary<ServerClient, object>();
    private Queue<Tuple<ServerClient, PacketDecryptor>> messageQueue = new Queue<Tuple<ServerClient, PacketDecryptor>>(); //Packet queue
    public StreamWriter sw = null;
    Dictionary<int, ServerCommands> teams = new Dictionary<int, ServerCommands>();
    public List<ServerPickup> pickups = new List<ServerPickup>();
    int HealthPickUP = -1;
    long HealthRespawnTime = long.MaxValue;

    int[] TramplinePicUP = { -1, -1 };

    int AmmoPickUP = -1;
    long AmmoRespawnTime = long.MaxValue;

    long Gettime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    /*public static void Main()
    {
        Program init = new Program();
        init.Init();
    }*/

    public void printf(string str)
    {
        if (sw != null)
        {
            sw.WriteLine(str);
            sw.Flush();
        }
    }
    // Start is called before the first frame update
    public async Task Init()
    {
        DontDestroyOnLoad(gameObject);
        // Create a file to write to.
        sw = new StreamWriter(Path.Combine(System.IO.Directory.GetCurrentDirectory(), "ServerLogs.txt"));
        InvokeRepeating("SecondTimer", 1, 1);
        printf("SERVER начал работу.");
        try
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();
            serverStarted = true;

            Debug.Log("SERVER начал работу.");
            //Создаём команды
            teams = new Dictionary<int, ServerCommands> {
              { 0, new ServerCommands(new Vector3(0f, 2f, 0f))},
              { 1, new ServerCommands(new Vector3(130f, 6.3f, 60f))}
            };
            HealthPickUP = CreatePickup(new ServerPickup(new Vector3(82f, -6.3f, 85.6f), 0, "0"));
            AmmoPickUP = CreatePickup(new ServerPickup(new Vector3(10.5f, -13f, 75f), 0, "2"));
            TramplinePicUP[0] = CreatePickup(new ServerPickup(new Vector3(15f, 8.505f, 18.5f), new Vector3(-90, 0, 45), 0, "1"));
            TramplinePicUP[1] = CreatePickup(new ServerPickup(new Vector3(-12.89f, -31.25f, 81.12f), new Vector3(-121.54f, -75, 180), 0, "1"));
            
        }
        catch (Exception e)
        {
            Debug.Log("SERVER Socket error: " + e.Message);
        }
        try
        {
            serverThread = new Thread(new ThreadStart(QueueUpdate));
            serverThread.Start();
            await StartListening();
        }
        catch(Exception e)
        {
            Debug.Log($"Server listener error: {e.Message}");
        }
    }
    public async Task StartListening()
    {
        while (true)
        {
            ServerClient client = new ServerClient(await server.AcceptTcpClientAsync());
            client.health = 100.0f;
            clients.TryAdd(client, null);
            client.TimeOutTimer = Gettime;
            Debug.Log($"Client connected: {client.tcp.Client.RemoteEndPoint}");
            client.stream = client.tcp.GetStream();
            Packet packet = new Packet((int)WorldCommand.SMSG_OFFER_ENTER);
            packet.Write((int)0);
            await client.stream.WriteAsync(packet.GetBytes());

            client.Remaining = HeaderSize;
            client.CountGetPacketData = 0;
            client.buffer = new byte[client.Remaining];
            //client.stream.BeginRead(client.buffer, 0, client.Remaining, ReadHeaderCallback, new Tuple<ServerClient>(client));
            int bytesRead = await client.stream.ReadAsync(client.buffer, 0, client.Remaining);
            ReadHeaderCallback(client.buffer, bytesRead, client);
        }
    }
    async void ReadHeaderCallback(byte[] buffer, int bytesRead, ServerClient client)
    {
        try
        {
            if (bytesRead == 0)
            {
                // Соединение было закрыто сервером
                DisconnectPlayer(client);
                return;
            }
            printf($"{PrintByteArray(client.buffer)}");
            if (bytesRead != client.Remaining) //Если количество байт которое мы получили не соответствует тому, которое мы запросили
            {
                printf($"Ошибка сети, количество байт не соответствует запрошенному значению. Запрошено байт: {client.Remaining}, получено: {bytesRead}");
                Debug.Log($"Ошибка сети, количество байт не соответствует запрошенному значению. Запрошено байт: {client.Remaining}, получено: {bytesRead}");
                //Пытаемся запросить байты по новой
                client.buffer = new byte[client.Remaining];
                int byteRead = await client.stream.ReadAsync(client.buffer, 0, client.Remaining);
                ReadHeaderCallback(client.buffer, byteRead, client);
                return;
            }

            int headerSize = (int)BitConverter.ToUInt32(client.buffer, 2); //Ищем длинну пакета

            if (headerSize > MaxDataSize || headerSize < 1) //Если количество байтов пакета больше или меньше разрешенного 
            {
                printf($"Ошибка сети, получен пакет некорректной длинны. Длинна {headerSize}, байт код: {PrintByteArray(client.buffer)}");
                Debug.Log($"Ошибка сети, получен пакет некорректной длинны. Длинна {headerSize}, байт код: {PrintByteArray(client.buffer)}");
                client.buffer = new byte[client.Remaining];
                int byteRead = await client.stream.ReadAsync(client.buffer, 0, client.Remaining);
                ReadHeaderCallback(client.buffer, byteRead, client);
            }
            client.TimeOutTimer = Gettime;
            client.CountGetPacketData += bytesRead;
            try
            {
                client.Remaining = headerSize;
                Array.Resize(ref client.buffer, client.CountGetPacketData + client.Remaining);
                //client.stream.BeginRead(client.buffer, client.CountGetPacketData, client.Remaining, ReadDataCallback, new Tuple<ServerClient>(client));
                int byteRead = await client.stream.ReadAsync(client.buffer, client.CountGetPacketData, client.Remaining);
                ReadDataCallback(client.buffer, byteRead, client);
            }
            catch (Exception ex)
            {
                printf($"Fail to read header. Packet size: {headerSize}\nError: {ex}");

                Debug.Log($"Fail to read header. Packet size: {headerSize}\nError: {ex}");
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"Error: {ex.Message}");
            DisconnectPlayer(client);
        }
    }
    async void ReadDataCallback(byte[] buffer, int bytesRead, ServerClient client)
    {
        try
        {
            if (bytesRead == 0)
            {
                DisconnectPlayer(client);
                return;
            }
            printf($"{PrintByteArray(client.buffer)}");

            PacketDecryptor packet = new PacketDecryptor(client.buffer);
            client.CountGetPacketData = 0;

            messageQueue.Enqueue(Tuple.Create(client, packet));
            client.TimeOutTimer = Gettime;

            client.Remaining = HeaderSize;
            client.buffer = new byte[client.Remaining];
            int byteRead = await client.stream.ReadAsync(client.buffer, 0, client.Remaining);
            ReadHeaderCallback(client.buffer, byteRead, client);
            //Debug.Log("Packet get, find new");
            //
        }
        catch (Exception ex)
        {
            Debug.Log($"Error: {ex.Message}");
            DisconnectPlayer(client);
        }
    }
    private void DisconnectPlayer(ServerClient c)
    {
        // Соединение было закрыто сервером
        Debug.Log($"DATA: Сервер разорвал соединение с {c.tcp.Client.RemoteEndPoint}");
        Packet packet = new Packet((int)WorldCommand.SMSG_REMOVE_PLAYER);
        packet.Write(c.tcp.Client.RemoteEndPoint.ToString());
        c.stream.Close();
        foreach (ServerClient client in clients.Keys)//Отправляем всем игрокам позицию нового игрока
        {
            client.stream.WriteAsync(packet.GetBytes());
            SendPlayerMessage(client, $"Игрок {c.PlayerName} покинул игру!");
            break;
        }
        c.stream.Close();
        clients.TryRemove(c, out _);
        return;
    }
    private void QueueUpdate()
    {
        while (true) //Да, я могу позволить себе бесконечный цикл, а ты?
        {
            ServerClient client = null;
            PacketDecryptor packet = null;
            lock (messageQueue)
            {
                if (messageQueue.Count > 0)
                {
                    Tuple<ServerClient, PacketDecryptor> packetTuple = messageQueue.Dequeue();
                    client = packetTuple.Item1;
                    packet = packetTuple.Item2;

                }
            }

            if (packet != null) //Если нашли какой-то пакетик
            {
                OnIncomingData(client, packet); //Отправляем его на обработку
            }
            else //Если нет
            {
                Thread.Sleep(10); //Спим 10 мс
            }
        }
    }
    private void OnIncomingData(ServerClient c, PacketDecryptor packet)
    {
        //Логика обработчика данных
        int packetid = packet.GetPacketId();
        switch ((WorldCommand)packetid)
        {
            case (WorldCommand.CMSG_OFFER_ENTER_ANSWER): //Запрос на авторизацию клиента
                {
                    int playerid = packet.ReadInt();
                    c.PlayerName = packet.ReadString();
                    c.PlayerID = c.tcp.Client.RemoteEndPoint.ToString();
                    Debug.Log($"SERVER: Игрок {c.PlayerName} подключился к серверу");
                    Debug.Log("SERVER: Начинаем игру!");

                    int minCount = int.MaxValue;
                    ServerCommands minTeam = null;
                    foreach (var team in teams.Values)
                    {
                        if (team.CommandPlayers.Count < minCount)
                        {
                            minCount = team.CommandPlayers.Count;
                            minTeam = team;
                        }
                    }
                    minTeam.CommandPlayers.Add(c);

                    Packet apacket = new Packet((int)WorldCommand.SMSG_START_GAME);

                    apacket.Write(c.tcp.Client.RemoteEndPoint.ToString());
                    foreach (ServerClient client in clients.Keys)//Отправляем всем игрокам позицию нового игрока
                    {
                        if(c.tcp == client.tcp) apacket.Write(1);
                        else apacket.Write(0);
                        apacket.Write((float)minTeam.spawnPoint.x);
                        apacket.Write((float)minTeam.spawnPoint.y);
                        apacket.Write((float)minTeam.spawnPoint.z);
                        break;
                    }
                    c.stream.WriteAsync(apacket.GetBytes());
                    break;
                }
            case (WorldCommand.CMSG_PLAYER_LOGIN): //Запрос на вход в игровой мир клиента
                {

                    c.authorized = true;
                    //int objModelId = packet.ReadInt();


                    c.lastPos[0] = packet.ReadFloat();
                    c.lastPos[1] = packet.ReadFloat();
                    c.lastPos[2] = packet.ReadFloat();

                    c.lastPos[3] = packet.ReadFloat();

                    Packet responcePacket = new Packet((int)WorldCommand.SMSG_PLAYER_LOGIN);
                    responcePacket.Write((string)c.tcp.Client.RemoteEndPoint.ToString());
                    responcePacket.Write((string)c.PlayerName);

                    responcePacket.Write((float)c.lastPos[0]);
                    responcePacket.Write((float)c.lastPos[1]);
                    responcePacket.Write((float)c.lastPos[2]);
                    responcePacket.Write((float)c.lastPos[3]);

                    foreach (ServerClient client in clients.Keys)//Отправляем всем игрокам позицию нового игрока
                    {
                        if (c.tcp == client.tcp) continue;
                        if (client.authorized == false) continue;
                        client.stream.WriteAsync(responcePacket.GetBytes());
                        SendPlayerMessage(client, $"Игрок {c.PlayerName} присоединился к игре!");
                    }
                    //
                    Packet playersPacket = new Packet((int)WorldCommand.SMSG_CREATE_PLAYERS);

                    int counter = 0;
                    foreach (ServerClient client in clients.Keys)
                    {
                        if (c.tcp == client.tcp) continue;
                        if (client.authorized == false) continue;
                        counter++;
                    }

                    playersPacket.Write((int)counter);

                    foreach (ServerClient client in clients.Keys)//Отправляем подключившемуся игроку позицию всех остальных игроков
                    {
                        if (c.tcp == client.tcp) continue;
                        if (client.authorized == false) continue;

                        playersPacket.Write((string)client.tcp.Client.RemoteEndPoint.ToString());
                        playersPacket.Write((string)client.PlayerName);
                        playersPacket.Write((float)client.lastPos[0]);
                        playersPacket.Write((float)client.lastPos[1]);
                        playersPacket.Write((float)client.lastPos[2]);
                        playersPacket.Write((float)client.lastPos[3]);
                        playersPacket.Write((int)client.weaponId);
                        Debug.Log($"{(int)client.weaponId}");
                    }
                    c.stream.WriteAsync(playersPacket.GetBytes());

                    Packet picupPacket = new Packet((int) WorldCommand.SMSG_CREATE_PICKUP_COMPRESS);
                    picupPacket.Write(pickups.Count);
                    foreach(ServerPickup pic in pickups)
                    {
                        picupPacket.Write(pic.id);
                        picupPacket.Write(pic.type);
                        picupPacket.Write(pic.ModelName);
                        picupPacket.Write(pic.PickupPos.x);
                        picupPacket.Write(pic.PickupPos.y);
                        picupPacket.Write(pic.PickupPos.z);
                        if(pic.type == 0)
                        {
                            picupPacket.Write(pic.PickupRot.x);
                            picupPacket.Write(pic.PickupRot.y);
                            picupPacket.Write(pic.PickupRot.z);
                        }
                    }
                    c.stream.WriteAsync(picupPacket.GetBytes());
                    break;
                }
            case (WorldCommand.CMSG_OBJ_INFO): //Синхронизация объектов и игроков
                {
                    Packet responcePacket = new Packet((int)WorldCommand.SMSG_OBJ_INFO);

                    responcePacket.Write((string)c.tcp.Client.RemoteEndPoint.ToString()); //Object ID
                    responcePacket.Write((int)packet.ReadInt()); //animid
                    byte before = packet.ReadByte();
                    responcePacket.Write((byte)before); //before

                    bool position = (before & 0b100) != 0;
                    bool rotation = (before & 0b010) != 0;
                    bool speed = (before & 0b001) != 0;

                    if (position)
                    {
                        //Debug.Log($"POS: {packet.ReadFloat()}; {packet.ReadFloat()}; {packet.ReadFloat()}");
                        responcePacket.Write((float)packet.ReadFloat());
                        responcePacket.Write((float)packet.ReadFloat());
                        responcePacket.Write((float)packet.ReadFloat());
                    }

                    if (rotation)
                    {
                        //Debug.Log($"ROT: {packet.ReadFloat()}; {packet.ReadFloat()}; {packet.ReadFloat()}");
                        responcePacket.Write((float)packet.ReadFloat());
                    }

                    if (speed)
                    {
                        //Debug.Log($"SPEED: {packet.ReadFloat()}; {packet.ReadFloat()}; {packet.ReadFloat()}");
                        responcePacket.Write((float)packet.ReadFloat());
                        responcePacket.Write((float)packet.ReadFloat());
                        responcePacket.Write((float)packet.ReadFloat());
                    }




                    foreach (ServerClient client in clients.Keys)
                    {
                        if (c == client) continue;
                        if (client.authorized == false) continue;

                        client.stream.WriteAsync(responcePacket.GetBytes());
                    }
                    //Debug.Log("POS: " +packet.ReadFloat() + "; " + packet.ReadFloat() + "; " + packet.ReadFloat() + "; ");
                    break;
                }
            case (WorldCommand.CMSG_CREATE_BULLET): //Создание пули от клиента
                {
                    Packet responcePacket = new Packet((int)WorldCommand.SMSG_CREATE_BULLET);

                    responcePacket.Write((string)c.tcp.Client.RemoteEndPoint.ToString());

                    //Позиция
                    responcePacket.Write((float)packet.ReadFloat());
                    responcePacket.Write((float)packet.ReadFloat());
                    responcePacket.Write((float)packet.ReadFloat());

                    //ротация
                    responcePacket.Write((float)packet.ReadFloat());
                    responcePacket.Write((float)packet.ReadFloat());
                    responcePacket.Write((float)packet.ReadFloat());

                    //Скорость
                    responcePacket.Write((float)packet.ReadFloat());
                    responcePacket.Write((float)packet.ReadFloat());
                    responcePacket.Write((float)packet.ReadFloat());


                    foreach (ServerClient client in clients.Keys)
                    {
                        if (c == client) continue;
                        if (client.authorized == false) continue;

                        client.stream.WriteAsync(responcePacket.GetBytes());
                    }

                    break;
                }
            case (WorldCommand.CMSG_PLAYER_TAKE_DAMAGE): //Информация о дамаге
                {
                    float damage = packet.ReadFloat();

                    break;
                }
            case (WorldCommand.CMSG_PLAYER_GIVE_DAMAGE): //Информация о дамаге
                {
                    string VictimID = packet.ReadString();
                    float damage = packet.ReadFloat();
                    byte weaponid = packet.ReadByte();

                    //Debug.Log($"SERVER: received info about damage: {}");
                    foreach (ServerClient client in clients.Keys) //Проходимся по всем клиентам
                    {
                        if (client.tcp.Client.RemoteEndPoint.ToString() == VictimID) //Если клиент является тем, кому нанесли урон
                        {
                            if (client.status != (int)PlayerStatus.PLAYER_IS_DEATH) //Если этот клиент жив
                            {
                                client.health -= damage; //снимаем хп
                                Packet apacket = new Packet((int)WorldCommand.SMSG_PLAYER_TAKE_DAMAGE); //Формируем пакет с текущим количеством хп
                                apacket.Write((float)client.health);
                                client.stream.WriteAsync(apacket.GetBytes());

                                if (client.health <= 0) //Если ХП у игрока не осталось
                                {
                                    Packet deathPacket = new Packet((int)WorldCommand.SMSG_PLAYER_DEATH); //формируем пакет смерти и отправляем всем игрокам кроме того, кто помер
                                    deathPacket.Write(VictimID);
                                    client.status = (int)PlayerStatus.PLAYER_IS_DEATH;
                                    foreach (ServerClient cli in clients.Keys)
                                    {
                                        if (cli.tcp.Client.RemoteEndPoint.ToString() != VictimID)
                                            cli.stream.WriteAsync(deathPacket.GetBytes());
                                        SendKillMessage(cli, c.PlayerName, weaponid, client.PlayerName);
                                    }
                                }
                            }
                            break;
                        }
                    }

                    break;
                }
            case (WorldCommand.CMSG_PLAYER_RESTORE_HEALTH): //восстановление здоровья
                {
                    string playerId = c.tcp.Client.RemoteEndPoint.ToString();
                    float health = packet.ReadFloat();
                    c.health = health;
                    c.status = (int)PlayerStatus.PLAYER_ON_FOOT;
                    Packet apacket = new Packet((int)WorldCommand.SMSG_PLAYER_RESPAWN);
                    apacket.Write(playerId);
                    foreach (ServerClient client in clients.Keys)
                    {
                        if(client.tcp.Client.RemoteEndPoint.ToString() != playerId){
                                client.stream.WriteAsync(apacket.GetBytes());
                        }
                    }
                    break;
                }
            case (WorldCommand.CMSG_PLAYER_WEAPON_INFO): 
                { 
                    c.weaponId = (WeaponId) packet.ReadInt(); 

                    Packet apacket = new Packet((int)WorldCommand.SMSG_PLAYER_WEAPON_INFO);
                    apacket.Write(c.tcp.Client.RemoteEndPoint.ToString());
                    apacket.Write((int) c.weaponId);
                    foreach (ServerClient client in clients.Keys)
                    {
                        if(client.tcp.Client.RemoteEndPoint.ToString() != c.tcp.Client.RemoteEndPoint.ToString()){
                            client.stream.WriteAsync(apacket.GetBytes());
                        }
                    }
                    break;
                }
            case (WorldCommand.CMSG_CREATE_BULLET_EFFECT): 
                {  
                    Packet apacket = new Packet((int)WorldCommand.SMSG_CREATE_BULLET_EFFECT);

                    apacket.Write(packet.ReadFloat());
                    apacket.Write(packet.ReadFloat());
                    apacket.Write(packet.ReadFloat());
                    apacket.Write(packet.ReadFloat());

                    apacket.Write(packet.ReadFloat());
                    apacket.Write(packet.ReadFloat());
                    apacket.Write(packet.ReadFloat());

                    apacket.Write(c.tcp.Client.RemoteEndPoint.ToString());

                    foreach (ServerClient client in clients.Keys)
                    {
                        if(client.tcp.Client.RemoteEndPoint.ToString() != c.tcp.Client.RemoteEndPoint.ToString()){
                            client.stream.WriteAsync(apacket.GetBytes());
                        }
                    }
                    break;
                }
            case (WorldCommand.CMSG_PLAYER_PICKUP_PICKUP):
                {
                    int pickupid = packet.ReadInt();
                    OnPlayerPickupPickup(c, pickupid);
                    break;
                }
            case (WorldCommand.CMSG_SEND_MESSAGE):
                {
                    string message = packet.ReadString();
                    OnPlayerSendMessage(c, message);
                    break;
                }
            case (WorldCommand.CMSG_STEP):
                { 
                    Packet p = new Packet((int)WorldCommand.SMSG_STEP);
                    p.Write(c.tcp.Client.RemoteEndPoint.ToString());
                    foreach (ServerClient client in clients.Keys)
                    {
                        if(client.tcp.Client.RemoteEndPoint.ToString() != c.tcp.Client.RemoteEndPoint.ToString()){
                            client.stream.WriteAsync(p.GetBytes());
                        }
                    }
                    break;
                }
        }
    }
    public void OnPlayerSendMessage(ServerClient c, string message)
    {
        if (message[0] == '/')
        {
            int index = message.IndexOf(' ');
            if(index == -1)
            {
                index = message.Length - 1;
            }
            else
            {
                index--;
            }
            string cmd = message.Substring(1, index);
            if(cmd == "cc")
            {
                foreach (ServerClient client in clients.Keys)
                {
                    ClearPlayerChat(client);
                }
            }
        }
        else
        {
            string msg = $"{c.PlayerName}: {message}";
            foreach (ServerClient client in clients.Keys)
            {
                SendPlayerMessage(client, msg);
            }
        }
        return;
    }
    public void SendPlayerMessage(ServerClient c, string messsage)
    {
        Packet packet = new Packet((int)WorldCommand.SMSG_SEND_MESSAGE);
        packet.Write(messsage);
        c.stream.WriteAsync(packet.GetBytes());
    }
    public void ClearPlayerChat(ServerClient c)
    {
        Packet packet = new Packet((int)WorldCommand.SMSG_CLEAR_PLAYER_CHAT);
        packet.Write(1);
        c.stream.WriteAsync(packet.GetBytes());
    }
    public void OnPlayerPickupPickup(ServerClient c, int pickupid)
    {
        if(pickupid == HealthPickUP)
        {   
            if (c.health >= 100)
                return;
            
            if (c.health + 50 > 100)
                c.health = 100;
            else
                c.health += 50;

            HealthPickUP = -1;
            DestroyPickup(pickupid);
            HealthRespawnTime = Gettime + 30;
            Packet packet = new Packet((int)WorldCommand.SMSG_SET_PLAYER_HEALTH);
            packet.Write((byte)0);
            packet.Write(c.health);

            c.stream.WriteAsync(packet.GetBytes());   
        }
        if(pickupid == AmmoPickUP)
        {   
            AmmoPickUP = -1;
            DestroyPickup(pickupid);
            AmmoRespawnTime = Gettime + 30;
            Packet packet = new Packet((int)WorldCommand.SMSG_ADD_PLAYER_AMMO);
            packet.Write((byte)0);
            c.stream.WriteAsync(packet.GetBytes());   
        }
        else if(pickupid == TramplinePicUP[0])
        {
            SetPlayerVelocity(c, new Vector3(18f, 30f, 18f), 1f);
        }
        else if(pickupid == TramplinePicUP[1])
        {
            SetPlayerVelocity(c, new Vector3(2f, 30f, 10f), 1f);
        }
        return;
    }
    public int CreatePickup(ServerPickup pickup)
    {
        pickups.Add(pickup);
        Packet packet = new Packet((int)WorldCommand.SMSG_CREATE_PICKUP);
        packet.Write(pickup.id);
        packet.Write(pickup.type);
        packet.Write(pickup.ModelName);
        packet.Write(pickup.PickupPos.x);
        packet.Write(pickup.PickupPos.y);
        packet.Write(pickup.PickupPos.z);
        if (pickup.type == 0)
        {
            packet.Write(pickup.PickupRot.x);
            packet.Write(pickup.PickupRot.y);
            packet.Write(pickup.PickupRot.z);
        }
        foreach (ServerClient client in clients.Keys)
        {
            client.stream.WriteAsync(packet.GetBytes());
        }
        return pickup.id;
    }
    public bool DestroyPickup(int pickupid)
    {
        foreach(ServerPickup pic in pickups)
        {
            if(pic.id == pickupid)
            {
                Packet packet = new Packet((int)WorldCommand.SMSG_DESTROY_PICKUP);
                packet.Write(pickupid);
                foreach (ServerClient client in clients.Keys)
                {
                    client.stream.WriteAsync(packet.GetBytes());
                }
                pickups.Remove(pic);
                return true;
            }
        }
        return false;
    }
    public void SetPlayerVelocity(ServerClient c, Vector3 direction, float disableMoveTime)
    {
        Packet packet = new Packet((int)WorldCommand.SMSG_SET_PLAYER_IMPYLSE);
        packet.Write((float)direction.x);
        packet.Write((float)direction.y);
        packet.Write((float)direction.z);
        packet.Write((float)disableMoveTime);
        c.stream.WriteAsync(packet.GetBytes());
    }
    public void SendKillMessage(ServerClient c, string KillerName, byte IconID, string DeathName)
    {
        Packet packet = new Packet((int)WorldCommand.SMSG_SEND_KILL_MESSAGE);
        packet.Write(KillerName);
        packet.Write(DeathName);
        packet.Write(IconID);
        c.stream.WriteAsync(packet.GetBytes());
    }
    private void OnApplicationQuit()
    {
        if (serverStarted) server.Stop();
        if (serverThread != null) serverThread.Abort();
        Destroy(this);
    }
    void OnDestroy()
    {
        if(serverStarted)server.Stop();
        if(serverThread != null) serverThread.Abort();
        Destroy(this);
    }
    public void SecondTimer()
    {
        Gettime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        //Debug.Log($"Now gettime {Gettime}");
        if (Gettime >= HealthRespawnTime)
        {
            HealthRespawnTime = long.MaxValue;
            HealthPickUP = CreatePickup(new ServerPickup(new Vector3(82f, -6.3f, 85.6f), 0, "0"));
        }
        if (Gettime >= AmmoRespawnTime)
        {
            AmmoRespawnTime = long.MaxValue;
            AmmoPickUP = CreatePickup(new ServerPickup(new Vector3(10.5f, -13f, 75f), 0, "2"));
        }
        foreach(ServerClient c in clients.Keys)
        {
            if (Gettime - c.TimeOutTimer > 15)
            {
                DisconnectPlayer(c);
            }
        }
        return;
    }    
}