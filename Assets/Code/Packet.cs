using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

//namespace socketAPP
//{
public class Packet
{
    private MemoryStream stream;
    private BinaryWriter writer;

    public Packet(int packetId)
    {
        stream = new MemoryStream();
        writer = new BinaryWriter(stream);

        // write packet header
        writer.Write((ushort)packetId);
        writer.Write((uint)0); // placeholder for packet size
    }

    public byte[] GetBytes()
    {
        // update packet size in header
        int packetSize = (int)stream.Length - 6; // отнимаем 6, так как нам нужна длинна пакета без учета заголовка
        stream.Seek(sizeof(ushort), SeekOrigin.Begin);
        writer.Write((uint)packetSize);

        return stream.ToArray();
    }

    public void Write(byte value)
    {
        writer.Write(value);
    }

    public void Write(short value)
    {
        writer.Write(value);
    }

    public void Write(ushort value)
    {
        writer.Write(value);
    }

    public void Write(int value)
    {
        writer.Write(value);
    }

    public void Write(uint value)
    {
        writer.Write(value);
    }

    public void Write(float value)
    {
        writer.Write(value);
    }

    public void Write(double value)
    {
        writer.Write(value);
    }

    public void Write(string value)
    {
        // Преобразуем строку в массив байтов
        byte[] bytes = Encoding.UTF8.GetBytes(value);

        // Записываем длину строки
        Write((short)bytes.Length);

        // Записываем байты строки
        writer.Write(bytes);
    }
}
//}
