using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using NetworkLib;
using System.Diagnostics;

namespace NetTest
{
    public class Client
    {
        ClientNetwork sn;
        Stopwatch sw = new Stopwatch();
        int count = 0;
        Int32 index;
        Reactor reactor;
        public Client(Reactor reactor, Int32 index)
        {
            this.index = index;
            this.reactor = reactor;
            sn = new ClientNetwork(reactor);
        }

        public bool Init()
        {
            sw.Start();
            sn.Connect("127.0.0.1", 1334);

            sn.RegisterMessageHandler((Int32)Command.ResContent, HandlerContent);
            sn.RegisterMessageHandler((Int32)Command.ResRawContent, HandlerData);
            return true;
        }
  
        public void SendMsg(byte[] bytes)
        {

            RawContent content = new RawContent();
            content.RawData = ByteString.CopyFrom(bytes, 0, bytes.Length);
            sn.SendMsg((Int32)Command.ReqContent, content);
        }
        public void HandlerContent(CodedInputStream stream)
        {
            RawContent content = RawContent.Parser.ParseFrom(stream);
            sn.SendMsg((Int32)Command.ReqRawContent, content);
        }

        public void HandlerData(CodedInputStream stream)
        {
            RawContent content = RawContent.Parser.ParseFrom(stream);

            if (count == 1000)
            {
                sw.Stop();
                Console.WriteLine( "index = " + index.ToString() + ", timespan: " + sw.ElapsedMilliseconds);
                Console.WriteLine(content.RawData.ToStringUtf8());
                sn.Close();
                return;
            }
            count++;
            
            sn.SendMsg((Int32)Command.ReqContent, content);
        }
    }
}
