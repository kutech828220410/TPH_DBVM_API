using System;
using System.Collections.Generic;
using Basic;
using HIS_DB_Lib;
using Oracle.ManagedDataAccess.Client;
using SQLUI;

namespace 調劑資訊寫回戰情_batch
{
    class Program
    {
        static string MySQL_server = "127.0.0.1";
        static string MySQL_database = "dbvm";
        static string MySQL_userid = "user";
        static string MySQL_password = "66437068";
        static string MySQL_port = "3306";

        static void Main(string[] args)
        {
            try
            {
                Logger.Log($"------------------開始執行------------------");

                Console.WriteLine($"----------------------------------");
                Console.WriteLine($"調劑資訊寫回戰情");
                Console.WriteLine($"----------------------------------");
                SQLControl sQLControl_醫囑資料 = new SQLControl(MySQL_server, MySQL_database, "order_list", MySQL_userid, MySQL_password, (uint)MySQL_port.StringToInt32(), MySql.Data.MySqlClient.MySqlSslMode.None);

                string conn_str = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.24.211)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=SISDCP)));User ID=tphphaadc;Password=tph@phaadc2860;";
                OracleConnection conn_oracle;
                OracleDataReader reader;
                string commandText = "INSERT INTO PHACHK (PCK_VISITDT, PCK_PATID ,PCK_SEQ ,PCK_DIVNO ,PCK_TYPE ,PCK_DRUGNO ,PCK_PROCDTTM ,PCK_PROCOPID ,PCK_CREATEDTTM) VALUES " +
                    "(:PCK_VISITDT, :PCK_PATID ,:PCK_SEQ ,:PCK_DIVNO ,:PCK_TYPE ,:PCK_DRUGNO ,:PCK_PROCDTTM ,:PCK_PROCOPID ,:PCK_CREATEDTTM)";

                conn_oracle = new OracleConnection(conn_str);
                conn_oracle.Open();


                using (OracleCommand cmd = new OracleCommand(commandText, conn_oracle))
                {
                    // 添加参数并赋值
                    cmd.Parameters.Add(new OracleParameter("PCK_VISITDT", "20240602"));
                    cmd.Parameters.Add(new OracleParameter("PCK_PATID", "0000000000"));
                    cmd.Parameters.Add(new OracleParameter("PCK_SEQ", "5"));
                    cmd.Parameters.Add(new OracleParameter("PCK_DIVNO", "0"));
                    cmd.Parameters.Add(new OracleParameter("PCK_TYPE", "1"));
                    cmd.Parameters.Add(new OracleParameter("PCK_DRUGNO", "500"));
                    cmd.Parameters.Add(new OracleParameter("PCK_PROCDTTM", "20240602073900"));
                    cmd.Parameters.Add(new OracleParameter("PCK_PROCOPID", "TEST000"));
                    cmd.Parameters.Add(new OracleParameter("PCK_CREATEDTTM", "20240602073900"));

                    // 执行插入操作
                    int rowsInserted = cmd.ExecuteNonQuery();
                    Console.WriteLine($"{rowsInserted} row(s) inserted");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception : {ex.Message}");
            }
            finally
            {
                Logger.Log($"------------------程序結束------------------");
            }
          
        }
    }
}
