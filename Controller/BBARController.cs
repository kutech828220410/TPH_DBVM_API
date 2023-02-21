using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IBM.Data.DB2.Core;
using System.Data;
using System.Configuration;
using Basic;
using SQLUI;
using Oracle.ManagedDataAccess.Client;
namespace DB2VM
{
    [Route("dbvm/[controller]")]
    [ApiController]
    public class BBARController : ControllerBase
    {
        public enum enum_醫囑資料
        {
            GUID,
            PRI_KEY,
            藥局代碼,
            藥袋條碼,
            藥品碼,
            藥品名稱,
            病人姓名,
            病歷號,
            交易量,
            開方日期,
            產出時間,
            過帳時間,
            狀態,
        }
        public enum enum_急診藥袋
        {
            本次領藥號,
            看診日期,
            病歷號,
            序號,
            頻率,
            途徑,
            總量,
            前次領藥號,
            本次醫令序號,
        }





        static string MySQL_server = $"{ConfigurationManager.AppSettings["MySQL_server"]}";
        static string MySQL_database = $"{ConfigurationManager.AppSettings["MySQL_database"]}";
        static string MySQL_userid = $"{ConfigurationManager.AppSettings["MySQL_user"]}";
        static string MySQL_password = $"{ConfigurationManager.AppSettings["MySQL_password"]}";
        static string MySQL_port = $"{ConfigurationManager.AppSettings["MySQL_port"]}";

        private SQLControl sQLControl_UDSDBBCM = new SQLControl(MySQL_server, MySQL_database, "UDSDBBCM", MySQL_userid, MySQL_password, (uint)MySQL_port.StringToInt32(), MySql.Data.MySqlClient.MySqlSslMode.None);
        private SQLControl sQLControl_醫囑資料 = new SQLControl(MySQL_server, MySQL_database, "order_list", MySQL_userid, MySQL_password, (uint)MySQL_port.StringToInt32(), MySql.Data.MySqlClient.MySqlSslMode.None);

        [HttpGet]
        public string Get(string? BarCode)
        {
            string conn_str = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.24.211)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=SISDCP)));User ID=tphphaadc;Password=tph@phaadc2860;";
            try
            {
                OracleConnection conn_oracle = new OracleConnection(conn_str);
                conn_oracle.Open();
                string jsonstring = "";
                string commandText = "";
                List<OrderClass> orderClasses = new List<OrderClass>();
                List<object[]> list_value_Add = new List<object[]>();
                if (BarCode.Length == 25)
                {
                    //住院首日或住院ST藥袋
                    string 住院序號 = BarCode.Substring(0, 10);
                    string 醫令時間 = BarCode.Substring(10, 14);
                    string 醫令類型 = BarCode.Substring(24, 1);
                    醫令類型 = (醫令類型 == "0") ? "S" : "B";
                    commandText = $"select * from PHAADC where PAC_SEQ='{住院序號}'  and PAC_PROCDTTM='{醫令時間}' AND PAC_TYPE='{醫令類型}'";                   
                }
                else
                {
                    string[] strArray_Barcode = BarCode.Split(";");
                    if(strArray_Barcode.Length >= 9)
                    {
                        string 本次領藥號 = strArray_Barcode[(int)enum_急診藥袋.本次領藥號];
                        string 看診日期 = strArray_Barcode[(int)enum_急診藥袋.看診日期];
                        string 病歷號 = strArray_Barcode[(int)enum_急診藥袋.病歷號];
                        string 序號 = strArray_Barcode[(int)enum_急診藥袋.序號];

                        commandText = $"select * from phaadc where PAC_DRUGNO={本次領藥號} and PAC_VISITDT={看診日期} and PAC_PATID={病歷號} and PAC_SEQ={序號}";
                    }
                }
                if (commandText.StringIsEmpty()) return "Barcode type error!";
                //xstring jsonString = "";
                OracleCommand cmd = new OracleCommand(commandText, conn_oracle);

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    OrderClass orderClass = new OrderClass();
                    orderClass.PRI_KEY = reader["PAC_ORDERSEQ"].ToString().Trim();
                    orderClass.藥局代碼 = "PHER";
                    orderClass.處方序號 = reader["PAC_SEQ"].ToString().Trim();
                    orderClass.藥袋條碼 = $"{reader["PAC_VISITDT"].ToString().Trim()}{reader["PAC_PATID"].ToString().Trim()}{reader["PAC_SEQ"].ToString().Trim()}";
                    orderClass.藥品碼 = reader["PAC_DIACODE"].ToString().Trim();
                    orderClass.藥品名稱 = reader["PAC_DIANAME"].ToString().Trim();
                    orderClass.病人姓名 = reader["PAC_PATNAME"].ToString().Trim();
                    orderClass.病歷號 = reader["PAC_PATID"].ToString().Trim();
                    orderClass.包裝單位 = reader["PAC_UNIT"].ToString().Trim();
                    orderClass.劑量 = reader["PAC_QTYPERTIME"].ToString().Trim();
                    orderClass.頻次 = reader["PAC_FEQNO"].ToString().Trim();
                    orderClass.途徑 = reader["PAC_PATHNO"].ToString().Trim();
                    orderClass.天數 = reader["PAC_DAYS"].ToString().Trim();
                    orderClass.交易量 = (reader["PAC_SUMQTY"].ToString().Trim().StringToInt32() * -1).ToString();
                    string Time = reader["PAC_PROCDTTM"].ToString().Trim();
                    if (Time.Length == 14)
                    {
                        string Year = Time.Substring(0, 4);
                        string Month = Time.Substring(4, 2);
                        string Day = Time.Substring(6, 2);
                        string Hour = Time.Substring(8, 2);
                        string Min = Time.Substring(10, 2);
                        string Sec = Time.Substring(12, 2);
                        orderClass.開方時間 = $"{Year}/{Month}/{Day} {Hour}:{Min}:{Sec}";
                    }

                    orderClasses.Add(orderClass);

                }
                conn_oracle.Close();
                conn_oracle.Dispose();

                for (int i = 0; i < orderClasses.Count; i++)
                {
                    List<object[]> list_value = this.sQLControl_醫囑資料.GetRowsByDefult(null, enum_醫囑資料.PRI_KEY.GetEnumName(), orderClasses[i].PRI_KEY);
                    if (list_value.Count == 0)
                    {
                        object[] value = new object[new enum_醫囑資料().GetLength()];
                        value[(int)enum_醫囑資料.GUID] = Guid.NewGuid().ToString();
                        value[(int)enum_醫囑資料.PRI_KEY] = orderClasses[i].PRI_KEY;
                        value[(int)enum_醫囑資料.藥局代碼] = orderClasses[i].藥局代碼;
                        value[(int)enum_醫囑資料.藥品碼] = orderClasses[i].藥品碼;
                        value[(int)enum_醫囑資料.藥品名稱] = orderClasses[i].藥品名稱;
                        value[(int)enum_醫囑資料.病歷號] = orderClasses[i].病歷號;
                        value[(int)enum_醫囑資料.藥袋條碼] = orderClasses[i].藥袋條碼;
                        value[(int)enum_醫囑資料.病人姓名] = orderClasses[i].病人姓名;
                        value[(int)enum_醫囑資料.交易量] = orderClasses[i].交易量;
                        value[(int)enum_醫囑資料.開方日期] = orderClasses[i].開方時間;
                        value[(int)enum_醫囑資料.產出時間] = DateTime.Now.ToDateTimeString_6();
                        value[(int)enum_醫囑資料.過帳時間] = DateTime.MinValue.ToDateTimeString_6();
                        value[(int)enum_醫囑資料.狀態] = "未過帳";
                        list_value_Add.Add(value);
                    }
                }

                if (list_value_Add.Count > 0)
                {
                    this.sQLControl_醫囑資料.AddRows(null, list_value_Add);
                }
                return orderClasses.JsonSerializationt();
            }
            catch
            {
                return "Database connecting error!";
            }
           
        }
    }
}
