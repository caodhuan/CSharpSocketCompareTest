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
        ClientNetwork sn = new ClientNetwork();
        Stopwatch sw = new Stopwatch();
        int count = 0;
        Int32 index;
        public Client(Int32 index)
        {
            this.index = index;
        }

        public bool Init()
        {
            sw.Start();
            sn.Connect("127.0.0.1", 1334);

            sn.RegisterMessageHandler((Int32)Command.ResContent, HandlerContent);
            sn.RegisterMessageHandler((Int32)Command.ResRawContent, HandlerData);
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

        public void SendMsg()
        {
            byte[] bytes = Encoding.UTF8.GetBytes("这是个一个测试阿什顿发斯蒂芬开啦就速度发奖蝶恋蜂狂");

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

            if (count == 10000)
            {
                sw.Stop();
                Console.WriteLine( "index = " + index.ToString() + ", timespan: " + sw.ElapsedMilliseconds);
                Console.WriteLine(content.RawData.ToStringUtf8());
                sn.Stop();
                return;
            }
            count++;
            
            sn.SendMsg((Int32)Command.ReqContent, content);
        }
    }
}
