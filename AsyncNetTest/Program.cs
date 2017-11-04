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
            
            for (int i = 0; i < 100000; i++)
            {
                Client s = new Client(i);
                s.Init();
                s.SendMsg();
                await s.Run();
            }
            
        }
    }
}
