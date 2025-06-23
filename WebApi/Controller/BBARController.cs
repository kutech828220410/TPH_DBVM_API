using Microsoft.AspNetCore.Mvc;
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
using System.Text.Json.Serialization;
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
        private static readonly string conn_str =
            "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.24.211)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=SISDCP)));"
            + "User ID=tphphaadc;"
            + "Password=tph@phaadc2860;"
            + "Pooling=true;Min Pool Size=5;Max Pool Size=80;Connection Lifetime=600;Connection Timeout=10;";
        static string MySQL_server = $"{ConfigurationManager.AppSettings["MySQL_server"]}";
        static string MySQL_database = $"{ConfigurationManager.AppSettings["MySQL_database"]}";
        static string MySQL_userid = $"{ConfigurationManager.AppSettings["MySQL_user"]}";
        static string MySQL_password = $"{ConfigurationManager.AppSettings["MySQL_password"]}";
        static string MySQL_port = $"{ConfigurationManager.AppSettings["MySQL_port"]}";

        private SQLControl sQLControl_UDSDBBCM = new SQLControl(MySQL_server, MySQL_database, "UDSDBBCM", MySQL_userid, MySQL_password, (uint)MySQL_port.StringToInt32(), MySql.Data.MySqlClient.MySqlSslMode.None);
        private SQLControl sQLControl_醫囑資料 = new SQLControl(MySQL_server, MySQL_database, "order_list", MySQL_userid, MySQL_password, (uint)MySQL_port.StringToInt32(), MySql.Data.MySqlClient.MySqlSslMode.None);
        private string API_Server = "http://192.168.23.54:4433";
        [HttpGet]
        public string Get(string? BarCode, string? test, string? MRN)
        {
            MyTimerBasic myTimer_total = new MyTimerBasic();
            if(BarCode == "66437068" || BarCode == "123456789" || BarCode == "987654321" || BarCode == "19961029")
            {
                returnData returnData = new returnData();
                List<OrderClass> orderClasses = OrderClass.get_by_barcode(API_Server, BarCode);
                foreach (var item in orderClasses)
                {
                    item.開方日期 = DateTime.Now.ToDateTimeString();
                }
                returnData.Code = 200;
                returnData.Data = orderClasses;
                returnData.Result = $"取得醫囑完成! 共<{orderClasses.Count}>筆 ";
                return returnData.JsonSerializationt(true);
            }
            
            bool flag_術中醫令 = false;
            OracleConnection conn_oracle;
            OracleDataReader reader;
            OracleCommand cmd;
            List<string> list_ICD = new List<string>();
            string 性別 = "";
            string 年齡 = "";
            //string ICD3 = "";
            try
            {
                MyTimerBasic myTimer_HISconnect = new MyTimerBasic();
                string HIS連線時間 = string.Empty;
                try
                {
                    conn_oracle = new OracleConnection(conn_str);
                    conn_oracle.Open();
                    HIS連線時間 = myTimer_HISconnect.ToString();
                }
                catch
                {
                    return "HIS系統連結失敗!";
                }
                MyTimerBasic myTimer_HISData = new MyTimerBasic();

                string PAC_ORDERSEQ = "";
                returnData returnData = new returnData();
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
                    //commandText += "PAC_SEX,"; //性別 
                    //commandText += "PAC_AGE,"; //年齡 
                    commandText += "PAC_DRUGGIST "; //藥師代碼

                    commandText += $"from PHAADCAL where PAC_PATID='{MRN}' ";
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
                    //commandText += "PAC_SEX,"; //性別 
                    //commandText += "PAC_AGE,"; //年齡 
                    commandText += "PAC_DRUGGIST "; //藥師代碼

                }
                else if (strArray_Barcode.Length == 5)
                {
                    string PAC_SEQ = strArray_Barcode[0];
                    string PAC_VISITDT = strArray_Barcode[1];
                    string PAC_DIACODE = strArray_Barcode[2];
                    PAC_ORDERSEQ = strArray_Barcode[3];
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
                    //commandText += "PAC_SEX,"; //性別 
                    //commandText += "PAC_AGE,"; //年齡 
                    commandText += "PAC_DRUGGIST "; //藥師代碼


                    commandText += $"from phaadcal where PAC_SEQ='{PAC_SEQ}' and PAC_VISITDT='{PAC_VISITDT}' AND PAC_DIACODE='{PAC_DIACODE}' AND PAC_ORDERSEQ='{PAC_ORDERSEQ}' ";
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
                    //commandText += "PAC_SEX,"; //性別 
                    //commandText += "PAC_AGE,"; //年齡 
                    commandText += "PAC_DRUGGIST "; //藥師代碼

                }
                else if (strArray_Barcode.Length == 4)
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
                        commandText += "PAC_PROCDTTM, "; //醫令開立時間
                        commandText += "PAC_PAYCD, "; //費用別
                        commandText += "PAC_ICDX1,"; //診斷碼1
                        commandText += "PAC_ICDX2,"; //診斷碼2
                        commandText += "PAC_ICDX3,"; //診斷碼3
                        //commandText += "PAC_SEX,"; //性別 
                        //commandText += "PAC_AGE,"; //年齡 
                        commandText += "PAC_DRUGGIST "; //藥師代碼

                        commandText += $"from  phaadcal  where PAC_SEQ='{住院序號}' and PAC_PROCDTTM='{醫令時間}' AND PAC_TYPE='{醫令類型}' ";
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
                        commandText += "PAC_PROCDTTM, "; //醫令開立時間
                        commandText += "PAC_PAYCD, "; //費用別
                        commandText += "PAC_ICDX1,"; //診斷碼1
                        commandText += "PAC_ICDX2,"; //診斷碼2
                        commandText += "PAC_ICDX3,"; //診斷碼3
                        //commandText += "PAC_SEX,"; //性別 
                        //commandText += "PAC_AGE,"; //年齡 
                        commandText += "PAC_DRUGGIST "; //藥師代碼

                    }
                    else
                    {
                        if(strArray_Barcode[2].Length == 10)
                        {
                            string PAC_DRUGNO = strArray_Barcode[0];
                            string PAC_VISITDT = strArray_Barcode[1];
                            string PAC_PATID = strArray_Barcode[2];
                            string PAC_SEQ = strArray_Barcode[3];
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
                            //commandText += "PAC_SEX,"; //性別 
                            //commandText += "PAC_AGE,"; //年齡 
                            commandText += "PAC_DRUGGIST "; //藥師代碼

                            commandText += $"from phaadcal where PAC_DRUGNO='{PAC_DRUGNO}' and PAC_VISITDT='{PAC_VISITDT}' AND PAC_PATID='{PAC_PATID}' AND PAC_SEQ='{PAC_SEQ}'  ";
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
                            //commandText += "PAC_SEX,"; //性別 
                            //commandText += "PAC_AGE,"; //年齡 
                            commandText += "PAC_DRUGGIST "; //藥師代碼

                            flag_術中醫令 = true;
                        }
                        else
                        {
                            string PAC_SEQ = strArray_Barcode[0];
                            string PAC_VISITDT = strArray_Barcode[1];
                            string PAC_DIACODE = strArray_Barcode[2];
                            PAC_ORDERSEQ = strArray_Barcode[3];
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
                            //commandText += "PAC_SEX,"; //性別 
                            //commandText += "PAC_AGE,"; //年齡 
                            commandText += "PAC_DRUGGIST "; //藥師代碼

                            commandText += $"from phaadcal where PAC_SEQ='{PAC_SEQ}' and PAC_VISITDT='{PAC_VISITDT}' AND PAC_DIACODE='{PAC_DIACODE}' AND PAC_ORDERSEQ='{PAC_ORDERSEQ}' ";
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
                            //commandText += "PAC_SEX,"; //性別 
                            //commandText += "PAC_AGE,"; //年齡 
                            commandText += "PAC_DRUGGIST "; //藥師代碼

                            flag_術中醫令 = true;
                        }
                       
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
                    //commandText += "PAC_SEX,"; //性別 
                    //commandText += "PAC_AGE,"; //年齡 
                    commandText += "PAC_DRUGGIST "; //藥師代碼

                    commandText += $"from PHAADC where PAC_SEQ='{住院序號}' and PAC_PROCDTTM='{醫令時間}' AND PAC_TYPE='{醫令類型}' ";
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
                    //commandText += "PAC_SEX,"; //性別 
                    //commandText += "PAC_AGE,"; //年齡 
                    commandText += "PAC_DRUGGIST "; //藥師代碼


                }
                else if (strArray_Barcode.Length >= 9)
                {
                    string 本次領藥號 = strArray_Barcode[(int)enum_急診藥袋.本次領藥號];
                    string 看診日期 = strArray_Barcode[(int)enum_急診藥袋.看診日期];
                    string _病歷號 = strArray_Barcode[(int)enum_急診藥袋.病歷號];
                    string 序號 = strArray_Barcode[(int)enum_急診藥袋.序號];
                    看診日期 = strArray_Barcode[(int)enum_急診藥袋.本次醫令序號].ObjectToString().Substring(0,8);
                    //commandText = $"select * from PHAADC where PAC_DRUGNO={本次領藥號} and PAC_VISITDT={看診日期} and PAC_PATID={_病歷號} and PAC_SEQ={序號}";
                    //commandText = $"select * from phaadcal where PAC_DRUGNO={本次領藥號} and PAC_PATID={_病歷號} and PAC_SEQ={序號}";

                    commandText = "";
                    //commandText = $"select * from phaadcal where PAC_DRUGNO={本次領藥號}  and PAC_PATID={_病歷號} and PAC_SEQ={序號} ";

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
                    //commandText += "PAC_SEX,"; //性別 
                    //commandText += "PAC_AGE,"; //年齡 
                    commandText += "PAC_DRUGGIST "; //藥師代碼

                    commandText += $"from phaadcal where PAC_DRUGNO={本次領藥號}  and PAC_PATID={_病歷號} and PAC_SEQ={序號} ";
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
                    //commandText += "PAC_SEX,"; //性別 
                    //commandText += "PAC_AGE,"; //年齡 
                    commandText += "PAC_DRUGGIST "; //藥師代碼


                }

                cmd = new OracleCommand(commandText, conn_oracle);
                List<object[]> list_temp = new List<object[]>();
                string queryTime = "";
                try
                {
                    MyTimerBasic myTimerBasic_query = new MyTimerBasic();

                    reader = cmd.ExecuteReader();
                    queryTime = myTimerBasic_query.ToString();
                    MyTimerBasic myTimerBasic_DB = new MyTimerBasic();
                    List<string> list_colname = new List<string>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        string colname = reader.GetName(i);
                        list_colname.Add(colname);
                    }

                    try
                    {
                        List<personPageClass> personPageClasses = personPageClass.get_all("http://192.168.23.54:4433");

                        while (reader.Read())
                        {
                            List<object> value = new List<object>();
                            for (int i = 0; i < list_colname.Count; i++)
                            {
                                value.Add(reader[list_colname[i]]);

                            }
                            list_temp.Add(value.ToArray());
                            OrderClass orderClass = new OrderClass();
                            string type = reader["PAC_TYPE"].ToString().Trim();

                            if (type == "E") orderClass.藥袋類型 = "PHER";
                            if (type == "S") orderClass.藥袋類型 = "STAT";
                            if (type == "B") orderClass.藥袋類型 = "首日量";
                            if (type == "O") orderClass.藥袋類型 = "OPD";
                            if (type == "M") orderClass.藥袋類型 = "出院帶藥";

                         

                            orderClass.PRI_KEY = $"{reader["PAC_ORDERSEQ"].ToString().Trim()}-{reader["PAC_DRUGNO"].ToString().Trim()}";
                            orderClass.藥袋條碼 = BarCode;
                            //orderClass.藥袋條碼 = $"{reader["PAC_VISITDT"].ToString().Trim()}{reader["PAC_PATID"].ToString().Trim()}{reader["PAC_SEQ"].ToString().Trim()}";
                            orderClass.住院序號 = reader["PAC_SEQ"].ToString().Trim();
                            orderClass.藥品碼 = reader["PAC_DIACODE"].ToString().Trim();
                            orderClass.藥品名稱 = reader["PAC_DIANAME"].ToString().Trim();
                            orderClass.病人姓名 = reader["PAC_PATNAME"].ToString().Trim();
                            orderClass.病歷號 = reader["PAC_PATID"].ToString().Trim();
                            orderClass.領藥號 = reader["PAC_DRUGNO"].ToString().Trim();
                            orderClass.就醫時間 = reader["PAC_VISITDT"].ToString().Trim();
                            orderClass.就醫時間 = $"{orderClass.就醫時間.Substring(0, 4)}-{orderClass.就醫時間.Substring(4, 2)}-{orderClass.就醫時間.Substring(6, 2)}";
                            orderClass.科別 = reader["PAC_SECTNAME"].ToString().Trim();
                            orderClass.醫師代碼 = reader["PAC_DOCNAME"].ToString().Trim();
                            orderClass.頻次 = reader["PAC_FEQNO"].ToString().Trim();
                            orderClass.天數 = reader["PAC_DAYS"].ToString().Trim();
                            orderClass.單次劑量 = reader["PAC_QTYPERTIME"].ToString().Trim();
                            orderClass.劑量單位 = reader["PAC_UNIT"].ToString().Trim();
                            //性別 = reader["PAC_SEX"].ToString().Trim();
                            //年齡 = reader["PAC_AGE"].ToString().Trim();
                            //ICD3 = reader["PAC_ICDX3"].ToString().Trim();





                            if (reader["PAC_PAYCD"].ToString().Trim() == "Y")
                            {
                                orderClass.費用別 = "自費";
                            }
                            else
                            {
                                orderClass.費用別 = "健保";
                            }
                            if (strArray_Barcode.Length >= 9)
                            {
                                string PAC_DRUGGIST = reader["PAC_DRUGGIST"].ToString().Trim();
                                orderClass.藥師ID = PAC_DRUGGIST.StringToInt32().ToString();
                                List<personPageClass> personPageClasses_buf = (from temp in personPageClasses
                                                                               where temp.ID == orderClass.藥師ID
                                                                               select temp).ToList();
                                if (personPageClasses_buf.Count> 0 )
                                {
                                    orderClass.藥師姓名 = personPageClasses_buf[0].姓名;
                                }
                            }
                           

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
                catch(Exception ex)
                {
                    return $"HIS系統命令下達失敗! \n {ex} \n {commandText}";
                }
                conn_oracle.Close();
                conn_oracle.Dispose();
                string HISData = myTimer_HISData.ToString();
                MyTimerBasic myTimer_DB = new MyTimerBasic();
                if (orderClasses.Count == 0)
                {
                    returnData.Code = -200;
                    returnData.TimeTaken = myTimer_total.ToString();
                    returnData.Result = $"無此藥袋資料!";

                    return returnData.JsonSerializationt(true);
                }
                List<Task> tasks = new List<Task>();
                tasks.Add(Task.Run(new Action(delegate
                {
                    string reponse = ICD(orderClasses);
                    returnData returnData_ICD = reponse.JsonDeserializet<returnData>();
                    list_ICD = returnData_ICD.Data.ObjToClass<List<string>>();
                })));
                List<List<OrderClass>> list_orderclasses = GroupOrders(orderClasses);
                for (int i = 0; i < list_orderclasses.Count; i++)
                {
                    double Truncate;
                    List<OrderClass> temp_orderclasses = list_orderclasses[i];
                    double 總量 = 0.0D;
                    for (int k = 0; k < temp_orderclasses.Count; k++)
                    {
                  
                        總量 += temp_orderclasses[k].交易量.StringToDouble();
   
                    }
                    Truncate = 總量 - Math.Truncate(總量);
                    if (Truncate != 0) 總量 = (int)總量 - 1;
                    bool 總量已到達 = false;
                    for (int k = 0; k < temp_orderclasses.Count; k++)
                    {
                    

                        double 交易量 = temp_orderclasses[k].交易量.StringToDouble();
                        Truncate = 交易量 - Math.Truncate(交易量);
                        if (Truncate != 0) 交易量 = (int)交易量 - 1;
                        if(總量已到達)
                        {
                            temp_orderclasses[k].交易量 = "0";
                            continue;
                        }
                        if(總量 - 交易量 <= 0)
                        {
                            temp_orderclasses[k].交易量 = 交易量.ToString();
                        }
                        else
                        {
                            temp_orderclasses[k].交易量 = 總量.ToString();
                            總量已到達 = true;
                        }
                        總量 = 總量 - 交易量;
                    }
                }
                List<KeyValuePair<string, int>> groupedByDate = orderClasses
                    .GroupBy(order => order.就醫時間)
                    .Select(group => new KeyValuePair<string, int>(group.Key, group.Count()))
                    .ToList();

                // 輸出分類結果
                Logger.Log("BBAR", $"條碼: {BarCode}");

                foreach (KeyValuePair<string, int> item in groupedByDate)
                {
                    Logger.Log("BBAR", $"日期: {item.Key}, 筆數: {item.Value}");
                }
                string 病歷號 = orderClasses[0].病歷號;
                string Today = DateTime.Now.ToString("yyyy-MM-dd");
                string tenDaysAgo = DateTime.Now.AddDays(-27).ToString("yyyy-MM-dd");
                //orderClasses = orderClasses.Where(temp => string.Compare(temp.就醫時間, tenDaysAgo) >= 0 && string.Compare(temp.就醫時間, Today) <= 0).ToList();
                //orderClasses = orderClasses.Where(temp => string.Compare(temp.就醫時間, tenDaysAgo) >= 0 ).ToList();

                //orderClasses = orderClasses.Where(temp => temp.就醫時間 == Today).ToList();
                List<object[]> list_value = this.sQLControl_醫囑資料.GetRowsByDefult(null, enum_醫囑資料.病歷號.GetEnumName(), 病歷號);
                List<object[]> list_value_buf = new List<object[]>();
                for (int i = 0; i < orderClasses.Count; i++)
                {
                    list_value_buf = (from temp in list_value
                                      where temp[(int)enum_醫囑資料.PRI_KEY].ObjectToString() == orderClasses[i].PRI_KEY
                                      select temp).ToList();
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
                        value[(int)enum_醫囑資料.住院序號] = orderClasses[i].住院序號;
                        value[(int)enum_醫囑資料.醫師代碼] = orderClasses[i].醫師代碼;
                        value[(int)enum_醫囑資料.科別] = orderClasses[i].科別;
                        value[(int)enum_醫囑資料.就醫時間] = orderClasses[i].就醫時間;
                        value[(int)enum_醫囑資料.開方日期] = orderClasses[i].開方日期;
                        value[(int)enum_醫囑資料.藥師姓名] = orderClasses[i].藥師姓名;
                        value[(int)enum_醫囑資料.頻次] = orderClasses[i].頻次;
                        value[(int)enum_醫囑資料.天數] = orderClasses[i].天數;
                        value[(int)enum_醫囑資料.藥袋類型] = orderClasses[i].藥袋類型;
                        value[(int)enum_醫囑資料.費用別] = orderClasses[i].費用別;


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
                            value[(int)enum_醫囑資料.住院序號] = orderClasses[i].住院序號;
                            value[(int)enum_醫囑資料.藥品碼] = orderClasses[i].藥品碼;
                            value[(int)enum_醫囑資料.藥品名稱] = orderClasses[i].藥品名稱;
                            value[(int)enum_醫囑資料.病歷號] = orderClasses[i].病歷號;
                            value[(int)enum_醫囑資料.藥袋條碼] = orderClasses[i].藥袋條碼;
                            value[(int)enum_醫囑資料.病人姓名] = orderClasses[i].病人姓名;
                            value[(int)enum_醫囑資料.交易量] = orderClasses[i].交易量;
                            value[(int)enum_醫囑資料.醫師代碼] = orderClasses[i].醫師代碼;
                            value[(int)enum_醫囑資料.科別] = orderClasses[i].科別;
                            value[(int)enum_醫囑資料.就醫時間] = orderClasses[i].就醫時間;
                            value[(int)enum_醫囑資料.開方日期] = orderClasses[i].開方日期;
                            value[(int)enum_醫囑資料.藥師姓名] = orderClasses[i].藥師姓名;
                            value[(int)enum_醫囑資料.頻次] = orderClasses[i].頻次;
                            value[(int)enum_醫囑資料.天數] = orderClasses[i].天數;
                            value[(int)enum_醫囑資料.藥袋類型] = orderClasses[i].藥袋類型;
                            value[(int)enum_醫囑資料.費用別] = orderClasses[i].費用別;

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
                List<string> list_診斷碼 = new List<string>();
                List<string> list_中文說明 = new List<string>();
                Task.WhenAll(tasks).Wait();
                tasks.Clear();
                suspiciousRxLogClass suspiciousRxLogClasses = new suspiciousRxLogClass();

                tasks.Add(Task.Run(new Action(delegate
                {
                    if (orderClasses.Count == 0) return;
                    List<suspiciousRxLogClass> suspiciousRxLoges = suspiciousRxLogClass.get_by_barcode(API_Server, orderClasses[0].藥袋條碼);                   

                    if (suspiciousRxLoges.Count  == 0)
                    {
                        List<string> disease_list = list_ICD;
                        //if (ICD1.StringIsEmpty() == false) disease_list.Add(ICD1);
                        //if (ICD2.StringIsEmpty() == false) disease_list.Add(ICD2);
                        //if (ICD3.StringIsEmpty() == false) disease_list.Add(ICD3);
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
                            //調劑人員 = orderClasses[0].藥師姓名,
                            調劑時間 = DateTime.Now.ToDateTimeString(),
                            //提報等級 = enum_suspiciousRxLog_ReportLevel.Normal.GetEnumName(),
                            提報時間 = DateTime.MinValue.ToDateTimeString(),
                            處理時間 = DateTime.MinValue.ToDateTimeString(),
                            性別 = 性別,
                            年齡 = 年齡,
                            diseaseClasses = diseaseClasses
                        };
                        suspiciousRxLogClass.add(API_Server, suspiciousRxLogClasses);
                    }
                })));
                tasks.Add(Task.Run(new Action(delegate
                {
                    if (orderClasses.Count == 0) return;
                    string reponse = pragnant(orderClasses[0].病歷號);
                    returnData returnData_pragnannt = reponse.JsonDeserializet<returnData>();
                    List<pragnantClass> pragnantClasses = returnData_pragnannt.Data.ObjToClass<List<pragnantClass>>();
                    if(pragnantClasses.Count > 0) 
                    {
                        foreach ( var item in orderClasses)
                        {
                            item.備註 = "懷孕";
                        }
                    }
                })));
                Task.WhenAll(tasks).Wait();

                string DBTIme = myTimer_DB.ToString();



                returnData.Code = 200;
                returnData.Data = orderClasses;
                returnData.TimeTaken = myTimer_total.ToString();
                returnData.Result = $"取得醫囑完成! 共<{orderClasses.Count}>筆 ,新增<{list_value_Add.Count}>筆,修改<{list_value_replace.Count}>筆 , HIS連線時間；{HIS連線時間}，取得HIS資料；{HISData}，DB寫入時間:{DBTIme}";
                string json_result = returnData.JsonSerializationt(true);
                Logger.Log("BBAR", json_result);
                return json_result;
            }
            catch(Exception ex)
            {
                return $"Exception : {ex.Message} ";
            }

        }
        [Route("order_controll_drug")]
        [HttpGet]
        public string order_controll_drug(string? BarCode, string? MRN)
        {
            MyTimerBasic myTimer_total = new MyTimerBasic();
            bool flag_術中醫令 = false;

            OracleConnection conn_oracle;
            OracleDataReader reader;
            OracleCommand cmd;
            string 年齡 = "";
            string 性別 = "";
            string ICD3 = "";
            List<string> list_ICD = new List<string>();
            try
            {
                MyTimerBasic myTimer_HisConnet = new MyTimerBasic();
                string HIS連線時間 = string.Empty;
                try
                {
                    conn_oracle = new OracleConnection(conn_str);
                    conn_oracle.Open();
                    HIS連線時間 = myTimer_HisConnet.ToString();
                    //Logger.Log("BBAR_control", $"與HIS建立連線");

                }
                catch
                {
                    return "HIS系統連結失敗!";
                }
                MyTimerBasic myTimer_HisData = new MyTimerBasic();
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
                    //commandText += "PAC_SEX,"; //性別 
                    //commandText += "PAC_AGE,"; //年齡 
                    commandText += "PAC_DRUGGIST "; //藥師代碼

                    commandText += $"from PHAADCAL where PAC_PATID='{MRN}' ";
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
                    //commandText += "PAC_SEX,"; //性別 
                    //commandText += "PAC_AGE,"; //年齡 
                    commandText += "PAC_DRUGGIST "; //藥師代碼



                }
                else if (strArray_Barcode.Length == 5)
                {
                    string PAC_SEQ = strArray_Barcode[0];
                    string PAC_VISITDT = strArray_Barcode[1];
                    string PAC_DIACODE = strArray_Barcode[2];
                    PAC_ORDERSEQ = strArray_Barcode[3];
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
                    //commandText += "PAC_SEX,"; //性別 
                    //commandText += "PAC_AGE,"; //年齡 
                    commandText += "PAC_DRUGGIST "; //藥師代碼

                    commandText += $"from phaadcal where PAC_SEQ='{PAC_SEQ}' and PAC_VISITDT='{PAC_VISITDT}' AND PAC_DIACODE='{PAC_DIACODE}' AND PAC_ORDERSEQ='{PAC_ORDERSEQ}' ";
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
                    //commandText += "PAC_SEX,"; //性別 
                    //commandText += "PAC_AGE,"; //年齡 
                    commandText += "PAC_DRUGGIST "; //藥師代碼

                }
                else if (strArray_Barcode.Length == 4)
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
                        //commandText += "PAC_SEX,"; //性別 
                        //commandText += "PAC_AGE,"; //年齡 
                        commandText += "PAC_DRUGGIST "; //藥師代碼

                        commandText += $"from phaadcal where PAC_SEQ='{住院序號}' and PAC_PROCDTTM='{醫令時間}' AND PAC_TYPE='{醫令類型}' ";
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
                        //commandText += "PAC_SEX,"; //性別 
                        //commandText += "PAC_AGE,"; //年齡 
                        commandText += "PAC_DRUGGIST "; //藥師代碼

                    }
                    else
                    {
                        if (strArray_Barcode[0].Length == 4)
                        {
                            string PAC_DRUGNO = strArray_Barcode[0];
                            string PAC_VISITDT = strArray_Barcode[1];
                            string PAC_PATID = strArray_Barcode[2];
                            string PAC_SEQ = strArray_Barcode[3];
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
                            //commandText += "PAC_SEX,"; //性別 
                            //commandText += "PAC_AGE,"; //年齡 
                            commandText += "PAC_DRUGGIST "; //藥師代碼

                            commandText += $"from phaadcal where PAC_DRUGNO='{PAC_DRUGNO}' and PAC_VISITDT='{PAC_VISITDT}' AND PAC_PATID='{PAC_PATID}' AND PAC_SEQ='{PAC_SEQ}'  ";
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
                            //commandText += "PAC_SEX,"; //性別 
                            //commandText += "PAC_AGE,"; //年齡 
                            commandText += "PAC_DRUGGIST "; //藥師代碼

                            flag_術中醫令 = true;
                        }
                        else
                        {
                            string PAC_SEQ = strArray_Barcode[0];
                            string PAC_VISITDT = strArray_Barcode[1];
                            string PAC_DIACODE = strArray_Barcode[2];
                            PAC_ORDERSEQ = strArray_Barcode[3];
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
                            //commandText += "PAC_SEX,"; //性別 
                            //commandText += "PAC_AGE,"; //年齡 
                            commandText += "PAC_DRUGGIST "; //藥師代碼

                            commandText += $"from phaadcal where PAC_SEQ='{PAC_SEQ}' and PAC_VISITDT='{PAC_VISITDT}' AND PAC_DIACODE='{PAC_DIACODE}' AND PAC_ORDERSEQ='{PAC_ORDERSEQ}' ";
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
                            //commandText += "PAC_SEX,"; //性別 
                            //commandText += "PAC_AGE,"; //年齡 
                            commandText += "PAC_DRUGGIST "; //藥師代碼

                            flag_術中醫令 = true;
                        }

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
                    //commandText += "PAC_SEX,"; //性別 
                    //commandText += "PAC_AGE,"; //年齡 
                    commandText += "PAC_DRUGGIST "; //藥師代碼

                    commandText += $"from PHAADC where PAC_SEQ='{住院序號}' and PAC_PROCDTTM='{醫令時間}' AND PAC_TYPE='{醫令類型}' ";
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
                    //commandText += "PAC_SEX,"; //性別 
                    //commandText += "PAC_AGE,"; //年齡 
                    commandText += "PAC_DRUGGIST "; //藥師代碼



                }
                else if (strArray_Barcode.Length >= 9)
                {
                    string 本次領藥號 = strArray_Barcode[(int)enum_急診藥袋.本次領藥號];
                    string 看診日期 = strArray_Barcode[(int)enum_急診藥袋.看診日期];
                    string _病歷號 = strArray_Barcode[(int)enum_急診藥袋.病歷號];
                    string 序號 = strArray_Barcode[(int)enum_急診藥袋.序號];
                    看診日期 = strArray_Barcode[(int)enum_急診藥袋.本次醫令序號].ObjectToString().Substring(0, 8);
                    //commandText = $"select * from PHAADC where PAC_DRUGNO={本次領藥號} and PAC_VISITDT={看診日期} and PAC_PATID={_病歷號} and PAC_SEQ={序號}";
                    //commandText = $"select * from phaadcal where PAC_DRUGNO={本次領藥號} and PAC_PATID={_病歷號} and PAC_SEQ={序號}";

                    commandText = "";
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
                    //commandText += "PAC_SEX,"; //性別 
                    //commandText += "PAC_AGE,"; //年齡 
                    commandText += "PAC_DRUGGIST "; //藥師代碼
                   
                    commandText += $"from phaadcal where PAC_DRUGNO={本次領藥號}  and PAC_PATID={_病歷號} and PAC_SEQ={序號} ";
                    //commandText += $"from phaadcal where PAC_DRUGNO={本次領藥號} and PAC_VISITDT = {看診日期}   ";
                    //commandText += $"from phaadcal where PAC_DRUGNO={本次領藥號} and PAC_PATID={_病歷號} and  PAC_VISITDT={看診日期} ";
                    //commandText += $"from phaadcal where PAC_DRUGNO={本次領藥號} and  PAC_VISITDT={看診日期} ";


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
                    //commandText += "PAC_SEX,"; //性別 
                    //commandText += "PAC_AGE,"; //年齡 
                    commandText += "PAC_DRUGGIST "; //藥師代碼


                }

                cmd = new OracleCommand(commandText, conn_oracle);
                List<object[]> list_temp = new List<object[]>();
                string queryTime = "";
                try
                {
                    MyTimerBasic myTimerBasic_query = new MyTimerBasic();
                    Logger.Log("BBAR_control", $"與HIS擷取資料開始");
                    reader = cmd.ExecuteReader();
                    Logger.Log("BBAR_control", $"與HIS擷取資料結束");
                    queryTime = myTimerBasic_query.ToString();
                    List<string> list_colname = new List<string>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        string colname = reader.GetName(i);
                        list_colname.Add(colname);
                    }

                    try
                    {
                        List<personPageClass> personPageClasses = personPageClass.get_all("http://192.168.23.54:4433");

                        while (reader.Read())
                        {
                            List<object> value = new List<object>();
                            for (int i = 0; i < list_colname.Count; i++)
                            {
                                value.Add(reader[list_colname[i]]);

                            }
                            list_temp.Add(value.ToArray());
                            OrderClass orderClass = new OrderClass();
                            string type = reader["PAC_TYPE"].ToString().Trim();
                            if (type == "E") orderClass.藥袋類型 = "PHER";
                            if (type == "S") orderClass.藥袋類型 = "STAT";
                            if (type == "B") orderClass.藥袋類型 = "首日量";
                            if (type == "O") orderClass.藥袋類型 = "OPD";
                            if (type == "M") orderClass.藥袋類型 = "出院帶藥"; //出院帶藥


                            orderClass.PRI_KEY = $"{reader["PAC_ORDERSEQ"].ToString().Trim()}-{reader["PAC_DRUGNO"].ToString().Trim()}";
                            orderClass.藥袋條碼 = BarCode;
                            //orderClass.藥袋條碼 = $"{reader["PAC_VISITDT"].ToString().Trim()}{reader["PAC_PATID"].ToString().Trim()}{reader["PAC_SEQ"].ToString().Trim()}";
                            orderClass.住院序號 = reader["PAC_SEQ"].ToString().Trim();
                            orderClass.藥品碼 = reader["PAC_DIACODE"].ToString().Trim();
                            orderClass.藥品名稱 = reader["PAC_DIANAME"].ToString().Trim();
                            orderClass.病人姓名 = reader["PAC_PATNAME"].ToString().Trim();
                            orderClass.病歷號 = reader["PAC_PATID"].ToString().Trim();
                            orderClass.領藥號 = reader["PAC_DRUGNO"].ToString().Trim();
                            orderClass.就醫時間 = reader["PAC_VISITDT"].ToString().Trim();
                            orderClass.就醫時間 = $"{orderClass.就醫時間.Substring(0, 4)}-{orderClass.就醫時間.Substring(4, 2)}-{orderClass.就醫時間.Substring(6, 2)}";
                            orderClass.科別 = reader["PAC_SECTNAME"].ToString().Trim();
                            orderClass.醫師代碼 = reader["PAC_DOCNAME"].ToString().Trim();
                            orderClass.頻次 = reader["PAC_FEQNO"].ToString().Trim();
                            orderClass.天數 = reader["PAC_DAYS"].ToString().Trim();
                            //性別 = reader["PAC_SEX"].ToString().Trim();
                            //年齡 = reader["PAC_AGE"].ToString().Trim();
                


                            if (reader["PAC_PAYCD"].ToString().Trim() == "Y")
                            {
                                orderClass.費用別 = "自費";
                            }
                            else
                            {
                                orderClass.費用別 = "健保";
                            }

                            if (strArray_Barcode.Length >= 9)
                            {
                                string PAC_DRUGGIST = reader["PAC_DRUGGIST"].ToString().Trim();
                                orderClass.藥師ID = PAC_DRUGGIST.StringToInt32().ToString();
                                List<personPageClass> personPageClasses_buf = (from temp in personPageClasses
                                                                               where temp.ID == orderClass.藥師ID
                                                                               select temp).ToList();
                                if (personPageClasses_buf.Count > 0)
                                {
                                    orderClass.藥師姓名 = personPageClasses_buf[0].姓名;
                                }
                            }


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
                    catch(Exception ex)
                    {

                        return $"HIS系統回傳資料異常! \n {ex} \n {commandText}";
                    }
                }
                catch(Exception ex)
                {
                    return $"HIS系統命令下達失敗!\n {ex} \n { commandText} ";
                }
                conn_oracle.Close();
                conn_oracle.Dispose();
                string HISData = myTimer_HisData.ToString();
                MyTimerBasic myTimer_DB = new MyTimerBasic();
                if (orderClasses.Count == 0)
                {
                    returnData.Code = -200;
                    returnData.TimeTaken = myTimer_total.ToString();
                    returnData.Result = $"無此藥袋資料!";

                    return returnData.JsonSerializationt(true);
                }
                List<Task> tasks = new List<Task>();
                tasks.Add(Task.Run(new Action(delegate
                {
                    string reponse = ICD(orderClasses);
                    returnData returnData_ICD = reponse.JsonDeserializet<returnData>();
                    list_ICD = returnData_ICD.Data.ObjToClass<List<string>>();
                })));
                List<List<OrderClass>> list_orderclasses = GroupOrders(orderClasses);
                for (int i = 0; i < list_orderclasses.Count; i++)
                {
                    double Truncate;
                    List<OrderClass> temp_orderclasses = list_orderclasses[i];
                    double 總量 = 0.0D;
                    for (int k = 0; k < temp_orderclasses.Count; k++)
                    {

                        總量 += temp_orderclasses[k].交易量.StringToDouble();

                    }
                    Truncate = 總量 - Math.Truncate(總量);
                    if (Truncate != 0) 總量 = (int)總量 - 1;
                    bool 總量已到達 = false;
                    for (int k = 0; k < temp_orderclasses.Count; k++)
                    {


                        double 交易量 = temp_orderclasses[k].交易量.StringToDouble();
                        Truncate = 交易量 - Math.Truncate(交易量);
                        if (Truncate != 0) 交易量 = (int)交易量 - 1;
                        if (總量已到達)
                        {
                            temp_orderclasses[k].交易量 = "0";
                            continue;
                        }
                        if (總量 - 交易量 <= 0)
                        {
                            temp_orderclasses[k].交易量 = 交易量.ToString();
                        }
                        else
                        {
                            temp_orderclasses[k].交易量 = 總量.ToString();
                            總量已到達 = true;
                        }
                        總量 = 總量 - 交易量;
                    }
                }
                List<KeyValuePair<string, int>> groupedByDate = orderClasses
                    .GroupBy(order => order.就醫時間)
                    .Select(group => new KeyValuePair<string, int>(group.Key, group.Count()))
                    .ToList();

                // 輸出分類結果
                Logger.Log("BBAR_control", $"條碼: {BarCode}");

                foreach (KeyValuePair<string, int> item in groupedByDate)
                {
                    Logger.Log("BBAR_control", $"日期: {item.Key}, 筆數: {item.Value}");
                }
                string 病歷號 = orderClasses[0].病歷號;
                string Today = DateTime.Now.ToString("yyyy-MM-dd");
                string tenDaysAgo = DateTime.Now.AddDays(-0).ToString("yyyy-MM-dd");

                if(BarCode!= "1013;20250617;0001680569;1;HS;PO;28;0;20250617082937981") orderClasses = orderClasses.Where(temp => string.Compare(temp.就醫時間, tenDaysAgo) >= 0).ToList();

                
                List<OrderClass> orders = OrderClass.get_by_PATCODE("http://127.0.0.1:4433", 病歷號);
                List<object[]> list_value = sQLControl_醫囑資料.GetRowsByDefult(null, (int)enum_醫囑資料.病歷號, 病歷號);
           
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
                        value[(int)enum_醫囑資料.住院序號] = orderClasses[i].住院序號;
                        value[(int)enum_醫囑資料.醫師代碼] = orderClasses[i].醫師代碼;
                        value[(int)enum_醫囑資料.科別] = orderClasses[i].科別;
                        value[(int)enum_醫囑資料.就醫時間] = orderClasses[i].就醫時間;
                        value[(int)enum_醫囑資料.藥師姓名] = orderClasses[i].藥師姓名;
                        value[(int)enum_醫囑資料.頻次] = orderClasses[i].頻次;
                        value[(int)enum_醫囑資料.天數] = orderClasses[i].天數;
                        value[(int)enum_醫囑資料.藥袋類型] = orderClasses[i].藥袋類型;
                        value[(int)enum_醫囑資料.費用別] = orderClasses[i].費用別;
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
                            value[(int)enum_醫囑資料.住院序號] = orderClasses[i].住院序號;
                            value[(int)enum_醫囑資料.藥品碼] = orderClasses[i].藥品碼;
                            value[(int)enum_醫囑資料.藥品名稱] = orderClasses[i].藥品名稱;
                            value[(int)enum_醫囑資料.病歷號] = orderClasses[i].病歷號;
                            value[(int)enum_醫囑資料.藥袋條碼] = orderClasses[i].藥袋條碼;
                            value[(int)enum_醫囑資料.病人姓名] = orderClasses[i].病人姓名;
                            value[(int)enum_醫囑資料.交易量] = orderClasses[i].交易量;
                            value[(int)enum_醫囑資料.醫師代碼] = orderClasses[i].醫師代碼;
                            value[(int)enum_醫囑資料.科別] = orderClasses[i].科別;
                            value[(int)enum_醫囑資料.就醫時間] = orderClasses[i].就醫時間;
                            value[(int)enum_醫囑資料.開方日期] = orderClasses[i].開方日期;
                            value[(int)enum_醫囑資料.藥師姓名] = orderClasses[i].藥師姓名;
                            value[(int)enum_醫囑資料.頻次] = orderClasses[i].頻次;
                            value[(int)enum_醫囑資料.天數] = orderClasses[i].天數;
                            value[(int)enum_醫囑資料.藥袋類型] = orderClasses[i].藥袋類型;
                            value[(int)enum_醫囑資料.費用別] = orderClasses[i].費用別;

                            value[(int)enum_醫囑資料.狀態] = "未過帳";
                            orderClasses[i].狀態 = "未過帳";
                            list_value_replace.Add(value);
                        }



                    }
                }
                Task.WhenAll(tasks).Wait();
                tasks.Clear();
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
                tasks.Add(Task.Run(new Action(delegate
                {
                    if (orderClasses.Count == 0) return;
                    List<suspiciousRxLogClass> suspiciousRxLoges = suspiciousRxLogClass.get_by_barcode(API_Server, orderClasses[0].藥袋條碼);
                    suspiciousRxLogClass suspiciousRxLogClasses = new suspiciousRxLogClass();

                    if (suspiciousRxLoges.Count == 0)
                    {
                        List<string> disease_list = list_ICD;
                        //if (ICD1.StringIsEmpty() == false) disease_list.Add(ICD1);
                        //if (ICD2.StringIsEmpty() == false) disease_list.Add(ICD2);
                        //if (ICD3.StringIsEmpty() == false) disease_list.Add(ICD3);
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
                            性別 = 性別,
                            年齡= 年齡,
                            diseaseClasses = diseaseClasses
                        };
                        suspiciousRxLogClass.add(API_Server, suspiciousRxLogClasses);
                    }
                })));
                Task.WhenAll(tasks).Wait();
                string DBTIme = myTimer_DB.ToString();
                returnData.Code = 200;
                returnData.Data = orderClasses;
                returnData.TimeTaken = myTimer_total.ToString();
                returnData.Result = $"取得醫囑完成! 共<{orderClasses.Count}>筆 ,新增<{list_value_Add.Count}>筆,修改<{list_value_replace.Count}>筆 ,  HIS連線時間；{HIS連線時間}，取得HIS資料；{HISData}，DB寫入時間:{DBTIme}";
                string json_result = returnData.JsonSerializationt(true);
                Logger.Log("BBAR_control", json_result);
                return json_result;
            }
            catch(Exception ex)
            {

                return $"醫令串接異常 \n {ex}";
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


                commandText = "";
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
                commandText += "PAC_SEX,"; //性別 
                //commandText += "PAC_AGE,"; //年齡 
                commandText += "PAC_DRUGGIST "; //藥師代碼
                commandText += $"from phaadcal where SUBSTR(PAC_PROCDTTM, 1, 8) = '{datetime.Year}{datetime.Month.ToString("00")}{datetime.Day.ToString("00")}' ";
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
                commandText += "PAC_SEX,"; //性別 
                //commandText += "PAC_AGE,"; //年齡 
                commandText += "PAC_DRUGGIST "; //藥師代碼
                commandText += " ORDER BY PAC_PROCDTTM DESC ";
                commandText += " FETCH FIRST 300 ROWS ONLY ";
                cmd = new OracleCommand(commandText, conn_oracle);
                try
                {
                    reader = cmd.ExecuteReader();


                    try
                    {
                        while (reader.Read())
                        {
                            try
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

                                if (type == "E") orderClass.藥袋類型 = "PHER";
                                if (type == "S") orderClass.藥袋類型 = "STAT";
                                if (type == "B") orderClass.藥袋類型 = "首日量";
                                if (type == "O") orderClass.藥袋類型 = "OPD";
                                if (type == "M") orderClass.藥袋類型 = "出院帶藥"; 

                                //orderClass. = reader["PAC_SEQ"].ToString().Trim();
                                orderClass.PRI_KEY = $"{reader["PAC_ORDERSEQ"].ToString().Trim()}-{reader["PAC_DRUGNO"].ToString().Trim()}";
                                orderClass.藥袋條碼 = $"{reader["PAC_VISITDT"].ToString().Trim()}{reader["PAC_PATID"].ToString().Trim()}{reader["PAC_SEQ"].ToString().Trim()}";
                                orderClass.藥品碼 = reader["PAC_DIACODE"].ToString().Trim();
                                orderClass.藥品名稱 = reader["PAC_DIANAME"].ToString().Trim();
                                orderClass.病人姓名 = reader["PAC_PATNAME"].ToString().Trim();
                                orderClass.病歷號 = reader["PAC_PATID"].ToString().Trim();
                                orderClass.領藥號 = reader["PAC_DRUGNO"].ToString().Trim();
                                orderClass.產出時間 = DateTime.Now.ToDateTimeString_6();
                                string PAC_QTYPERTIME = reader["PAC_QTYPERTIME"].ToString().Trim();
                                string PAC_SUMQTY = reader["PAC_SUMQTY"].ToString().Trim();
                                double sumQTY = PAC_SUMQTY.StringToDouble();
                                //sumQTY = Math.Ceiling(sumQTY);
                                orderClass.交易量 = (sumQTY * -1).ToString();

                                orderClasses.Add(orderClass);
                            }
                            catch
                            {

                            }

                            


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
        [Route("get_by_BagNum_date")]
        [HttpPost]
        public string get_by_BagNum_date([FromBody] returnData returnData)
        {
            if (returnData.ValueAry == null)
            {
                returnData.Code = -200;
                returnData.Result = $"returnData.ValueAry 無傳入資料";
                return returnData.JsonSerializationt(true);
            }
            if (returnData.ValueAry.Count != 2)
            {
                returnData.Code = -200;
                returnData.Result = $"returnData.ValueAry 內容應為[領藥號,日期]";
                return returnData.JsonSerializationt(true);
            }
            bool flag_術中醫令 = false;
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
                //returnData returnData = new returnData();
                MyTimerBasic myTimerBasic = new MyTimerBasic();
                string commandText = "";
                List<OrderClass> orderClasses = new List<OrderClass>();
                List<object[]> list_value_Add = new List<object[]>();
                List<object[]> list_value_replace = new List<object[]>();
                string[] strArray_Barcode = new string[0];

                string 本次領藥號 = returnData.ValueAry[0];
                string 開方時間 = returnData.ValueAry[1];
                DateTime datetime = DateTime.Parse(開方時間);
                string formattedDate = datetime.ToString("yyyyMMdd");


                //commandText = "";
                //commandText += "select ";
                //commandText += "min(PAC_VISITDT) PAC_VISITDT,";
                //commandText += "sum(PAC_SUMQTY) PAC_SUMQTY,";
                //commandText += "PAC_ORDERSEQ,";
                //commandText += "PAC_SEQ,";
                //commandText += "PAC_DIACODE,";
                //commandText += "PAC_DIANAME,";
                //commandText += "PAC_PATNAME,";
                //commandText += "PAC_PATID,";
                //commandText += "PAC_UNIT,";
                //commandText += "PAC_QTYPERTIME,";
                //commandText += "PAC_FEQNO,";
                //commandText += "PAC_PATHNO,";
                //commandText += "PAC_DAYS,";
                //commandText += "PAC_TYPE,";
                //commandText += "PAC_DRUGNO,";
                //commandText += "PAC_SECTNAME,";
                //commandText += "PAC_DOCNAME,";
                //commandText += "PAC_PROCDTTM ,";
                //commandText += "PAC_VISITDT ,";
                //commandText += "PAC_DRUGGIST ";
                ////commandText += $"from phaadcal where PAC_DRUGNO={本次領藥號}  and SUBSTR(PAC_PROCDTTM, 1, 8) = '{datetime.Year}{datetime.Month.ToString("00")}{datetime.Day.ToString("00")}' ";
                //commandText += $"from phaadcal where PAC_DRUGNO={本次領藥號}";

                //commandText += "GROUP BY ";

                //commandText += "PAC_ORDERSEQ,";
                //commandText += "PAC_SEQ,";
                //commandText += "PAC_DIACODE,";
                //commandText += "PAC_DIANAME,";
                //commandText += "PAC_PATNAME,";
                //commandText += "PAC_PATID,";
                //commandText += "PAC_UNIT,";
                //commandText += "PAC_QTYPERTIME,";
                //commandText += "PAC_FEQNO,";
                //commandText += "PAC_PATHNO,";
                //commandText += "PAC_DAYS,";
                //commandText += "PAC_TYPE,";
                //commandText += "PAC_DRUGNO,";
                //commandText += "PAC_SECTNAME,";
                //commandText += "PAC_DOCNAME,";
                //commandText += "PAC_PROCDTTM ,";
                //commandText += "PAC_VISITDT ,";
                //commandText += "PAC_DRUGGIST ";
                string date_str = $"{ datetime.Year }{ datetime.Month.ToString("00")}{ datetime.Day.ToString("00")}";
                commandText = "";
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
                commandText += "PAC_SECTNAME,";
                commandText += "PAC_DOCNAME,";
                commandText += "PAC_PROCDTTM ,";
                commandText += "PAC_VISITDT ,";
                commandText += "PAC_DRUGGIST ";
                commandText += $"from phaadcal where PAC_DRUGNO={本次領藥號} and PAC_VISITDT = {date_str}   ";
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
                commandText += "PAC_SECTNAME,";
                commandText += "PAC_DOCNAME,";
                commandText += "PAC_PROCDTTM ,";
                commandText += "PAC_VISITDT ,";
                commandText += "PAC_DRUGGIST ";


                cmd = new OracleCommand(commandText, conn_oracle);
                List<object[]> list_temp = new List<object[]>();
                string queryTime = "";
                try
                {
                    MyTimerBasic myTimerBasic_query = new MyTimerBasic();

                    reader = cmd.ExecuteReader();
                    queryTime = myTimerBasic_query.ToString();
                    List<string> list_colname = new List<string>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        string colname = reader.GetName(i);
                        list_colname.Add(colname);
                    }

                    try
                    {
                        List<personPageClass> personPageClasses = personPageClass.get_all("http://192.168.23.54:4433");

                        while (reader.Read())
                        {
                            List<object> value = new List<object>();
                            for (int i = 0; i < list_colname.Count; i++)
                            {
                                value.Add(reader[list_colname[i]]);

                            }
                            list_temp.Add(value.ToArray());
                            OrderClass orderClass = new OrderClass();
                            string type = reader["PAC_TYPE"].ToString().Trim();
                            if (type == "E") orderClass.藥局代碼 = "PHER";
                            if (type == "S") orderClass.藥局代碼 = "STAT";
                            if (type == "B") orderClass.藥局代碼 = "首日量";
                            if (type == "O") orderClass.藥局代碼 = "OPD";
                            if (type == "M") orderClass.藥局代碼 = "出院帶藥";

                            orderClass.PRI_KEY = $"{reader["PAC_ORDERSEQ"].ToString().Trim()}-{reader["PAC_DRUGNO"].ToString().Trim()}";
                            orderClass.藥袋條碼 = $"{reader["PAC_VISITDT"].ToString().Trim()}{reader["PAC_PATID"].ToString().Trim()}{reader["PAC_SEQ"].ToString().Trim()}";
                            orderClass.住院序號 = reader["PAC_SEQ"].ToString().Trim();
                            orderClass.藥品碼 = reader["PAC_DIACODE"].ToString().Trim();
                            orderClass.藥品名稱 = reader["PAC_DIANAME"].ToString().Trim();
                            orderClass.病人姓名 = reader["PAC_PATNAME"].ToString().Trim();
                            orderClass.病歷號 = reader["PAC_PATID"].ToString().Trim();
                            orderClass.領藥號 = reader["PAC_DRUGNO"].ToString().Trim();

                            orderClass.就醫時間 = reader["PAC_VISITDT"].ToString().Trim();
                            orderClass.就醫時間 = $"{orderClass.就醫時間.Substring(0, 4)}-{orderClass.就醫時間.Substring(4, 2)}-{orderClass.就醫時間.Substring(6, 2)}";
                            orderClass.科別 = reader["PAC_SECTNAME"].ToString().Trim();
                            orderClass.醫師代碼 = reader["PAC_DOCNAME"].ToString().Trim();


                            if (strArray_Barcode.Length >= 9)
                            {
                                string PAC_DRUGGIST = reader["PAC_DRUGGIST"].ToString().Trim();
                                orderClass.藥師ID = PAC_DRUGGIST.StringToInt32().ToString();
                                List<personPageClass> personPageClasses_buf = (from temp in personPageClasses
                                                                               where temp.ID == orderClass.藥師ID
                                                                               select temp).ToList();
                                if (personPageClasses_buf.Count > 0)
                                {
                                    orderClass.藥師姓名 = personPageClasses_buf[0].姓名;
                                }
                            }


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
                    for (int k = 0; k < temp_orderclasses.Count; k++)
                    {

                        總量 += temp_orderclasses[k].交易量.StringToDouble();

                    }
                    Truncate = 總量 - Math.Truncate(總量);
                    if (Truncate != 0) 總量 = (int)總量 - 1;
                    bool 總量已到達 = false;
                    for (int k = 0; k < temp_orderclasses.Count; k++)
                    {


                        double 交易量 = temp_orderclasses[k].交易量.StringToDouble();
                        Truncate = 交易量 - Math.Truncate(交易量);
                        if (Truncate != 0) 交易量 = (int)交易量 - 1;
                        if (總量已到達)
                        {
                            temp_orderclasses[k].交易量 = "0";
                            continue;
                        }
                        if (總量 - 交易量 <= 0)
                        {
                            temp_orderclasses[k].交易量 = 交易量.ToString();
                        }
                        else
                        {
                            temp_orderclasses[k].交易量 = 總量.ToString();
                            總量已到達 = true;
                        }
                        總量 = 總量 - 交易量;
                    }
                }
                List<KeyValuePair<string, int>> groupedByDate = orderClasses
                    .GroupBy(order => order.就醫時間)
                    .Select(group => new KeyValuePair<string, int>(group.Key, group.Count()))
                    .ToList();

                // 輸出分類結果
                //Logger.Log("BBAR", $"條碼: {BarCode}");

                foreach (KeyValuePair<string, int> item in groupedByDate)
                {
                    Logger.Log("BBAR", $"日期: {item.Key}, 筆數: {item.Value}");
                }
                string 病歷號 = orderClasses[0].病歷號;
                string Today = DateTime.Now.ToString("yyyy-MM-dd");
                string tenDaysAgo = DateTime.Now.AddDays(-0).ToString("yyyy-MM-dd");
                //orderClasses = orderClasses.Where(temp => string.Compare(temp.就醫時間, tenDaysAgo) >= 0 && string.Compare(temp.就醫時間, Today) <= 0).ToList();
                orderClasses = orderClasses.Where(temp => string.Compare(temp.就醫時間, tenDaysAgo) >= 0).ToList();

                
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
                        value[(int)enum_醫囑資料.住院序號] = orderClasses[i].住院序號;
                        value[(int)enum_醫囑資料.醫師代碼] = orderClasses[i].醫師代碼;
                        value[(int)enum_醫囑資料.科別] = orderClasses[i].科別;
                        value[(int)enum_醫囑資料.就醫時間] = orderClasses[i].就醫時間;
                        value[(int)enum_醫囑資料.藥師姓名] = orderClasses[i].藥師姓名;
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
                            value[(int)enum_醫囑資料.住院序號] = orderClasses[i].住院序號;
                            value[(int)enum_醫囑資料.藥品碼] = orderClasses[i].藥品碼;
                            value[(int)enum_醫囑資料.藥品名稱] = orderClasses[i].藥品名稱;
                            value[(int)enum_醫囑資料.病歷號] = orderClasses[i].病歷號;
                            value[(int)enum_醫囑資料.藥袋條碼] = orderClasses[i].藥袋條碼;
                            value[(int)enum_醫囑資料.病人姓名] = orderClasses[i].病人姓名;
                            value[(int)enum_醫囑資料.交易量] = orderClasses[i].交易量;
                            value[(int)enum_醫囑資料.醫師代碼] = orderClasses[i].醫師代碼;
                            value[(int)enum_醫囑資料.科別] = orderClasses[i].科別;
                            value[(int)enum_醫囑資料.就醫時間] = orderClasses[i].就醫時間;
                            value[(int)enum_醫囑資料.開方日期] = orderClasses[i].開方日期;
                            value[(int)enum_醫囑資料.藥師姓名] = orderClasses[i].藥師姓名;

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
                returnData.Result = $"取得醫囑完成! 共<{orderClasses.Count}>筆 ,新增<{list_value_Add.Count}>筆,修改<{list_value_replace.Count}>筆 , 從DB取得時間:{queryTime}";
                string json_result = returnData.JsonSerializationt(true);
                Logger.Log("BBAR", json_result);
                return json_result;
            }
            catch
            {
                return "醫令串接異常";
            }

        }
        [HttpGet("get_DBdata")]
        public string get_DBdata(string? commandText)
        {
            returnData returnData = new returnData();
            MyTimerBasic myTimerBasic = new MyTimerBasic();
            try
            {
                OracleConnection conn_oracle;
                OracleDataReader reader;
                OracleCommand cmd;
                try
                {
                    conn_oracle = new OracleConnection(conn_str);
                    conn_oracle.Open();
                    Logger.Log("conn_oracle", $"與HIS建立連線");
                }
                catch
                {
                    return "HIS系統連線失敗";
                }
                cmd = new OracleCommand(commandText, conn_oracle);
                Logger.Log("conn_oracle", $"與HIS擷取資料開始");
                reader = cmd.ExecuteReader();
                Logger.Log("conn_oracle", $"與HIS擷取資料結束");
                returnData.Code = 200;
                returnData.TimeTaken = $"{myTimerBasic}";
                returnData.Result = "成功取得資料";
                return returnData.JsonSerializationt(true);
            }
            catch(Exception ex)
            {
                returnData.Code = -200;
                returnData.Result = ex.Message;
                return returnData.JsonSerializationt(true);

            }
        }
        [HttpGet("pragnant")]
        public string pragnant(string? ID)
        {
            OracleConnection conn_oracle;
            OracleDataReader reader;
            OracleCommand cmd;
            returnData returnData = new returnData();
            List<object[]> list_value = new List<object[]>();

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
                MyTimerBasic myTimerBasic = new MyTimerBasic();
                string commandText = "";

                commandText += "select ";
                commandText += "* ";
                commandText += $"from PHAADCPRGY where PRG_PATID ='{ID}' ";

                cmd = new OracleCommand(commandText, conn_oracle);
                OracleDataAdapter adapter = new OracleDataAdapter(cmd);
                try
                {
                    reader = cmd.ExecuteReader();
                    try
                    {
                        while (reader.Read())
                        {
                            object[] value = new object[new enum_懷孕檢測報告().GetLength()];
                            value[(int)enum_懷孕檢測報告.病歷號] = reader["PRG_PATID"].ToString().Trim();
                            value[(int)enum_懷孕檢測報告.院內碼] = reader["PRG_DIACODE"].ToString().Trim();
                            value[(int)enum_懷孕檢測報告.健保碼] = reader["PRG_INSCODE"].ToString().Trim();
                            value[(int)enum_懷孕檢測報告.檢驗項目名稱] = reader["PRG_EGNAME"].ToString().Trim();
                            value[(int)enum_懷孕檢測報告.報告值] = reader["PRG_STATE"].ToString().Trim();
                            value[(int)enum_懷孕檢測報告.報告日期] = reader["PRG_REPDTTM"].ToString().Trim();
                            list_value.Add(value);
                        }
                    }
                    catch
                    {
                        return "HIS系統回傳資料異常!";
                    }
                }
                catch (Exception ex)
                {
                    return "HIS系統命令下達失敗! \n {ex} \n {commandText}!";
                }
                conn_oracle.Close();
                conn_oracle.Dispose();
                List<pragnantClass> pragnantClasses = list_value.SQLToClass<pragnantClass, enum_懷孕檢測報告>();
                returnData.Code = 200;
                returnData.Data = pragnantClasses;
                returnData.TimeTaken = myTimerBasic.ToString();
                returnData.Result = $"取得懷孕資料! 共<{pragnantClasses.Count}>筆 ";
                return returnData.JsonSerializationt(true);
            }
            catch (Exception ex)
            {
                return $"Exception : {ex.Message} ";
            }

        }
        [HttpGet("ICD")]
        public string ICD(List<OrderClass> orderClasses)
        {
            OracleConnection conn_oracle;
            OracleDataReader reader;
            OracleCommand cmd;
            returnData returnData = new returnData();
            List<object[]> list_value = new List<object[]>();
            List<string> list_ICD = new List<string>();

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
                MyTimerBasic myTimerBasic = new MyTimerBasic();
                string commandText = "";
                string 就醫時間 = orderClasses[0].就醫時間.Replace("-", "");
                string 病歷號 = orderClasses[0].病歷號;
                string 住院序號 = orderClasses[0].住院序號;
                string ICD = string.Empty;
                commandText += "select ";
                commandText += "* ";
                commandText += $"from PHAOPDSOA where SOA_VISITDT ='{就醫時間}' and SOA_PATID ='{病歷號}' and  SOA_SEQ = '{住院序號}'";

                cmd = new OracleCommand(commandText, conn_oracle);
                OracleDataAdapter adapter = new OracleDataAdapter(cmd);
                try
                {
                    reader = cmd.ExecuteReader();
                    try
                    {
                        while (reader.Read())
                        {
                            ICD = reader["SOA_CONTENT"].ToString().Trim();
                            ICD = ICD.Split("_")[0];
                            list_ICD.Add(ICD);
                        }
                    }
                    catch
                    {
                        return "HIS系統回傳資料異常!";
                    }
                }
                catch (Exception ex)
                {
                    return "HIS系統命令下達失敗! \n {ex} \n {commandText}!";
                }
                conn_oracle.Close();
                conn_oracle.Dispose();
                returnData.Code = 200;
                returnData.Data = list_ICD;
                returnData.TimeTaken = myTimerBasic.ToString();
                returnData.Result = $"取得疾病資料! 共<{list_ICD.Count}>筆 ";
                return returnData.JsonSerializationt(true);
            }
            catch (Exception ex)
            {
                return $"Exception : {ex.Message} ";
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
        private enum enum_懷孕檢測報告
        {
            病歷號,
            院內碼,
            健保碼,
            檢驗項目名稱,
            報告值,
            報告日期,
        }
        private class pragnantClass
        {
            [JsonPropertyName("PRG_PATID")]
            public string 病歷號 { get; set; }
            [JsonPropertyName("PRG_DIACODE")]
            public string 院內碼 { get; set; }
            [JsonPropertyName("PRG_INSCODE")]
            public string 健保碼 { get; set; }
            [JsonPropertyName("PRG_EGNAME")]
            public string 檢驗項目名稱 { get; set; }
            [JsonPropertyName("PRG_STATE")]
            public string 報告值 { get; set; }
            [JsonPropertyName("PRG_REPDTTM")]
            public string 報告日期 { get; set; }
        }

    }

}
