
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Configuration;
using Basic;
using SQLUI;
using Oracle.ManagedDataAccess.Client;
using HIS_DB_Lib;

namespace order_update
{
    class Program
    {
      
        static void Main(string[] args)
        {
            string json = Basic.Net.WEBApiGet($"http://192.168.23.54:443/dbvm/BBAR/order_update?datetime={DateTime.Now.AddDays(-1).ToDateString()}");
            Console.WriteLine($"{json}");
            System.Threading.Thread.Sleep(5000);
        }
    }
}
