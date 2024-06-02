using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Configuration;
using Basic;
using SQLUI;
using HIS_DB_Lib;
using System.Threading;
using System.Data.OleDb;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Torder_update_batch
{
    class Program
    {
        public enum enum_Torder
        {
            病歷號,
            看診日,
            病患姓名,
            性別,
            年齡,
            醫師代碼,
            科別,
            領藥號,
            藥碼,
            藥名,
            單次劑量,
            單位,
            途徑,
            頻次,
            天數,
            總量,
            大單位,
            是否磨粉,
            藥品類型,
            醫令類型,
            開方日期,
            開方時間,
            CDIANAME,
            是否撥半,
            是否餐包,
            是否傳送包藥機,
            MEDTCT,
            OLDDRUGNO,
        }

        private static string API_Server = "http://127.0.0.1:4433";


        public static int ConvertToMinguoYear(int year)
        {
            if (year < 1912)
            {
                throw new ArgumentOutOfRangeException(nameof(year), "年份必須大於等於 1912 年。");
            }
            return year - 1911;
        }

        /// <summary>
        /// 將西元日期轉換為民國日期。
        /// </summary>
        /// <param name="date">西元日期。</param>
        /// <returns>民國日期字符串。</returns>
        public static string ConvertToMinguoDate(DateTime date)
        {
            int minguoYear = ConvertToMinguoYear(date.Year);
            return $"{minguoYear}{date.Month.ToString("00")}{date.Day.ToString("00")}";
        }

        static void Main(string[] args)
        {


            bool isNewInstance;
            Mutex mutex = new Mutex(true, "Torder_update_batch", out isNewInstance);
            try
            {
                if (!isNewInstance)
                {
                    Console.WriteLine("程式已經在運行中...");
                    return;
                }
                Console.WriteLine($"---------------------------------------------------------------------");

                while (true)
                {
                    List<object[]> list_src_order = new List<object[]>();
                    MyTimerBasic myTimerBasic = new MyTimerBasic(50000);
                    string TimeTaken = "";
                    string dbfFilePath = $@"Y:\DB_601\o{ConvertToMinguoDate(DateTime.Now)}.DBF"; // 替換成你的 DBF 檔案路徑
                    string databaseDirectory = @"Y:\DB_601";
                    string connectionString = $"Provider=Microsoft.Jet.OLEDB.4.0;Data Source='{databaseDirectory}';Extended Properties=dBASE IV;"; // 設定連接字串
                    using (OleDbConnection connection = new OleDbConnection(connectionString))
                    {
                        try
                        {
                  
                            DataTable dataTable = null;
                            // 開啟連接
                            connection.Open();

                            // 執行 SQL 查詢
                            string sqlQuery = "SELECT * FROM " + $"{System.IO.Path.GetFileNameWithoutExtension(dbfFilePath)}.DBF";
                            using (OleDbCommand command = new OleDbCommand(sqlQuery, connection))
                            {

                                using (OleDbDataAdapter adapter = new OleDbDataAdapter(command))
                                {
                                    dataTable = new DataTable();
                                    adapter.Fill(dataTable);

                                    // 輸出資料到控制台

                                    foreach (DataRow row in dataTable.Rows)
                                    {
                                        object[] value = new object[dataTable.Columns.Count];
                                        for (int i = 0; i < dataTable.Columns.Count; i++)
                                        {
                                            value[i] = row[i];
                                        }
                                        list_src_order.Add(value);
                                    }
                                }
                            }
                            TimeTaken = $"{myTimerBasic}";
                            Console.WriteLine($"下載中藥處方資料,共<{list_src_order.Count}>筆...,{TimeTaken}");
                            Logger.Log($"下載中藥處方資料,共<{list_src_order.Count}>筆...,{TimeTaken}");
            

                            Table table = OrderTClass.init(API_Server);
                            if (table == null)
                            {
                                Console.WriteLine("中藥醫令Table初始化失敗");
                                Logger.Log($"中藥醫令Table初始化失敗");
                                return;
                            }
                            SQLControl sQLControl_醫囑資料 = new SQLControl(table);

                            DateTime dateTime_st = DateTime.Now.AddDays(-1);
                            dateTime_st = new DateTime(dateTime_st.Year, dateTime_st.Month, dateTime_st.Day, 00, 00, 00);
                            DateTime dateTime_end = DateTime.Now.AddDays(2);
                            dateTime_end = new DateTime(dateTime_end.Year, dateTime_end.Month, dateTime_end.Day, 23, 59, 59);

                            List<object[]> list_order = sQLControl_醫囑資料.GetRowsByBetween(null, (int)enum_OrderT.產出時間, dateTime_st.ToDateTimeString(), dateTime_end.ToDateTimeString());
                            List<object[]> list_order_buf = new List<object[]>();
                            List<object[]> list_order_add = new List<object[]>();

                            TimeTaken = $"{myTimerBasic}";
                            Console.WriteLine($"從資料庫讀取處方資料,共<{list_order.Count}>筆...,{TimeTaken}");
                            Logger.Log($"從資料庫讀取處方資料,共<{list_order.Count}>筆...,{TimeTaken}");
             

                            string PRI_KEY = "";
                            string 藥碼 = "";
                            string 藥名 = "";
                            string 病歷號 = "";
                            string 病人姓名 = "";
                            string 科別 = "";
                            string 醫師姓名 = "";
                            string 醫師ID = "";
                            string 總量 = "";
                            string 領藥號 = "";
                            string 開方日期 = "";
                            string 天數 = "";
                            string 單次劑量 = "";
                            string 途徑 = "";
                            string 頻次 = "";

                            string 大單位 = "";
                            string 藥局代碼 = "";
                            string 年齡 = "";
                            string 性別 = "";
                            string 單位 = "";
                            string DateNow = DateTime.Now.ToDateString();

                            for (int i = 0; i < list_src_order.Count; i++)
                            {
                                藥碼 = list_src_order[i][(int)enum_Torder.藥碼].ObjectToString();
                                藥名 = list_src_order[i][(int)enum_Torder.藥名].ObjectToString();
                                病歷號 = list_src_order[i][(int)enum_Torder.病歷號].ObjectToString();
                                病人姓名 = list_src_order[i][(int)enum_Torder.病患姓名].ObjectToString();
                                總量 = list_src_order[i][(int)enum_Torder.總量].ObjectToString();
                                領藥號 = list_src_order[i][(int)enum_Torder.領藥號].ObjectToString();
                                科別 = list_src_order[i][(int)enum_Torder.科別].ObjectToString();
                                醫師ID = list_src_order[i][(int)enum_Torder.醫師代碼].ObjectToString();
                                年齡 = list_src_order[i][(int)enum_Torder.年齡].ObjectToString();
                                性別 = list_src_order[i][(int)enum_Torder.性別].ObjectToString();
                                單位 = list_src_order[i][(int)enum_Torder.單位].ObjectToString();
                                途徑 = list_src_order[i][(int)enum_Torder.途徑].ObjectToString();
                                頻次 = list_src_order[i][(int)enum_Torder.頻次].ObjectToString();
                                單次劑量 = list_src_order[i][(int)enum_Torder.單次劑量].ObjectToString();
                                string 開方日期_date = list_src_order[i][(int)enum_Torder.開方日期].ObjectToString();
                                string 開方日期_time = list_src_order[i][(int)enum_Torder.開方時間].ObjectToString();
                                if (開方日期_date.Length == 8)
                                {
                                    開方日期_date = $"{開方日期_date.Substring(0, 4)}-{開方日期_date.Substring(4, 2)}-{開方日期_date.Substring(6, 2)}";
                                }
                                if (開方日期_time.Length == 4)
                                {
                                    開方日期_time = $"{開方日期_time.Substring(0, 2)}:{開方日期_time.Substring(2, 2)}";
                                }
                                開方日期 = $"{開方日期_date} {開方日期_time}";
                                if (開方日期.Check_Date_String() == false) continue;

                                天數 = list_src_order[i][(int)enum_Torder.天數].ObjectToString();
                                大單位 = list_src_order[i][(int)enum_Torder.大單位].ObjectToString();
                                藥局代碼 = list_src_order[i][(int)enum_Torder.醫令類型].ObjectToString();

                                PRI_KEY = $"{病歷號},{領藥號},{藥局代碼},{開方日期},{DateNow}";

                                if (藥局代碼 == "E") 藥局代碼 = "PHER";
                                if (藥局代碼 == "S") 藥局代碼 = "STAT";
                                if (藥局代碼 == "B") 藥局代碼 = "首日量";
                                if (藥局代碼 == "O") 藥局代碼 = "OPD";
                                if (藥局代碼 == "M") 藥局代碼 = "出院帶藥";

                                list_order_buf = list_order.GetRows((int)enum_OrderT.PRI_KEY, PRI_KEY);

                                if (list_order_buf.Count == 0)
                                {
                                    object[] value = new object[new enum_OrderT().GetLength()];
                                    value[(int)enum_OrderT.GUID] = Guid.NewGuid().ToString();
                                    value[(int)enum_OrderT.PRI_KEY] = PRI_KEY;
                                    value[(int)enum_OrderT.藥局代碼] = 藥局代碼;
                                    value[(int)enum_OrderT.藥品碼] = 藥碼;
                                    value[(int)enum_OrderT.藥品名稱] = 藥名;
                                    value[(int)enum_OrderT.病歷號] = 病歷號;
                                    value[(int)enum_OrderT.交易量] = 總量.StringToDouble() * (-1);
                                    value[(int)enum_OrderT.領藥號] = 領藥號;
                                    value[(int)enum_OrderT.科別] = 科別;
                                    value[(int)enum_OrderT.醫師ID] = 醫師ID;
                                    value[(int)enum_OrderT.病人姓名] = 病人姓名;
                                    value[(int)enum_OrderT.年齡] = 年齡;
                                    value[(int)enum_OrderT.性別] = 性別;
                                    value[(int)enum_OrderT.途徑] = 途徑;
                                    value[(int)enum_OrderT.頻次] = 頻次;
                                    value[(int)enum_OrderT.天數] = 天數;
                                    value[(int)enum_OrderT.劑量單位] = 單位;
                                    value[(int)enum_OrderT.單次劑量] = 單次劑量;
                                    value[(int)enum_OrderT.開方日期] = 開方日期;
                                    value[(int)enum_OrderT.產出時間] = DateTime.Now.ToDateTimeString_6();
                                    value[(int)enum_OrderT.結方日期] = DateTime.MinValue.ToDateTimeString();
                                    value[(int)enum_OrderT.展藥時間] = DateTime.MinValue.ToDateTimeString();
                                    value[(int)enum_OrderT.過帳時間] = DateTime.MinValue.ToDateTimeString();
                                    value[(int)enum_OrderT.狀態] = enum_醫囑資料_狀態.未過帳.GetEnumName();
                                    list_order_add.Add(value);
                                }

                            }
                            List<string> list_病歷號_add = new List<string>();

                            list_病歷號_add = (from temp in list_order_add
                                            select temp[(int)enum_OrderT.病歷號].ObjectToString()).Distinct().ToList();

                            for (int i = 0; i < list_病歷號_add.Count; i++)
                            {
                                list_order_buf = list_order_add.GetRows((int)enum_OrderT.病歷號, list_病歷號_add[i]);
                                for (int k = 0; k < list_order_buf.Count; k++)
                                {
                                    list_order_buf[k][(int)enum_OrderT.批序] = $"{k + 1}";
                                }
                            }

                            sQLControl_醫囑資料.AddRows(null, list_order_add);
                            TimeTaken = $"{myTimerBasic}";
                            Logger.Log($"共新增<{list_order_add.Count}>筆處方,{TimeTaken} ");
                            Console.WriteLine($"共新增<{list_order_add.Count}>筆處方,{TimeTaken} {DateTime.Now.ToDateTimeString()}");
                            Console.WriteLine($"---------------------------------------------------------------------");
                            System.Threading.Thread.Sleep(5000);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Exception：" + ex.Message);
                        }
                        finally
                        {
                            connection.Close();
                            System.Threading.Thread.Sleep(200);
                        }
                    }

                }
            }
            catch (Exception e)
            {

            }
            finally
            {

                mutex.ReleaseMutex(); // 釋放互斥鎖

                Environment.Exit(0);
            }
        }
    }
}
