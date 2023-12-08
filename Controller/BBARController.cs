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
using HIS_DB_Lib;
namespace DB2VM
{
    [Route("dbvm/[controller]")]
    [ApiController]
    public class BBARController : ControllerBase
    {

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
        public string Get(string? BarCode, string? test, string? MRN)
        {
            string conn_str = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.24.211)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=SISDCP)));User ID=tphphaadc;Password=tph@phaadc2860;";
            OracleConnection conn_oracle;
            OracleDataReader reader;
            OracleCommand cmd;
            try
            {
                try
                {
                    conn_oracle = new OracleConnection(conn_str);
                    conn_oracle.Open();
                }
                catch
                {
                    return "HIS系統連結失敗!";
                }
                string PAC_ORDERSEQ = "";
                returnData returnData = new returnData();
                MyTimerBasic myTimerBasic = new MyTimerBasic();
                string commandText = "";
                List<OrderClass> orderClasses = new List<OrderClass>();
                List<object[]> list_value_Add = new List<object[]>();
                List<object[]> list_value_replace = new List<object[]>();
                string[] strArray_Barcode = new string[0];
                if (!BarCode.StringIsEmpty())
                {
                    strArray_Barcode = BarCode.Split(";");
                 
                }
                
                if (!MRN.StringIsEmpty())
                {
                    if (!BarCode.StringIsEmpty())
                    {
                        MRN = BarCode;
                    }
                    if (MRN.Length < 10) MRN = "0" + MRN;
                    if (MRN.Length < 10) MRN = "0" + MRN;
                    if (MRN.Length < 10) MRN = "0" + MRN;
                    if (MRN.Length < 10) MRN = "0" + MRN;
                    if (MRN.Length < 10) MRN = "0" + MRN;
                    if (MRN.Length < 10) MRN = "0" + MRN;
                    if (MRN.Length < 10) MRN = "0" + MRN;
                    if (MRN.Length < 10) MRN = "0" + MRN;
                    if (MRN.Length < 10) MRN = "0" + MRN;
                    if (MRN.Length < 10) MRN = "0" + MRN;
                    if (MRN.Length < 10) MRN = "0" + MRN;
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
                    commandText += "PAC_PROCDTTM ";

                    commandText += $"from PHAADC where PAC_PATID='{MRN}' ";
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
                    commandText += "PAC_PROCDTTM ";



                }
                else if (strArray_Barcode.Length == 4)
                {
                    string PAC_SEQ = strArray_Barcode[0];
                    string PAC_VISITDT = strArray_Barcode[1];
                    string PAC_DIACODE = strArray_Barcode[2];
                    PAC_ORDERSEQ = strArray_Barcode[3];
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
                    commandText += "PAC_PROCDTTM ";

                    commandText += $"from PHAADC where PAC_SEQ='{PAC_SEQ}' and PAC_VISITDT='{PAC_VISITDT}' AND PAC_DIACODE='{PAC_DIACODE}' ";
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
                    commandText += "PAC_PROCDTTM ";

                }
                else if (strArray_Barcode.Length == 3)
                {
                    //住院首日或住院ST藥袋
                    if (strArray_Barcode[0].Length == 25)
                    {
                        string 住院序號 = strArray_Barcode[0].Substring(0, 10);
                        string 醫令時間 = strArray_Barcode[0].Substring(10, 14);
                        string 醫令類型 = strArray_Barcode[0].Substring(24, 1);
                        string 藥品碼 = strArray_Barcode[2];
                        醫令類型 = (醫令類型 == "0") ? "S" : "B";
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
                        commandText += "PAC_PROCDTTM ";

                        commandText += $"from PHAADC where PAC_SEQ='{住院序號}' and PAC_PROCDTTM='{醫令時間}' AND PAC_TYPE='{醫令類型}' ";
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
                        commandText += "PAC_PROCDTTM ";

                    }

                }
                else if (BarCode.Length == 25)
                {
                    string 住院序號 = BarCode.Substring(0, 10);
                    string 醫令時間 = BarCode.Substring(10, 14);
                    string 醫令類型 = BarCode.Substring(24, 1);
                    醫令類型 = (醫令類型 == "0") ? "S" : "B";
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
                    commandText += "PAC_PROCDTTM ";

                    commandText += $"from PHAADC where PAC_SEQ='{住院序號}' and PAC_PROCDTTM='{醫令時間}' AND PAC_TYPE='{醫令類型}' ";
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
                    commandText += "PAC_PROCDTTM ";



                }
                else if (strArray_Barcode.Length >= 9)
                {
                    string 本次領藥號 = strArray_Barcode[(int)enum_急診藥袋.本次領藥號];
                    string 看診日期 = strArray_Barcode[(int)enum_急診藥袋.看診日期];
                    string _病歷號 = strArray_Barcode[(int)enum_急診藥袋.病歷號];
                    string 序號 = strArray_Barcode[(int)enum_急診藥袋.序號];

                    commandText = $"select * from phaadc where PAC_DRUGNO={本次領藥號} and PAC_VISITDT={看診日期} and PAC_PATID={_病歷號} and PAC_SEQ={序號}";
                }
                //string _病歷號_ = "0000681203";
                //string _看診日期 = "20230608";
                //string _CODE = "IMOR";
                //commandText = $"select * from phaadc where PAC_PATID={_病歷號_}";
                //////1120003290202303241711370;8287;IRI
                //if (commandText.StringIsEmpty()) return "Barcode type error!";
                //xstring jsonString = "";
                cmd = new OracleCommand(commandText, conn_oracle);
                try
                {
                    reader = cmd.ExecuteReader();


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

                            orderClass.PRI_KEY = $"{reader["PAC_ORDERSEQ"].ToString().Trim()}-{reader["PAC_DRUGNO"].ToString().Trim()}";
                            orderClass.藥袋條碼 = $"{reader["PAC_VISITDT"].ToString().Trim()}{reader["PAC_PATID"].ToString().Trim()}{reader["PAC_SEQ"].ToString().Trim()}";
                            orderClass.藥品碼 = reader["PAC_DIACODE"].ToString().Trim();
                            orderClass.藥品名稱 = reader["PAC_DIANAME"].ToString().Trim();
                            orderClass.病人姓名 = reader["PAC_PATNAME"].ToString().Trim();
                            orderClass.病歷號 = reader["PAC_PATID"].ToString().Trim();
                            orderClass.領藥號 = reader["PAC_DRUGNO"].ToString().Trim();
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

                        }
                    }
                    catch
                    {
                        return "HIS系統回傳資料異常!";
                    }
                }
                catch
                {
                    return "HIS系統命令下達失敗!";
                }
                conn_oracle.Close();
                conn_oracle.Dispose();

                if (orderClasses.Count == 0)
                {
                    returnData.Code = -200;
                    returnData.TimeTaken = myTimerBasic.ToString();
                    returnData.Result = $"無此藥袋資料!";

                    return returnData.JsonSerializationt(true);
                }

                List<List<OrderClass>> list_orderclasses = GroupOrders(orderClasses);
                for (int i = 0; i < list_orderclasses.Count; i++)
                {
                    double Truncate;
                    List<OrderClass> temp_orderclasses = list_orderclasses[i];
                    double 總量 = 0.0D;
                    if(temp_orderclasses[0].藥品碼 == "OMOD")
                    {

                    }
                    for (int k = 0; k < temp_orderclasses.Count; k++)
                    {
                        總量 += temp_orderclasses[k].交易量.StringToDouble();
                    }
                    Truncate = 總量 - Math.Truncate(總量);
                    if (Truncate != 0) 總量 = (int)總量 - 1;
                    for (int k = 0; k < temp_orderclasses.Count; k++)
                    {
                        double 交易量 = temp_orderclasses[k].交易量.StringToDouble();
                        Truncate = 交易量 - Math.Truncate(交易量);
                        if (Truncate != 0) 交易量 = (int)交易量 - 1;
                        if(總量 - 交易量 <= 0)
                        {
                            temp_orderclasses[k].交易量 = 交易量.ToString();
                        }
                        else
                        {
                            temp_orderclasses[k].交易量 = 總量.ToString();
                        }
                        總量 = 總量 - 交易量;
                    }
                }
                string 病歷號 = orderClasses[0].病歷號;
                List<object[]> list_value = this.sQLControl_醫囑資料.GetRowsByDefult(null, enum_醫囑資料.病歷號.GetEnumName(), 病歷號);
                List<object[]> list_value_buf = new List<object[]>();
                for (int i = 0; i < orderClasses.Count; i++)
                {
                    list_value_buf = list_value.GetRows((int)enum_醫囑資料.PRI_KEY, orderClasses[i].PRI_KEY);
                    if (list_value_buf.Count == 0)
                    {
                        object[] value = new object[new enum_醫囑資料().GetLength()];
                        value[(int)enum_醫囑資料.GUID] = Guid.NewGuid().ToString();
                        orderClasses[i].GUID = value[(int)enum_醫囑資料.GUID].ObjectToString();
                        orderClasses[i].狀態 = value[(int)enum_醫囑資料.狀態].ObjectToString();
                        value[(int)enum_醫囑資料.PRI_KEY] = orderClasses[i].PRI_KEY;
                        value[(int)enum_醫囑資料.藥局代碼] = orderClasses[i].藥局代碼;
                        value[(int)enum_醫囑資料.領藥號] = orderClasses[i].領藥號;
                        value[(int)enum_醫囑資料.藥品碼] = orderClasses[i].藥品碼;
                        value[(int)enum_醫囑資料.藥品名稱] = orderClasses[i].藥品名稱;
                        value[(int)enum_醫囑資料.病歷號] = orderClasses[i].病歷號;
                        value[(int)enum_醫囑資料.藥袋條碼] = orderClasses[i].藥袋條碼;
                        value[(int)enum_醫囑資料.病人姓名] = orderClasses[i].病人姓名;
                        value[(int)enum_醫囑資料.交易量] = orderClasses[i].交易量;
                        value[(int)enum_醫囑資料.開方日期] = orderClasses[i].開方日期;
                        value[(int)enum_醫囑資料.產出時間] = DateTime.Now.ToDateTimeString_6();
                        value[(int)enum_醫囑資料.過帳時間] = DateTime.MinValue.ToDateTimeString_6();
                        value[(int)enum_醫囑資料.狀態] = "未過帳";
                        list_value_Add.Add(value);
                    }
                    else
                    {
                        object[] value = list_value_buf[0];
                        orderClasses[i].GUID = value[(int)enum_醫囑資料.GUID].ObjectToString();
                        orderClasses[i].狀態 = value[(int)enum_醫囑資料.狀態].ObjectToString();
                        object[] value_src = orderClasses[i].ClassToSQL<OrderClass, enum_醫囑資料>();
                        bool flag_replace = false;
                        string src_開方日期 = value_src[(int)enum_醫囑資料.開方日期].ToDateTimeString();
                        if (src_開方日期.StringIsEmpty()) src_開方日期 = value_src[(int)enum_醫囑資料.開方日期].ObjectToString();
                        string dst_開方日期 = value[(int)enum_醫囑資料.開方日期].ToDateTimeString();
                        if (dst_開方日期.StringIsEmpty()) dst_開方日期 = value[(int)enum_醫囑資料.開方日期].ObjectToString();
                        if (src_開方日期 != dst_開方日期)
                        {
                            flag_replace = true;
                        }

                        if (flag_replace)
                        {

                            value[(int)enum_醫囑資料.PRI_KEY] = orderClasses[i].PRI_KEY;
                            value[(int)enum_醫囑資料.藥局代碼] = orderClasses[i].藥局代碼;
                            value[(int)enum_醫囑資料.領藥號] = orderClasses[i].領藥號;
                            value[(int)enum_醫囑資料.藥品碼] = orderClasses[i].藥品碼;
                            value[(int)enum_醫囑資料.藥品名稱] = orderClasses[i].藥品名稱;
                            value[(int)enum_醫囑資料.病歷號] = orderClasses[i].病歷號;
                            value[(int)enum_醫囑資料.藥袋條碼] = orderClasses[i].藥袋條碼;
                            value[(int)enum_醫囑資料.病人姓名] = orderClasses[i].病人姓名;
                            value[(int)enum_醫囑資料.交易量] = orderClasses[i].交易量;
                            value[(int)enum_醫囑資料.開方日期] = orderClasses[i].開方日期;
                            value[(int)enum_醫囑資料.狀態] = "未過帳";
                            orderClasses[i].狀態 = "未過帳";
                            list_value_replace.Add(value);
                        }



                    }
                }
                Task task = Task.Run(() =>
                {
                    if (list_value_Add.Count > 0)
                    {
                        this.sQLControl_醫囑資料.AddRows(null, list_value_Add);
                    }
                    if (list_value_replace.Count > 0)
                    {
                        this.sQLControl_醫囑資料.UpdateByDefulteExtra(null, list_value_replace);
                    }
                });

                returnData.Code = 200;
                returnData.Data = orderClasses;
                returnData.TimeTaken = myTimerBasic.ToString();
                returnData.Result = $"取得醫囑完成! 共<{orderClasses.Count}>筆 ,新增<{list_value_Add.Count}>筆,修改<{list_value_replace.Count}>筆";

                return returnData.JsonSerializationt(true);
            }
            catch
            {
                return "醫令串接異常";
            }

        }

        [Route("order_update")]
        [HttpGet]
        public string GET_order_update(DateTime datetime)
        {
            string conn_str = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.24.211)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=SISDCP)));User ID=tphphaadc;Password=tph@phaadc2860;";
            OracleConnection conn_oracle;
            OracleDataReader reader;
            OracleCommand cmd;
            try
            {
                try
                {
                    conn_oracle = new OracleConnection(conn_str);
                    conn_oracle.Open();
                }
                catch
                {
                    return "HIS系統連結失敗!";
                }
               
                returnData returnData = new returnData();
                MyTimerBasic myTimerBasic = new MyTimerBasic();
                string commandText = "";
                List<OrderClass> orderClasses = new List<OrderClass>();
                List<object[]> list_value_Add = new List<object[]>();
                List<object[]> list_value_replace = new List<object[]>();
                string[] strArray_Barcode = new string[0];


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
                commandText += "PAC_PROCDTTM ";
                
                commandText += $"from PHAADC where SUBSTR(PAC_PROCDTTM, 1, 8) = '{datetime.Year}{datetime.Month.ToString("00")}{datetime.Day.ToString("00")}' ";
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
                commandText += "PAC_PROCDTTM ";

                cmd = new OracleCommand(commandText, conn_oracle);
                try
                {
                    reader = cmd.ExecuteReader();


                    try
                    {
                        while (reader.Read())
                        {

                            OrderClass orderClass = new OrderClass();

                            string Time = reader["PAC_PROCDTTM"].ToString().Trim();
                            if (Time.Length == 14)
                            {
                                string Year = Time.Substring(0, 4);
                                string Month = Time.Substring(4, 2);
                                string Day = Time.Substring(6, 2);
                                string Hour = Time.Substring(8, 2);
                                string Min = Time.Substring(10, 2);
                                string Sec = Time.Substring(12, 2);
                                if ($"{Year}/{Month}/{Day}" != $"{datetime.Year}/{datetime.Month.ToString("00")}/{datetime.Day.ToString("00")}")
                                {
                                    continue;
                                }
                                orderClass.開方日期 = $"{Year}/{Month}/{Day} {Hour}:{Min}:{Sec}";
                            }

                            string type = reader["PAC_TYPE"].ToString().Trim();
                            string PAC_ORDERSEQ = reader["PAC_ORDERSEQ"].ToString().Trim();
                       
                            if (type == "E") orderClass.藥局代碼 = "PHER";
                            if (type == "S") orderClass.藥局代碼 = "STAT";
                            if (type == "B") orderClass.藥局代碼 = "首日量";
                            if (type == "O") orderClass.藥局代碼 = "OPD";
                            if (type == "M") orderClass.藥局代碼 = "出院帶藥";

                            //orderClass. = reader["PAC_SEQ"].ToString().Trim();
                            orderClass.PRI_KEY = $"{reader["PAC_ORDERSEQ"].ToString().Trim()}-{reader["PAC_DRUGNO"].ToString().Trim()}";
                            orderClass.藥袋條碼 = $"{reader["PAC_VISITDT"].ToString().Trim()}{reader["PAC_PATID"].ToString().Trim()}{reader["PAC_SEQ"].ToString().Trim()}";
                            orderClass.藥品碼 = reader["PAC_DIACODE"].ToString().Trim();
                            orderClass.藥品名稱 = reader["PAC_DIANAME"].ToString().Trim();
                            orderClass.病人姓名 = reader["PAC_PATNAME"].ToString().Trim();
                            orderClass.病歷號 = reader["PAC_PATID"].ToString().Trim();
                            orderClass.領藥號 = reader["PAC_DRUGNO"].ToString().Trim();
                            string PAC_QTYPERTIME = reader["PAC_QTYPERTIME"].ToString().Trim();
                            string PAC_SUMQTY = reader["PAC_SUMQTY"].ToString().Trim();
                            double sumQTY = PAC_SUMQTY.StringToDouble();
                            //sumQTY = Math.Ceiling(sumQTY);
                            orderClass.交易量 = (sumQTY * -1).ToString();

                            orderClasses.Add(orderClass);


                        }
                    }
                    catch
                    {
                        return "HIS系統回傳資料異常!";
                    }
                }
                catch
                {
                    return "HIS系統命令下達失敗!";
                }
                conn_oracle.Close();
                conn_oracle.Dispose();

              

                List<List<OrderClass>> list_orderclasses = GroupOrders(orderClasses);
                for (int i = 0; i < list_orderclasses.Count; i++)
                {
                    double Truncate;
                    List<OrderClass> temp_orderclasses = list_orderclasses[i];
                    double 總量 = 0.0D;
                    if (temp_orderclasses[0].藥品碼 == "OMOD")
                    {

                    }
                    for (int k = 0; k < temp_orderclasses.Count; k++)
                    {
                        總量 += temp_orderclasses[k].交易量.StringToDouble();
                    }
                    Truncate = 總量 - Math.Truncate(總量);
                    if (Truncate != 0) 總量 = (int)總量 - 1;
                    for (int k = 0; k < temp_orderclasses.Count; k++)
                    {
                        double 交易量 = temp_orderclasses[k].交易量.StringToDouble();
                        Truncate = 交易量 - Math.Truncate(交易量);
                        if (Truncate != 0) 交易量 = (int)交易量 - 1;
                        if (總量 - 交易量 <= 0)
                        {
                            temp_orderclasses[k].交易量 = 交易量.ToString();
                        }
                        else
                        {
                            temp_orderclasses[k].交易量 = 總量.ToString();
                        }
                        總量 = 總量 - 交易量;
                    }
                }
                string 病歷號 = orderClasses[0].病歷號;
                DateTime datetime_st = new DateTime(datetime.Year, datetime.Month, datetime.Day, 00, 00, 00);
                DateTime datetime_end = new DateTime(datetime.Year, datetime.Month, datetime.Day, 23, 59, 59);
                List<object[]> list_value = this.sQLControl_醫囑資料.GetRowsByBetween(null, enum_醫囑資料.開方日期.GetEnumName(), datetime_st.ToDateTimeString(), datetime_end.ToDateTimeString());

                List<object[]> list_value_buf = new List<object[]>();
                for (int i = 0; i < orderClasses.Count; i++)
                {
                    list_value_buf = list_value.GetRows((int)enum_醫囑資料.PRI_KEY, orderClasses[i].PRI_KEY);

                    if (list_value_buf.Count == 0)
                    {
                        object[] value = new object[new enum_醫囑資料().GetLength()];
                        value[(int)enum_醫囑資料.GUID] = Guid.NewGuid().ToString();
                        orderClasses[i].GUID = value[(int)enum_醫囑資料.GUID].ObjectToString();
                        orderClasses[i].狀態 = value[(int)enum_醫囑資料.狀態].ObjectToString();
                        value[(int)enum_醫囑資料.PRI_KEY] = orderClasses[i].PRI_KEY;
                        value[(int)enum_醫囑資料.藥局代碼] = orderClasses[i].藥局代碼;
                        value[(int)enum_醫囑資料.領藥號] = orderClasses[i].領藥號;
                        value[(int)enum_醫囑資料.藥品碼] = orderClasses[i].藥品碼;
                        value[(int)enum_醫囑資料.藥品名稱] = orderClasses[i].藥品名稱;
                        value[(int)enum_醫囑資料.病歷號] = orderClasses[i].病歷號;
                        value[(int)enum_醫囑資料.藥袋條碼] = orderClasses[i].藥袋條碼;
                        value[(int)enum_醫囑資料.病人姓名] = orderClasses[i].病人姓名;
                        value[(int)enum_醫囑資料.交易量] = orderClasses[i].交易量;
                        value[(int)enum_醫囑資料.開方日期] = orderClasses[i].開方日期;
                        value[(int)enum_醫囑資料.產出時間] = DateTime.Now.ToDateTimeString_6();
                        value[(int)enum_醫囑資料.過帳時間] = DateTime.MinValue.ToDateTimeString_6();
                        value[(int)enum_醫囑資料.狀態] = "未過帳";
                        list_value_Add.Add(value);
                    }
                    else
                    {
                        object[] value = list_value_buf[0];
                        orderClasses[i].GUID = value[(int)enum_醫囑資料.GUID].ObjectToString();
                        orderClasses[i].狀態 = value[(int)enum_醫囑資料.狀態].ObjectToString();
                        object[] value_src = orderClasses[i].ClassToSQL<OrderClass, enum_醫囑資料>();
                        bool flag_replace = false;
                        if (value_src[(int)enum_醫囑資料.交易量].ObjectToString() != value[(int)enum_醫囑資料.交易量].ObjectToString())
                        {
                            flag_replace = true;
                        }
                        string src_開方日期 = value_src[(int)enum_醫囑資料.開方日期].ToDateTimeString();
                        if (src_開方日期.StringIsEmpty()) src_開方日期 = value_src[(int)enum_醫囑資料.開方日期].ObjectToString();
                        string dst_開方日期 = value[(int)enum_醫囑資料.開方日期].ToDateTimeString();
                        if (dst_開方日期.StringIsEmpty()) dst_開方日期 = value[(int)enum_醫囑資料.開方日期].ObjectToString();
                        if (src_開方日期 != dst_開方日期)
                        {
                            flag_replace = true;
                        }
                        if (flag_replace)
                        {

                            value[(int)enum_醫囑資料.PRI_KEY] = orderClasses[i].PRI_KEY;
                            value[(int)enum_醫囑資料.藥局代碼] = orderClasses[i].藥局代碼;
                            value[(int)enum_醫囑資料.領藥號] = orderClasses[i].領藥號;
                            value[(int)enum_醫囑資料.藥品碼] = orderClasses[i].藥品碼;
                            value[(int)enum_醫囑資料.藥品名稱] = orderClasses[i].藥品名稱;
                            value[(int)enum_醫囑資料.病歷號] = orderClasses[i].病歷號;
                            value[(int)enum_醫囑資料.藥袋條碼] = orderClasses[i].藥袋條碼;
                            value[(int)enum_醫囑資料.病人姓名] = orderClasses[i].病人姓名;
                            value[(int)enum_醫囑資料.交易量] = orderClasses[i].交易量;
                            value[(int)enum_醫囑資料.開方日期] = orderClasses[i].開方日期;
                            value[(int)enum_醫囑資料.狀態] = "未過帳";
                            orderClasses[i].狀態 = "未過帳";
                            list_value_replace.Add(value);
                        }



                    }
                }
                Task task = Task.Run(() =>
                {
                    if (list_value_Add.Count > 0)
                    {
                        this.sQLControl_醫囑資料.AddRows(null, list_value_Add);
                    }
                    if (list_value_replace.Count > 0)
                    {
                        this.sQLControl_醫囑資料.UpdateByDefulteExtra(null, list_value_replace);
                    }
                });
                task.Wait();
                returnData.Code = 200;
                returnData.Data = "";
                returnData.TimeTaken = myTimerBasic.ToString();
                returnData.Result = $"取得醫囑完成! 共<{orderClasses.Count}>筆 ,新增<{list_value_Add.Count}>筆,修改<{list_value_replace.Count}>筆";

                return returnData.JsonSerializationt(true);
            }
            catch(Exception e)
            {
                return $"醫令串接異常,msg:{e.Message}";
            }
        }
        public static List<List<OrderClass>> GroupOrders(List<OrderClass> orders)
        {
            List<List<OrderClass>> groupedOrders = orders
                .GroupBy(o => new { o.藥品碼, o.病歷號, o.開方日期 })
                .Select(group => group.ToList())
                .ToList();

            return groupedOrders;
        }
    }

}
