using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkLib;
using Google.Protobuf;

namespace NetTestServer
{
    public class Server
    {
        ServerNetwork sn = new ServerNetwork();
        public Server()
        {

        }

        public bool Init()
        {
            sn.Listen();

            sn.RegisterMessageHandler((Int32)Command.ReqContent, HandlerRawData);
            sn.RegisterMessageHandler((Int32)Command.ReqRawContent, HandlerData);
            return true;
        }
        public void Run()
        {
            sn.Run();
        }

        public void Stop()
        {
            sn.Stop();
        }
        public void HandlerRawData(CodedInputStream stream, NetworkInterface client)
        {
            //Console.WriteLine("HandlerRawData", client.GetSocket());
            RawContent rawContent = RawContent.Parser.ParseFrom(stream);

            sn.SendMsg((Int32)Command.ResContent, rawContent, client);
        }

        public void HandlerData(CodedInputStream stream, NetworkInterface client)
        {
           // Console.WriteLine("HandlerData", client.GetSocket());
            RawContent rawContent = RawContent.Parser.ParseFrom(stream);

            sn.SendMsg((Int32)Command.ResRawContent, rawContent, client);
        }
    }
}
