using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkLib
{
    public class ClientNetwork
    {
        const int PacketHeadSize = 8;
        NetworkInterface self = new NetworkInterface();
        Dictionary<Int32, Action<CodedInputStream>> handlers = new Dictionary<int, Action<CodedInputStream>>();
        List<Socket> checkRead = new List<Socket>();
        bool run = true;
        public ClientNetwork()
        {
        }

        public void Connect(string IP, Int32 port)
        {
            self.Connect(IP, port);
        }

        public bool RegisterMessageHandler(Int32 cmd, Action<CodedInputStream> action)
        {
            if (handlers.ContainsKey(cmd))
            {
                return false;
            }

            handlers.Add(cmd, action);

            return true;
        }
        public void UnregisterMessageHandler(Int32 cmd)
        {
            handlers.Remove(cmd);
        }

        public void OnMessageReceived(Int32 cmd, CodedInputStream inputstream)
        {
            if (handlers.ContainsKey(cmd))
            {
                handlers[cmd](inputstream);
            }
        }

        public Socket GetSocket()
        {
            return self.GetSocket();
        }

        public void Run()
        {

            while (run)
            {
                checkRead.Clear();
                checkRead.Add(self.GetSocket());

                Socket.Select(checkRead, null, null, -1);

                if (checkRead.Count > 0)
                {
                    if (self.Read() <= 0)
                    {
                        // disconnect
                    }
                    else
                    {
                        DispatchMessage();
                    }

                }
            }
        }

        public void Stop()
        {
            run = false;
        }
        public void DispatchMessage()
        {
            int currentIndex = 0;
            while (true)
            {
                var buffIndex = self.GetReadedBuffSize();
                if (buffIndex - currentIndex < PacketHeadSize)
                {
                    break;
                }
                var buff = self.GetReadedBuff();
                Int32 msgLen = System.BitConverter.ToInt32(buff, currentIndex);
                currentIndex += 4;
                msgLen = IPAddress.NetworkToHostOrder(msgLen);

                Int32 cmd = System.BitConverter.ToInt32(buff, currentIndex);
                currentIndex += 4;

                cmd = IPAddress.NetworkToHostOrder(cmd);
                if (buffIndex - currentIndex < msgLen)
                {
                    currentIndex = 0;
                    break;
                }

                CodedInputStream input = new CodedInputStream(buff, currentIndex, msgLen);

                OnMessageReceived(cmd, input);

                currentIndex += msgLen;
            }

            self.AdjustReadBuff(currentIndex);
        }

        public void SendMsg(Int32 cmd, IMessage msg)
        {
            int size = msg.CalculateSize();

            byte[] msgBuf = msg.ToByteArray();
            Int32 sendMsgLen = IPAddress.HostToNetworkOrder(size);

            Int32 sendCmd = IPAddress.HostToNetworkOrder(cmd);

            byte[] msgLenBuf = BitConverter.GetBytes(sendMsgLen);
            byte[] msgCmdBuf = BitConverter.GetBytes(sendCmd);


            // 注意， 这里可能会有问题！
            // 有可能会发不过去！
            self.Send(msgLenBuf, 0, msgLenBuf.Length);
            self.Send(msgCmdBuf, 0, msgCmdBuf.Length);

            Int32 needSentIndex = 0;
            do
            {
                Int32 sentSize = self.Send(msgBuf, needSentIndex, size - needSentIndex);
                needSentIndex += sentSize;

            } while (needSentIndex != size);

        }
    }
}
