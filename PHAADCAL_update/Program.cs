using System;
using System.Collections.Generic;
using Basic;
using HIS_DB_Lib;
using Oracle.ManagedDataAccess.Client;
using SQLUI;
using System.Threading.Tasks;

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
            string ICD1 = "";
            string ICD2 = "";
            string ICD3 = "";
            string commandText = "";
            string API_Server = "https://pharma-cetrlm.tph.mohw.gov.tw:4443";

            try
            {

                DateTime datetime = DateTime.Now;
                conn_oracle = new OracleConnection(conn_str);
                conn_oracle.Open();

                commandText += "select ";
                commandText += "min(PAC_VISITDT) PAC_VISITDT,";
                commandText += "sum(PAC_SUMQTY) PAC_SUMQTY,";
                commandText += "PAC_ORDERSEQ,"; //醫令序號
                commandText += "PAC_SEQ,"; //序號
                commandText += "PAC_DIACODE,"; //藥品院內碼
                commandText += "PAC_DIANAME,"; //藥品商品名稱
                commandText += "PAC_PATNAME,"; //病歷姓名
                commandText += "PAC_PATID,"; //病歷號
                commandText += "PAC_UNIT,"; //小單位
                commandText += "PAC_QTYPERTIME,"; //次劑量
                commandText += "PAC_FEQNO,"; //頻率
                commandText += "PAC_PATHNO,"; //途徑
                commandText += "PAC_DAYS,"; //使用天數
                commandText += "PAC_TYPE,"; // 醫令類型
                commandText += "PAC_DRUGNO,"; //領藥號
                commandText += "PAC_SECTNAME,"; //科別
                commandText += "PAC_DOCNAME,"; //醫師代碼
                commandText += "PAC_PROCDTTM,"; //醫令開立時間
                commandText += "PAC_PAYCD,"; //費用別
                commandText += "PAC_ICDX1,"; //診斷碼1
                commandText += "PAC_ICDX2,"; //診斷碼2
                commandText += "PAC_ICDX3,"; //診斷碼3
                commandText += "PAC_DRUGGIST "; //藥師代碼


                

                commandText += $"from PHAADCAL where SUBSTR(PAC_PROCDTTM, 1, 8) = '{datetime.Year}{datetime.Month.ToString("00")}{datetime.Day.ToString("00")}' AND (PAC_ORDERKIND = '1' OR PAC_ORDERKIND = '2')";
                commandText += "GROUP BY ";

                commandText += "PAC_ORDERSEQ,"; //醫令序號
                commandText += "PAC_SEQ,"; //序號
                commandText += "PAC_DIACODE,"; //藥品院內碼
                commandText += "PAC_DIANAME,"; //藥品商品名稱
                commandText += "PAC_PATNAME,"; //病歷姓名
                commandText += "PAC_PATID,"; //病歷號
                commandText += "PAC_UNIT,"; //小單位
                commandText += "PAC_QTYPERTIME,"; //次劑量
                commandText += "PAC_FEQNO,"; //頻率
                commandText += "PAC_PATHNO,"; //途徑
                commandText += "PAC_DAYS,"; //使用天數
                commandText += "PAC_TYPE,"; // 醫令類型
                commandText += "PAC_DRUGNO,"; //領藥號
                commandText += "PAC_SECTNAME,"; //科別
                commandText += "PAC_DOCNAME,"; //醫師代碼
                commandText += "PAC_PROCDTTM,"; //醫令開立時間
                commandText += "PAC_PAYCD,"; //費用別
                commandText += "PAC_ICDX1,"; //診斷碼1
                commandText += "PAC_ICDX2,"; //診斷碼2
                commandText += "PAC_ICDX3,"; //診斷碼3
                commandText += "PAC_DRUGGIST "; //藥師代碼

                cmd = new OracleCommand(commandText, conn_oracle);

                reader = cmd.ExecuteReader();
                List<OrderClass> orderClasses = new List<OrderClass>();
      
                try
                {
                    while (reader.Read())
                    {

                        OrderClass orderClass = new OrderClass();
                        string type = reader["PAC_TYPE"].ToString().Trim();
                        if (type == "E") orderClass.藥袋類型 = "PHER";
                        if (type == "S") orderClass.藥袋類型 = "STAT";
                        if (type == "B") orderClass.藥袋類型 = "首日量";
                        if (type == "O") orderClass.藥袋類型 = "OPD";
                        if (type == "M") orderClass.藥袋類型 = "出院帶藥";

                        orderClass.PRI_KEY = $"{reader["PAC_ORDERSEQ"].ToString().Trim()}-{reader["PAC_DRUGNO"].ToString().Trim()}-{reader["PAC_PATID"].ToString().Trim()}";
                        //orderClass.藥袋條碼 = $"{reader["PAC_VISITDT"].ToString().Trim()}{reader["PAC_PATID"].ToString().Trim()}{reader["PAC_SEQ"].ToString().Trim()}";
                        orderClass.住院序號 = reader["PAC_SEQ"].ToString().Trim();

                        orderClass.藥品碼 = reader["PAC_DIACODE"].ToString().Trim();
                        orderClass.藥品名稱 = reader["PAC_DIANAME"].ToString().Trim();
                        orderClass.病人姓名 = reader["PAC_PATNAME"].ToString().Trim();
                        orderClass.病歷號 = reader["PAC_PATID"].ToString().Trim();
                        orderClass.領藥號 = reader["PAC_DRUGNO"].ToString().Trim();
                        //orderClass.藥袋類型 = reader["PAC_ORDERKIND"].ToString().Trim();
                        orderClass.就醫時間 = reader["PAC_VISITDT"].ToString().Trim();
                        orderClass.就醫時間 = $"{orderClass.就醫時間.Substring(0, 4)}-{orderClass.就醫時間.Substring(4, 2)}-{orderClass.就醫時間.Substring(6, 2)}";
                        orderClass.科別 = reader["PAC_SECTNAME"].ToString().Trim();
                        orderClass.醫師代碼 = reader["PAC_DOCNAME"].ToString().Trim();
                        orderClass.頻次 = reader["PAC_FEQNO"].ToString().Trim();
                        orderClass.天數 = reader["PAC_DAYS"].ToString().Trim();
                        orderClass.單次劑量 = reader["PAC_QTYPERTIME"].ToString().Trim();
                        orderClass.劑量單位 = reader["PAC_UNIT"].ToString().Trim();
                        string PAC_QTYPERTIME = reader["PAC_QTYPERTIME"].ToString().Trim();
                        string PAC_SUMQTY = reader["PAC_SUMQTY"].ToString().Trim();
                        double sumQTY = PAC_SUMQTY.StringToDouble();
                        //sumQTY = Math.Ceiling(sumQTY);
                        orderClass.交易量 = (sumQTY * -1).ToString();

                        if (reader["PAC_PAYCD"].ToString().Trim() == "Y")
                        {
                            orderClass.費用別 = "自費";
                        }
                        else
                        {
                            orderClass.費用別 = "健保";
                        }

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

                List<Task> tasks = new List<Task>();
                tasks.Add(Task.Run(new Action(delegate
                {

                    if (orderClasses.Count == 0) return;
                    List<suspiciousRxLogClass> suspiciousRxLoges = suspiciousRxLogClass.get_by_barcode(API_Server, orderClasses[0].PRI_KEY);
                    suspiciousRxLogClass suspiciousRxLogClasses = new suspiciousRxLogClass();

                    if (suspiciousRxLoges.Count == 0)
                    {
                        List<string> disease_list = new List<string>();
                        if (ICD1.StringIsEmpty() == false) disease_list.Add(ICD1);
                        if (ICD2.StringIsEmpty() == false) disease_list.Add(ICD2);
                        if (ICD3.StringIsEmpty() == false) disease_list.Add(ICD3);
                        List<diseaseClass> diseaseClasses = diseaseClass.get_by_ICD(API_Server, disease_list);

                        suspiciousRxLogClasses = new suspiciousRxLogClass()
                        {
                            GUID = Guid.NewGuid().ToString(),
                            藥袋條碼 = orderClasses[0].藥袋條碼,
                            加入時間 = DateTime.Now.ToDateTimeString(),
                            病歷號 = orderClasses[0].病歷號,
                            科別 = orderClasses[0].科別,
                            醫生姓名 = orderClasses[0].醫師代碼,
                            開方時間 = orderClasses[0].開方日期,
                            藥袋類型 = orderClasses[0].藥袋類型,
                            //錯誤類別 = string.Join(",", suspiciousRxLog.error_type),
                            //簡述事件 = suspiciousRxLog.response,
                            狀態 = enum_suspiciousRxLog_status.未辨識.GetEnumName(),
                            調劑人員 = orderClasses[0].藥師姓名,
                            調劑時間 = DateTime.Now.ToDateTimeString(),
                            //提報等級 = enum_suspiciousRxLog_ReportLevel.Normal.GetEnumName(),
                            提報時間 = DateTime.MinValue.ToDateTimeString(),
                            處理時間 = DateTime.MinValue.ToDateTimeString(),
                            diseaseClasses = diseaseClasses
                        };
                        suspiciousRxLogClass.add(API_Server, suspiciousRxLogClasses);
                    }
                })));
                Task.WhenAll(tasks).Wait();
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
