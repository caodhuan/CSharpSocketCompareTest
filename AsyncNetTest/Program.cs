using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncNetTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            StartTest().GetAwaiter().GetResult();
            sw.Stop();

            Console.WriteLine("total time" + sw.ElapsedMilliseconds);
        }

        static async Task StartTest()
        {

            //             ParallelLoopResult result = Parallel.For(0, 1000, (i) =>
            //             {
            //                 Client s = new Client(i);
            //                 s.Init();
            //                 s.SendMsg();
            //                 s.Run().GetAwaiter().GetResult();
            //             });

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 10; i++)
            {
                sb.Append("这是个一个测试阿什顿发斯蒂芬开啦就速度发奖蝶恋蜂狂");
            }

            byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());
            for (int i = 0; i < 1000; i++)
            {
                Client s = new Client(i);
                s.Init();
                s.SendMsg(bytes);
                await s.Run();

            }

            Console.WriteLine("all sent");
            
            
        }
    }
}
