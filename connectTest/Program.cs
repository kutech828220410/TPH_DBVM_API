using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Basic;
using System.Threading;

namespace connectTest
{
    class Program
    {
        static void Main(string[] args)
        {
            bool isNewInstance;
            Mutex mutex = new Mutex(true, "connectTest", out isNewInstance);
            try 
            {
                if (!isNewInstance)
                {
                    Console.ReadKey();
                    Console.WriteLine("程式已經在運行中...");
                    return;
                }
                Console.WriteLine("----------------------------------------------------");
                while (true)
                {
                    Console.WriteLine($"{DateTime.Now.ToDateTimeString()}-測試通訊....");
                    //string API = @"http://192.168.23.54:4434/dbvm/test";
                    string json_out = Basic.Net.WEBApiGet("http://192.168.23.54:4434/dbvm/test");
                    Console.WriteLine($"{DateTime.Now.ToDateTimeString()}-{json_out} ok....");
                    System.Threading.Thread.Sleep(3000);
                    //Console.ReadKey();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception:{ex}");
                Console.ReadKey();
            }


        }
    }
}
