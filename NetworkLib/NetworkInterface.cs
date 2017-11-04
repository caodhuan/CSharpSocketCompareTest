using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkLib
{
    public class NetworkInterface
    {
        const Int32 CacheSize = 65536;
        Socket socket;
        byte[] buff = new byte[CacheSize];
        Int32 buffIndex = 0;

        private Action onCanRead;
        public NetworkInterface(Socket socket = null)
        {
            this.socket = socket;
        }

        public Socket GetSocket()
        {
            return socket;
        }


        public void Connect(string IP = "127.0.0.1", Int32 port = 36001)
        {
            if (socket == null)
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }
            socket.Blocking = false;
            socket.NoDelay = true;
            socket.ReceiveBufferSize = CacheSize;
            socket.Connect(new IPEndPoint(IPAddress.Parse(IP), port));
        }

        public void Listen(string IP = "127.0.0.1", Int32 port = 36001)
        {
            if (socket == null)
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }
            
            socket.NoDelay = true;
            socket.ReceiveBufferSize = CacheSize;
            socket.Bind(new IPEndPoint(IPAddress.Parse(IP), port));
            socket.Listen(5);
        }

        public void Disconnect()
        {
            if (socket != null)
            {
                socket.Close();
            }
        }

        public bool Connected()
        {
            return socket != null ? socket.Connected : false;
        }

        public void OnReadCallback(Action action)
        {
            this.onCanRead = action;
        }

        public Action GetReadCallback()
        {
            return this.onCanRead;
        }
        public int Read()
        {
            Int32 canRead = socket.Available;
            if (canRead > 0)
            {

                if (canRead + buffIndex > buff.Length)
                {
                    byte[] newBuff = new byte[canRead + buffIndex];
                    Buffer.BlockCopy(buff, 0, newBuff, 0, buff.Length);
                    buff = newBuff;
                }
                socket.Receive(buff, buffIndex, canRead, SocketFlags.None);
                buffIndex += canRead;

            }
            return canRead;
        }

        public int GetReadedBuffSize()
        {
            return buffIndex;
        }
        public byte[] GetReadedBuff()
        {
            return buff;
        }

        public void AdjustReadBuff(Int32 startPos)
        {
            if (startPos == 0)
            {
                return;
            }
            if (buffIndex == startPos)
            {
                buffIndex = 0;
                return;
            }

            System.Buffer.BlockCopy(buff, startPos, buff, 0, buffIndex - startPos);
            buffIndex -= startPos;

            Trace.Assert(buffIndex >= 0, "接收缓冲区出问题啦！");
        }

        public Int32 Send(byte[] buffer, Int32 offset, Int32 size)
        {

            return socket.Send(buffer, offset, size, SocketFlags.None);
        }
    }
}
