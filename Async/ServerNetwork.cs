using Google.Protobuf;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncNetworkLib
{
    public struct QueueMsg
    {
        public byte[] package;
        public NetworkInterface client;
    }

    public class ServerNetwork
    {
        const int PacketHeadSize = 8;
        TcpListener server;
        Dictionary<NetworkStream, NetworkInterface> clients = new Dictionary<NetworkStream, NetworkInterface>();
        Dictionary<Int32, Action<CodedInputStream, NetworkInterface>> handlers = new Dictionary<int, Action<CodedInputStream, NetworkInterface>>();

        // 每个消息包的填充队列
        BlockingCollection<QueueMsg> blockingQueue = new BlockingCollection<QueueMsg>();

        bool run = true;
        public ServerNetwork()
        {
        }

        public void Listen(Int32 port = 1334)
        {
            Trace.Assert(server == null);
            server = new TcpListener(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port));
            server.Start();
            Console.WriteLine("listening 127.0.0.1, port = " + port);
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

        public async Task Run()
        {
            Thread t = new Thread(() =>
            {
                foreach (var item in blockingQueue.GetConsumingEnumerable())
                {
                    int currentIndex = 0;

                    Int32 msgLen = System.BitConverter.ToInt32(item.package, currentIndex);
                    currentIndex += 4;
                    msgLen = IPAddress.NetworkToHostOrder(msgLen);

                    Int32 cmd = System.BitConverter.ToInt32(item.package, currentIndex);
                    currentIndex += 4;

                    cmd = IPAddress.NetworkToHostOrder(cmd);
                    CodedInputStream input = new CodedInputStream(item.package, currentIndex, msgLen);
                    OnMessageReceived(cmd, input, item.client);
                }
            });

            t.IsBackground = true;
            t.Start();

            while (run)
            {
                TcpClient client = await server.AcceptTcpClientAsync();

                StartProcessClient(client);
            }
        }

        private async void StartProcessClient(TcpClient client)
        {
            NetworkInterface ni = new NetworkInterface(client.GetStream());
            clients.Add(ni.GetStream(), ni);
            while (run)
            {
                try
                {
                    Int32 size = await ni.StartRead();
                    if (size <= 0)
                    {
                        RemoveClient(ni.GetStream());
                        client.Close();
                        break;
                    }
                    DispatchMessage(ni);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return;

                }

            }
        }

        private void DispatchMessage(NetworkInterface network)
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

                byte[] tmpData = new byte[PacketHeadSize + msgLen];
                Buffer.BlockCopy(buff, currentIndex - PacketHeadSize, tmpData, 0, PacketHeadSize + msgLen);
                blockingQueue.Add(new QueueMsg()
                {
                    client = network,
                    package = tmpData,
                });

                currentIndex += msgLen;
            }

            network.AdjustReadBuff(currentIndex);
        }

        private void AddClient(NetworkInterface client)
        {
            clients.Add(client.GetStream(), client);
        }

        private void RemoveClient(NetworkStream stream)
        {
            stream.Close();
            clients.Remove(stream);
        }

        public void Stop()
        {
            run = false;
            server.Stop();
        }

        public async void SendMsg(Int32 cmd, IMessage msg, NetworkInterface network)
        {
            int size = msg.CalculateSize();

            byte[] msgBuf = msg.ToByteArray();
            Int32 sendMsgLen = IPAddress.HostToNetworkOrder(size);

            Int32 sendCmd = IPAddress.HostToNetworkOrder(cmd);

            byte[] msgLenBuf = BitConverter.GetBytes(sendMsgLen);
            byte[] msgCmdBuf = BitConverter.GetBytes(sendCmd);

            await network.Send(msgLenBuf, 0, msgLenBuf.Length);
            await network.Send(msgCmdBuf, 0, msgCmdBuf.Length);
            await network.Send(msgBuf, 0, size);

        }
    }
}
