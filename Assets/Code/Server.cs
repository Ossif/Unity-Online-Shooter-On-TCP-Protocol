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
    public class ServerClient
    {
        public TcpClient tcp;
        public byte[] buffer;
        public int CountGetPacketData;
        public NetworkStream stream;
        public ServerClient(TcpClient tcp)
        {
            this.tcp = tcp;
        }

        public float [] lastPos = new float[4];
        public bool authorized = false;
    }

    public int port = 6321;
    private static int HeaderSize = 6; //(uint16(2) - PacketID, Uint32(4) - PacketSize)
    private TcpListener server;
    private bool serverStarted;
    private Thread serverThread;
    static ConcurrentDictionary<ServerClient, object> clients = new ConcurrentDictionary<ServerClient, object>();
    private Queue<Tuple<ServerClient, PacketDecryptor>> messageQueue = new Queue<Tuple<ServerClient, PacketDecryptor>>(); //Packet queue

    // Start is called before the first frame update
    public void Init()
    {
        DontDestroyOnLoad(gameObject);
        try
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();
            StartListening();

            serverStarted = true;
            Debug.Log("SERVER начал работу.");
            serverThread = new Thread(new ThreadStart(QueueUpdate));
            serverThread.Start();
        }
        catch (Exception e)
        {
            Debug.Log("SERVER Socket error: " + e.Message);
        }
    }

    public async Task StartListening()
    {
        ServerClient client = new ServerClient(await server.AcceptTcpClientAsync());
        clients.TryAdd(client, null);
        Debug.Log($"Client connected: {client.tcp.Client.RemoteEndPoint}");
        client.stream = client.tcp.GetStream();
        Packet packet = new Packet((int) PacketHeaders.WorldCommand.MSG_NULL_ACTION);
        packet.Write((int)0);
        client.stream.WriteAsync(packet.GetBytes());
        client.buffer = new byte[HeaderSize];
        client.CountGetPacketData = 0;
        client.stream.BeginRead(client.buffer, 0, HeaderSize, ReadHeaderCallback, new Tuple<ServerClient>(client));
        StartListening();
    }

    void ReadHeaderCallback(IAsyncResult ar)
    {
        var state = (Tuple<ServerClient>)ar.AsyncState;
        var client = state.Item1;

        try
        {
            int bytesRead = client.stream.EndRead(ar);

            if (bytesRead == 0)
            {
                // Соединение было закрыто сервером
                Debug.Log("Сервер разорвал соединение(1)");
                client.stream.Close();
                clients.TryRemove(client, out _);
                return;
            }
            client.CountGetPacketData += bytesRead;

            int headerSize = (int)BitConverter.ToUInt32(client.buffer, 2);
            //Debug.Log($"HeaderSize: {headerSize}, {PrintByteArray(client.buffer)}");
            Array.Resize(ref client.buffer, client.CountGetPacketData + headerSize);
            // Обработка принятых данных
            client.stream.BeginRead(client.buffer, client.CountGetPacketData, headerSize, ReadDataCallback, new Tuple<ServerClient>(client));
            //
        }
        catch (Exception ex)
        {
            Debug.Log($"Error: {ex.Message}");
            client.stream.Close();
            clients.TryRemove(client, out _);
        }
    }
    void ReadDataCallback(IAsyncResult ar)
    {
        var state = (Tuple<ServerClient>)ar.AsyncState;
        var client = state.Item1;
        try
        {
            int bytesRead = client.stream.EndRead(ar);

            if (bytesRead == 0)
            {
                // Соединение было закрыто сервером
                Debug.Log("Соединение закрыто(2)");
                client.stream.Close();
                clients.TryRemove(client, out _);
                return;
            }
            PacketDecryptor packet = new PacketDecryptor(client.buffer);
            client.CountGetPacketData = 0;

            messageQueue.Enqueue(Tuple.Create(client, packet));

            client.buffer = new byte[HeaderSize];
            client.stream.BeginRead(client.buffer, 0, HeaderSize, ReadHeaderCallback, new Tuple<ServerClient>(client));
            //Debug.Log("Packet get, find new");
            //
        }
        catch (Exception ex)
        {
            Debug.Log($"Error: {ex.Message}");
            client.stream.Close();
            clients.TryRemove(client, out _);
        }
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
        switch ((WorldCommand) packetid)
        {
            case (WorldCommand.MSG_NULL_ACTION):
            {
                Debug.Log("SERVER: Клиент подтвердил, что его id - " + packet.ReadInt());
                if(clients.Count == 2){
                    Debug.Log("SERVER: Начинаем игру!");
                    int counter = 0;
                    foreach (var client in clients.Keys)
                    {
                        Packet apacket = new Packet((int) PacketHeaders.WorldCommand.SMSG_START_GAME);
                        apacket.Write((int)counter);
                        counter ++;
                        client.stream.WriteAsync(apacket.GetBytes());
                    }
                }
                break;
            }
            case (WorldCommand.CMSG_PLAYER_LOGIN):
            {

                c.authorized = true;
                int objModelId = packet.ReadInt();

                Vector3 position = new Vector3();
                float rotation;

                c.lastPos[0] = packet.ReadFloat();
                c.lastPos[1] = packet.ReadFloat();
                c.lastPos[2] = packet.ReadFloat();

                c.lastPos[3] = packet.ReadFloat();

                Packet responcePacket = new Packet((int)WorldCommand.SMSG_PLAYER_LOGIN);
                responcePacket.Write((string) c.tcp.Client.RemoteEndPoint.ToString());

                responcePacket.Write((float) c.lastPos[0]);
                responcePacket.Write((float) c.lastPos[1]);
                responcePacket.Write((float) c.lastPos[2]);
                responcePacket.Write((float) c.lastPos[3]);

                foreach (ServerClient client in clients.Keys)//Отправляем всем игрокам позицию нового игрока
                {
                    if(c.tcp == client.tcp) continue;
                    if(client.authorized == false) continue;
                    client.stream.WriteAsync(responcePacket.GetBytes());
                    Debug.Log($"Тестирование - {client.tcp.Client.RemoteEndPoint.ToString()}");
                }

                //
                Packet playersPacket = new Packet((int) WorldCommand.SMSG_CREATE_PLAYERS);

                int counter = 0;
                foreach (ServerClient client in clients.Keys)//Отправляем всем игрокам позицию нового игрока
                {
                    if(c.tcp == client.tcp) continue;
                    if(client.authorized == false) continue;
                    counter ++;
                }

                playersPacket.Write((int) counter);
                
                foreach (ServerClient client in clients.Keys)//Отправляем всем игрокам позицию нового игрока
                {
                    if(c.tcp == client.tcp) continue;
                    if(client.authorized == false) continue;
                    
                    playersPacket.Write((string) client.tcp.Client.RemoteEndPoint.ToString());
                    playersPacket.Write((float) client.lastPos[0]);
                    playersPacket.Write((float) client.lastPos[1]);
                    playersPacket.Write((float) client.lastPos[2]);
                    playersPacket.Write((float) client.lastPos[3]);
                }

                c.stream.WriteAsync(playersPacket.GetBytes());

                break;
            }
            case (WorldCommand.CMSG_OBJ_INFO):
            {
                //int objectId = packet.ReadInt();

                int before = packet.ReadInt();

                Packet responcePacket = new Packet( (int) WorldCommand.SMSG_OBJ_INFO);

                responcePacket.Write((string) c.tcp.Client.RemoteEndPoint.ToString());

                responcePacket.Write((int) before);

                for(int i = 0; i < 3; i ++){
                    switch (i)
                    {
                        case 0:
                        {
                            if(before / 100 >= 1){
                                //Debug.Log($"POS: {packet.ReadFloat()}; {packet.ReadFloat()}; {packet.ReadFloat()}");
                                responcePacket.Write((float) packet.ReadFloat());
                                responcePacket.Write((float) packet.ReadFloat());
                                responcePacket.Write((float) packet.ReadFloat());
                            }
                            break;
                        }
                        case 1:
                        {
                            if((before % 100) / 10 >= 1)
                            {
                                //Debug.Log($"ROT: {packet.ReadFloat()}; {packet.ReadFloat()}; {packet.ReadFloat()}");
                                responcePacket.Write((float) packet.ReadFloat());
                            }
                            break;
                        }
                        case 2:
                        {
                            if(before % 10 >= 1)
                            {
                                //Debug.Log($"SPEED: {packet.ReadFloat()}; {packet.ReadFloat()}; {packet.ReadFloat()}");
                                responcePacket.Write((float) packet.ReadFloat());
                                responcePacket.Write((float) packet.ReadFloat());
                                responcePacket.Write((float) packet.ReadFloat());
                            }
                            break;
                        }
                    }
                }

                foreach (ServerClient client in clients.Keys)
                {
                    if(c == client) continue;
                    if(c.authorized == false) continue;
                    
                    client.stream.WriteAsync(responcePacket.GetBytes());
                }
                //Debug.Log("POS: " +packet.ReadFloat() + "; " + packet.ReadFloat() + "; " + packet.ReadFloat() + "; ");
                break;
            }

            case (WorldCommand.CMSG_CREATE_BULLET):
            {
                Packet responcePacket = new Packet((int)WorldCommand.SMSG_CREATE_BULLET);
                
                //Позиция
                responcePacket.Write((float) packet.ReadFloat());
                responcePacket.Write((float) packet.ReadFloat());
                responcePacket.Write((float) packet.ReadFloat());

                //ротация
                responcePacket.Write((float) packet.ReadFloat());
                responcePacket.Write((float) packet.ReadFloat());
                responcePacket.Write((float) packet.ReadFloat());

                //Скорость
                responcePacket.Write((float) packet.ReadFloat());
                responcePacket.Write((float) packet.ReadFloat());
                responcePacket.Write((float) packet.ReadFloat());

                foreach (ServerClient client in clients.Keys)
                {
                    if(c == client) continue;
                    if(c.authorized == false) continue;
                    
                    client.stream.WriteAsync(responcePacket.GetBytes());
                }

                break;
            }
            
        }
    }


    void OnDestroy()
    {
        if(serverStarted)server.Stop();
        if(serverThread != null) serverThread.Abort();
    }
}