using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace socketAPP
{
    class Program
    {
        static ConcurrentDictionary<TcpClient, object> clients = new ConcurrentDictionary<TcpClient, object>();

        static async Task Main(string[] args)
        {
            Console.WriteLine("Введите 1 чтобы запустить сервер");
            Console.WriteLine("Введите 2 чтобы подключиться к серверу");
            int Choose = Convert.ToInt32(Console.ReadLine());
            if (Choose == 1) //Если выбрали запустить сервер
            {
                TcpListener listener = new TcpListener(IPAddress.Any, 25543);
                listener.Start();
                Console.WriteLine("Server started. Listening for connections...");

                while (true)
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    clients.TryAdd(client, null);
                    Console.WriteLine("Client connected: {0}", client.Client.RemoteEndPoint);

                    // Начинаем асинхронный прием данных от клиента
                    Receive(client);
                }
            }
            else //Если выбрали запустить клиент
            {
                TcpClient client = new TcpClient();
                try
                {
                    await client.ConnectAsync("127.0.0.1", 25543);

                    NetworkStream stream = client.GetStream();
                    byte[] buffer = new byte[1024];

                    //Создаём пакет который отправим полсе подключения к серверу
                    Packet packet = new Packet(0);
                    packet.Write(123);
                    packet.Write((float)3.14);
                    await stream.WriteAsync(packet.GetBytes(), 0, packet.GetBytes().Length); //отправляем

                    Console.WriteLine($"Packet send: {PrintByteArray(packet.GetBytes())}");

                    //начинаем ждать ответ
                    stream.BeginRead(buffer, 0, buffer.Length, ReadCallback, new Tuple<NetworkStream, byte[]>(stream, buffer));

                    //Если нажали любую кнопку - закрываем консоль.
                    Console.ReadKey();
                }
                catch(System.Exception err)
                {
                    Console.WriteLine(err);
                }
            }
        }
        static void ReadCallback(IAsyncResult ar)
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
                    Console.WriteLine("Соединение закрыто");
                    stream.Close();
                    return;
                }

                // Обработка принятых данных
                Console.WriteLine("Received: {0}", Encoding.ASCII.GetString(buffer, 0, bytesRead));

                // Начинаем новый асинхронный прием данных
                stream.BeginRead(buffer, 0, buffer.Length, ReadCallback, new Tuple<NetworkStream, byte[]>(stream, buffer));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
                stream.Close();
            }
        }
        static async Task Receive(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            try
            {
                while (true)
                {
                    if (stream.DataAvailable)
                    {
                        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                        if (bytesRead == 0)
                        {
                            // Соединение было закрыто клиентом
                            Console.WriteLine("Client disconnected: {0}", client.Client.RemoteEndPoint);
                            stream.Close();
                            client.Close();
                            clients.TryRemove(client, out _);
                            break;
                        }

                        // Обработка принятых данных
                       
                        PacketDecryptor InComePacket = new PacketDecryptor(buffer);
                        int packetid = InComePacket.GetPacketId();
                        Console.WriteLine($"Packet take({packetid}):");
                        if (packetid == 0)
                        { 
                            Console.WriteLine($"Received: {InComePacket.ReadInt()} {InComePacket.ReadFloat()}");
                        }                       
                        if (packetid == 1)
                        { 
                            Console.WriteLine($"Received: {InComePacket.ReadFloat()} {InComePacket.ReadInt()}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
                stream.Close();
                client.Close();
                clients.TryRemove(client, out _);
            }
        }
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
    }
}

