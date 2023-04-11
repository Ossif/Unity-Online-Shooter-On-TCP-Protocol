using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//namespace socketAPP
//{
    class PacketDecryptor
    {
        private byte[] _data;
        private int _offset;

        public PacketDecryptor(byte[] data)
        {
            _data = data;
            _offset = 6; // пропускаем 6 байт заголовка пакета
        }

        public int GetPacketId()
        {
            return BitConverter.ToUInt16(_data, 0);
        }

        public int ReadInt()
        {
            int value = BitConverter.ToInt32(_data, _offset);
            _offset += sizeof(int);
            return value;
        }

        public float ReadFloat()
        {
            float value = BitConverter.ToSingle(_data, _offset);
            _offset += sizeof(float);
            return value;
        }

        public double ReadDouble()
        {
            double value = BitConverter.ToDouble(_data, _offset);
            _offset += sizeof(double);
            return value;
        }

        public short ReadInt16()
        {
            short value = BitConverter.ToInt16(_data, _offset);
            _offset += sizeof(short);
            return value;
        }

        public ushort ReadUInt16()
        {
            ushort value = BitConverter.ToUInt16(_data, _offset);
            _offset += sizeof(ushort);
            return value;
        }

        public byte ReadByte()
        {
            byte value = _data[_offset];
            _offset += sizeof(byte);
            return value;
        }
        public string ReadString()
        {
            // Считываем длину строки
            short length = ReadInt16();
            //_offset += sizeof(ushort);
            
            // Считываем байты строки
            byte[] bytes = new byte[length];
            Array.Copy(_data, _offset, bytes, 0, length);
            _offset += length;

            // Преобразуем байты в строку
            return Encoding.UTF8.GetString(bytes);
        }
    }
//}
