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
    public class ServerNetwork
    {
        int index = 0;
        const int PacketHeadSize = 8;
        NetworkInterface server = new NetworkInterface();
        Dictionary<Socket, NetworkInterface> clients = new Dictionary<Socket, NetworkInterface>();
        
        Dictionary<Int32, Action<CodedInputStream, NetworkInterface>> handlers = new Dictionary<int, Action<CodedInputStream, NetworkInterface>>();
        Reactor reactor = new Reactor();
        public ServerNetwork()
        {
            server.OnReadCallback(() =>
            {
                NetworkInterface client = new NetworkInterface(server.GetSocket().Accept());

                client.OnReadCallback(() =>
                {
                    if (client.Read() <= 0)
                    {
                        RemoveClient(client);
                    }
                    else
                    {
                        DispatchMessage(client);
                    }
                });
                AddClient(client);
            });
        }

        public void Listen(Int32 port = 1334)
        {
            server.Listen("0.0.0.0", port);
            Console.WriteLine("litening 0.0.0.0, port = " + port);
        }

        public bool RegisterMessageHandler(Int32 cmd, Action<CodedInputStream, NetworkInterface> action)
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

        public void OnMessageReceived(Int32 cmd, CodedInputStream inputstream, NetworkInterface network)
        {
            if (handlers.ContainsKey(cmd))
            {
                handlers[cmd](inputstream, network);
            }
        }

        public void Run()
        {
            reactor.Add(server);
            reactor.Run();
        }

        public void DispatchMessage(NetworkInterface network)
        {
            int currentIndex = 0;
            while (true)
            {
                var buffIndex = network.GetReadedBuffSize();
                if (buffIndex - currentIndex < PacketHeadSize)
                {
                    break;
                }
                var buff = network.GetReadedBuff();
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

                OnMessageReceived(cmd, input, network);

                currentIndex += msgLen;
            }

            network.AdjustReadBuff(currentIndex);
        }

        private void AddClient(NetworkInterface client)
        {
            
            //Console.WriteLine("增加一个客户端" + ++index);
            clients.Add(client.GetSocket(), client);
            reactor.Add(client);
        }

        private void RemoveClient(NetworkInterface client)
        {
            //Console.WriteLine("移除一个客户端");
            client.GetSocket().Close();
            clients.Remove(client.GetSocket());
            reactor.Remove(client);
        }

        public void Stop()
        {
            reactor.Stop();
        }

        public void SendMsg(Int32 cmd, IMessage msg, NetworkInterface network)
        {
            int size = msg.CalculateSize();

            byte[] msgBuf = msg.ToByteArray();
            Int32 sendMsgLen = IPAddress.HostToNetworkOrder(size);

            Int32 sendCmd = IPAddress.HostToNetworkOrder(cmd);

            byte[] msgLenBuf = BitConverter.GetBytes(sendMsgLen);
            byte[] msgCmdBuf = BitConverter.GetBytes(sendCmd);


            // 注意， 这里可能会有问题！
            // 有可能会发不过去！
            network.Send(msgLenBuf, 0, msgLenBuf.Length);
            network.Send(msgCmdBuf, 0, msgCmdBuf.Length);

            Int32 needSentIndex = 0;
            do
            {
                Int32 sentSize = network.Send(msgBuf, needSentIndex, size - needSentIndex);
                needSentIndex += sentSize;

            } while (needSentIndex != size);

        }
    }
}
