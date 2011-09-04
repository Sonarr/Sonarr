using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Ninject;

namespace NzbDrone.Core.Providers.Core
{
    public class UdpProvider
    {
        [Inject]
        public UdpProvider()
        {
            
        }

        private const int StandardPort = 9777;
        private const int MaxPacketSize = 1024;
        private const int HeaderSize = 32;
        private const int MaxPayloadSize = MaxPacketSize - HeaderSize;
        private const byte MajorVersion = 2;
        private const byte MinorVersion = 0;

        public enum PacketType
        {
            Helo = 0x01,
            Bye = 0x02,
            Button = 0x03,
            Mouse = 0x04,
            Ping = 0x05,
            Broadcast = 0x06,  //Currently not implemented
            Notification = 0x07,
            Blob = 0x08,
            Log = 0x09,
            Action = 0x0A,
            Debug = 0xFF //Currently not implemented
        }

        private byte[] Header(PacketType packetType, int numberOfPackets, int currentPacket, int payloadSize, uint uniqueToken)
        {
            byte[] header = new byte[HeaderSize];

            header[0] = (byte)'X';
            header[1] = (byte)'B';
            header[2] = (byte)'M';
            header[3] = (byte)'C';

            header[4] = MajorVersion;
            header[5] = MinorVersion;

            if (currentPacket == 1)
            {
                header[6] = (byte)(((ushort)packetType & 0xff00) >> 8);
                header[7] = (byte)((ushort)packetType & 0x00ff);
            }
            else
            {
                header[6] = (byte)(((ushort)PacketType.Blob & 0xff00) >> 8);
                header[7] = (byte)((ushort)PacketType.Blob & 0x00ff);
            }

            header[8] = (byte)((currentPacket & 0xff000000) >> 24);
            header[9] = (byte)((currentPacket & 0x00ff0000) >> 16);
            header[10] = (byte)((currentPacket & 0x0000ff00) >> 8);
            header[11] = (byte)(currentPacket & 0x000000ff);

            header[12] = (byte)((numberOfPackets & 0xff000000) >> 24);
            header[13] = (byte)((numberOfPackets & 0x00ff0000) >> 16);
            header[14] = (byte)((numberOfPackets & 0x0000ff00) >> 8);
            header[15] = (byte)(numberOfPackets & 0x000000ff);

            header[16] = (byte)((payloadSize & 0xff00) >> 8);
            header[17] = (byte)(payloadSize & 0x00ff);

            header[18] = (byte)((uniqueToken & 0xff000000) >> 24);
            header[19] = (byte)((uniqueToken & 0x00ff0000) >> 16);
            header[20] = (byte)((uniqueToken & 0x0000ff00) >> 8);
            header[21] = (byte)(uniqueToken & 0x000000ff);

            return header;

        }

        public virtual bool Send(string address, PacketType packetType, byte[] payload)
        {
            var uniqueToken = (uint)DateTime.Now.TimeOfDay.Milliseconds;

            var socket = Connect(address, StandardPort);

            if (socket == null || !socket.Connected)
            {
                return false;
            }

            try
            {
                bool successfull = true;
                int packetCount = (payload.Length / MaxPayloadSize) + 1;
                int bytesToSend = 0;
                int bytesSent = 0;
                int bytesLeft = payload.Length;

                for (int Package = 1; Package <= packetCount; Package++)
                {

                    if (bytesLeft > MaxPayloadSize)
                    {
                        bytesToSend = MaxPayloadSize;
                        bytesLeft -= bytesToSend;
                    }
                    else
                    {
                        bytesToSend = bytesLeft;
                        bytesLeft = 0;
                    }

                    byte[] header = Header(packetType, packetCount, Package, bytesToSend, uniqueToken);
                    byte[] packet = new byte[MaxPacketSize];

                    Array.Copy(header, 0, packet, 0, header.Length);
                    Array.Copy(payload, bytesSent, packet, header.Length, bytesToSend);

                    int sendSize = socket.Send(packet, header.Length + bytesToSend, SocketFlags.None);

                    if (sendSize != (header.Length + bytesToSend))
                    {
                        successfull = false;
                        break;
                    }

                    bytesSent += bytesToSend;
                }
                Disconnect(socket);
                return successfull;
            }

            catch
            {
                Disconnect(socket);
                return false;
            }
        }

        private Socket Connect(string address, int port)
        {
            try
            {
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                IPAddress ip;
                if (!IPAddress.TryParse(address, out ip))
                {
                    IPHostEntry ipHostEntry = Dns.GetHostEntry(address);
                    foreach (IPAddress ipAddress in ipHostEntry.AddressList)
                    {
                        if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                        {
                            ip = ipAddress;
                            break;
                        }
                    }
                }

                socket.Connect(new IPEndPoint(ip, port));
                return socket;
            }

            catch (Exception exc)
            {
                Console.WriteLine(exc);
                return null;
            }
        }

        private void Disconnect(Socket socket)
        {
            try
            {
                if (socket != null)
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
            }
            catch
            {
            }
        }
    }
}
