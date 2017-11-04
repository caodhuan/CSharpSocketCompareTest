using NetworkLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            StartTest();
            sw.Stop();

            Console.WriteLine("total time" + sw.ElapsedMilliseconds);
        }

        static void StartTest()
        {
            Reactor reactor = new Reactor();
           
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 10; i++)
            {
                sb.Append("这是个一个测试阿什顿发斯蒂芬开啦就速度发奖蝶恋蜂狂");
            }

            byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());
            for (int i = 0; i < 10000; i++)
            {
                Client s = new Client(reactor, i);
                s.Init();
                s.SendMsg(bytes);
            }
            Console.WriteLine("all sent");
            reactor.Run();
            
        }
    }
}
