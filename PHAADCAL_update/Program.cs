using System;
using System.Collections.Generic;
using Basic;
using HIS_DB_Lib;
using Oracle.ManagedDataAccess.Client;
using SQLUI;
namespace PHAADCAL_update
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
            Console.WriteLine($"----------------------------------");
            Console.WriteLine($"新增及搜索[術中][術後]醫令");
            Console.WriteLine($"----------------------------------");
            SQLControl sQLControl_醫囑資料 = new SQLControl(MySQL_server, MySQL_database, "order_list", MySQL_userid, MySQL_password, (uint)MySQL_port.StringToInt32(), MySql.Data.MySqlClient.MySqlSslMode.None);

            DateTime dt_st = new DateTime(DateTime.Now.AddDays(-2).Year, DateTime.Now.AddDays(-2).Month, DateTime.Now.AddDays(-2).Day, 00, 00, 00);
            DateTime dt_end = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);
            List<object[]> list_order = sQLControl_醫囑資料.GetRowsByBetween("order_list", (int)enum_醫囑資料.開方日期, dt_st.ToDateTimeString(), dt_end.ToDateTimeString());
            List<object[]> list_order_buf = new List<object[]>();
            List<object[]> list_order_add = new List<object[]>();
            string conn_str = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.24.211)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=SISDCP)));User ID=tphphaadc;Password=tph@phaadc2860;";
            OracleConnection conn_oracle;
            OracleDataReader reader;
            OracleCommand cmd;
            string commandText = "";
            try
            {

                DateTime datetime = DateTime.Now;
                conn_oracle = new OracleConnection(conn_str);
                conn_oracle.Open();

                commandText += "select ";
                commandText += "min(PAC_VISITDT) PAC_VISITDT,";
                commandText += "sum(PAC_SUMQTY) PAC_SUMQTY,";
                commandText += "PAC_ORDERSEQ,";
                commandText += "PAC_SEQ,";
                commandText += "PAC_DIACODE,";
                commandText += "PAC_DIANAME,";
                commandText += "PAC_PATNAME,";
                commandText += "PAC_PATID,";
                commandText += "PAC_UNIT,";
                commandText += "PAC_QTYPERTIME,";
                commandText += "PAC_FEQNO,";
                commandText += "PAC_PATHNO,";
                commandText += "PAC_DAYS,";
                commandText += "PAC_TYPE,";
                commandText += "PAC_DRUGNO,";
                commandText += "PAC_PROCDTTM, ";
                commandText += "PAC_ORDERKIND ";

                commandText += $"from PHAADCAL where SUBSTR(PAC_PROCDTTM, 1, 8) = '{datetime.Year}{datetime.Month.ToString("00")}{datetime.Day.ToString("00")}' AND (PAC_ORDERKIND = '1' OR PAC_ORDERKIND = '2')";
                commandText += "GROUP BY ";

                commandText += "PAC_ORDERSEQ,";
                commandText += "PAC_SEQ,";
                commandText += "PAC_DIACODE,";
                commandText += "PAC_DIANAME,";
                commandText += "PAC_PATNAME,";
                commandText += "PAC_PATID,";
                commandText += "PAC_UNIT,";
                commandText += "PAC_QTYPERTIME,";
                commandText += "PAC_FEQNO,";
                commandText += "PAC_PATHNO,";
                commandText += "PAC_DAYS,";
                commandText += "PAC_TYPE,";
                commandText += "PAC_DRUGNO,";
                commandText += "PAC_PROCDTTM, ";
                commandText += "PAC_ORDERKIND ";

                cmd = new OracleCommand(commandText, conn_oracle);

                reader = cmd.ExecuteReader();
                List<OrderClass> orderClasses = new List<OrderClass>();
      
                try
                {
                    while (reader.Read())
                    {

                        OrderClass orderClass = new OrderClass();
                        string type = reader["PAC_TYPE"].ToString().Trim();
                        if (type == "E") orderClass.藥局代碼 = "PHER";
                        if (type == "S") orderClass.藥局代碼 = "STAT";
                        if (type == "B") orderClass.藥局代碼 = "首日量";
                        if (type == "O") orderClass.藥局代碼 = "OPD";
                        if (type == "M") orderClass.藥局代碼 = "出院帶藥";

                        orderClass.PRI_KEY = $"{reader["PAC_ORDERSEQ"].ToString().Trim()}-{reader["PAC_DRUGNO"].ToString().Trim()}-{reader["PAC_PATID"].ToString().Trim()}";
                        orderClass.藥袋條碼 = $"{reader["PAC_VISITDT"].ToString().Trim()}{reader["PAC_PATID"].ToString().Trim()}{reader["PAC_SEQ"].ToString().Trim()}";
                        orderClass.藥品碼 = reader["PAC_DIACODE"].ToString().Trim();
                        orderClass.藥品名稱 = reader["PAC_DIANAME"].ToString().Trim();
                        orderClass.病人姓名 = reader["PAC_PATNAME"].ToString().Trim();
                        orderClass.病歷號 = reader["PAC_PATID"].ToString().Trim();
                        orderClass.領藥號 = reader["PAC_DRUGNO"].ToString().Trim();
                        orderClass.藥袋類型 = reader["PAC_ORDERKIND"].ToString().Trim();

                        string PAC_QTYPERTIME = reader["PAC_QTYPERTIME"].ToString().Trim();
                        string PAC_SUMQTY = reader["PAC_SUMQTY"].ToString().Trim();
                        double sumQTY = PAC_SUMQTY.StringToDouble();
                        //sumQTY = Math.Ceiling(sumQTY);
                        orderClass.交易量 = (sumQTY * -1).ToString();
                        string Time = reader["PAC_PROCDTTM"].ToString().Trim();
                        if (Time.Length == 14)
                        {
                            string Year = Time.Substring(0, 4);
                            string Month = Time.Substring(4, 2);
                            string Day = Time.Substring(6, 2);
                            string Hour = Time.Substring(8, 2);
                            string Min = Time.Substring(10, 2);
                            string Sec = Time.Substring(12, 2);
                            orderClass.開方日期 = $"{Year}/{Month}/{Day} {Hour}:{Min}:{Sec}";
                        }
                        orderClasses.Add(orderClass);
                        list_order_buf = list_order.GetRows((int)enum_醫囑資料.PRI_KEY, orderClass.PRI_KEY);
                        if(list_order_buf.Count == 0)
                        {
                            orderClass.GUID = Guid.NewGuid().ToString();
                            list_order_add.Add(orderClass.ClassToSQL<OrderClass, enum_醫囑資料>());
                        }

                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Exception : {ex.Message}");
                }

                conn_oracle.Close();
                conn_oracle.Dispose();
                sQLControl_醫囑資料.AddRows(null, list_order_add);
                Console.WriteLine($"搜尋到<{orderClasses.Count}>筆資料,共新增<{list_order_add.Count}>筆資料!");
                System.Threading.Thread.Sleep(3000);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception : {ex.Message}");
            }
            finally
            {

            }
        }
    }
}
