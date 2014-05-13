using System;
using System.Net;
using MonoTorrent.Exceptions;

namespace MonoTorrent.Messages
{
    public abstract class Message : IMessage
    {
        public abstract int ByteLength { get; }

        protected int CheckWritten(int written)
        {
            if (written != this.ByteLength)
                throw new MessageException("Message encoded incorrectly. Incorrect number of bytes written");
            return written;
        }

        public abstract void Decode(byte[] buffer, int offset, int length);

        public byte[] Encode()
        {
            byte[] buffer = new byte[this.ByteLength];
            this.Encode(buffer, 0);
            return buffer;
        }

        public abstract int Encode(byte[] buffer, int offset);

        static public byte ReadByte(byte[] buffer, int offset)
        {
            return buffer[offset];
        }

        static public byte ReadByte(byte[] buffer, ref int offset)
        {
            byte b = buffer[offset];
            offset++;
            return b;
        }

        static public byte[] ReadBytes(byte[] buffer, int offset, int count)
        {
            return ReadBytes(buffer, ref offset, count);
        }

        static public byte[] ReadBytes(byte[] buffer, ref int offset, int count)
        {
            byte[] result = new byte[count];
            Buffer.BlockCopy(buffer, offset, result, 0, count);
            offset += count;
            return result;
        }

        static public short ReadShort(byte[] buffer, int offset)
        {
            return ReadShort(buffer, ref offset);
        }

        static public short ReadShort(byte[] buffer, ref int offset)
        {
            short ret = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(buffer, offset));
            offset += 2;
            return ret;
        }

        static public string ReadString(byte[] buffer, int offset, int count)
        {
            return ReadString(buffer, ref offset, count);
        }

        static public string ReadString(byte[] buffer, ref int offset, int count)
        {
            string s = System.Text.Encoding.ASCII.GetString(buffer, offset, count);
            offset += count;
            return s;
        }

        static public int ReadInt(byte[] buffer, int offset)
        {
            return ReadInt(buffer, ref offset);
        }

        static public int ReadInt(byte[] buffer, ref int offset)
        {
            int ret = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buffer, offset));
            offset += 4;
            return ret;
        }

        static public long ReadLong(byte[] buffer, int offset)
        {
            return ReadLong(buffer, ref offset);
        }

        static public long ReadLong(byte[] buffer, ref int offset)
        {
            long ret = IPAddress.NetworkToHostOrder(BitConverter.ToInt64(buffer, offset));
            offset += 8;
            return ret;
        }

        static public int Write(byte[] buffer, int offset, byte value)
        {
            buffer[offset] = value;
            return 1;
        }

        static public int Write(byte[] dest, int destOffset, byte[] src, int srcOffset, int count)
        {
            Buffer.BlockCopy(src, srcOffset, dest, destOffset, count);
            return count;
        }

        static public int Write(byte[] buffer, int offset, ushort value)
        {
            return Write(buffer, offset, (short)value);
        }

        static public int Write(byte[] buffer, int offset, short value)
        {
            offset += Write(buffer, offset, (byte)(value >> 8));
            offset += Write(buffer, offset, (byte)value);
            return 2;
        }

        static public int Write(byte[] buffer, int offset, int value)
        {
            offset += Write(buffer, offset, (byte)(value >> 24));
            offset += Write(buffer, offset, (byte)(value >> 16));
            offset += Write(buffer, offset, (byte)(value >> 8));
            offset += Write(buffer, offset, (byte)(value));
            return 4;
        }

        static public int Write(byte[] buffer, int offset, uint value)
        {
            return Write(buffer, offset, (int)value);
        }

        static public int Write(byte[] buffer, int offset, long value)
        {
            offset += Write(buffer, offset, (int)(value >> 32));
            offset += Write(buffer, offset, (int)value);
            return 8;
        }

        static public int Write(byte[] buffer, int offset, ulong value)
        {
            return Write(buffer, offset, (long)value);
        }

        static public int Write(byte[] buffer, int offset, byte[] value)
        {
            return Write(buffer, offset, value, 0, value.Length);
        }

        static public int WriteAscii(byte[] buffer, int offset, string text)
        {
            for (int i = 0; i < text.Length; i++)
                Write(buffer, offset + i, (byte)text[i]);
            return text.Length;
        }
    }
}