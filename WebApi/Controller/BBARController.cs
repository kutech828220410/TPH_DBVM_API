using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;
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

        static string MySQL_server = $"{ConfigurationManager.AppSettings["MySQL_server"]}";
        static string MySQL_database = $"{ConfigurationManager.AppSettings["MySQL_database"]}";
        static string MySQL_userid = $"{ConfigurationManager.AppSettings["MySQL_user"]}";
        static string MySQL_password = $"{ConfigurationManager.AppSettings["MySQL_password"]}";
        static string MySQL_port = $"{ConfigurationManager.AppSettings["MySQL_port"]}";

        private SQLControl sQLControl_UDSDBBCM = new SQLControl(MySQL_server, MySQL_database, "UDSDBBCM", MySQL_userid, MySQL_password, (uint)MySQL_port.StringToInt32(), MySql.Data.MySqlClient.MySqlSslMode.None);
        private SQLControl sQLControl_醫囑資料 = new SQLControl(MySQL_server, MySQL_database, "order_list", MySQL_userid, MySQL_password, (uint)MySQL_port.StringToInt32(), MySql.Data.MySqlClient.MySqlSslMode.None);
        private string API_Server = "https://pharma-cetrlm.tph.mohw.gov.tw:4443";
        [HttpGet]
        public string Get(string? BarCode, string? test, string? MRN)
        {
            bool flag_術中醫令 = false;
            string conn_str = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.24.211)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=SISDCP)));User ID=tphphaadc;Password=tph@phaadc2860;";
            OracleConnection conn_oracle;
            OracleDataReader reader;
            OracleCommand cmd;
            string ICD1 = "";
            string ICD2 = "";
            string ICD3 = "";
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
                            ICD1 = reader["PAC_ICDX1"].ToString().Trim();
                            ICD2 = reader["PAC_ICDX2"].ToString().Trim();
                            ICD3 = reader["PAC_ICDX3"].ToString().Trim();





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
                List<Task> tasks = new List<Task>();
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
                    
                    List<suspiciousRxLogClass> suspiciousRxLoges = suspiciousRxLogClass.get_by_barcode(API_Server, orderClasses[0].藥袋條碼);
                    List<string> list_診斷碼 = new List<string>();
                    List<string> list_中文說明 = new List<string>();
                    suspiciousRxLogClass suspiciousRxLogClasses = new suspiciousRxLogClass();

                    if (suspiciousRxLoges.Count  != 0)
                    {
                        List<string> disease_list = new List<string>();
                        if (ICD1.StringIsEmpty() == false) disease_list.Add(ICD1);
                        if (ICD2.StringIsEmpty() == false) disease_list.Add(ICD2);
                        if (ICD3.StringIsEmpty() == false) disease_list.Add(ICD3);
                        if(disease_list.Count > 0)
                        {
                            string disease = string.Join(";", disease_list);

                            List<diseaseClass> diseaseClasses = diseaseClass.get_by_ICD(API_Server, disease);
                            
                            if (diseaseClasses.Count > 0)
                            {
                                foreach (var item in diseaseClasses)
                                {
                                    list_診斷碼.Add(item.疾病代碼);
                                    list_中文說明.Add(item.中文說明);
                                }
                            }
                        }
                        

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
                            調劑時間 = orderClasses[0].過帳時間,
                            //提報等級 = enum_suspiciousRxLog_ReportLevel.Normal.GetEnumName(),
                            提報時間 = DateTime.MinValue.ToDateTimeString(),
                            處理時間 = DateTime.MinValue.ToDateTimeString(),
                            診斷碼 = string.Join(";", list_診斷碼),
                            診斷內容 = string.Join(";", list_中文說明)
                        };
                        suspiciousRxLogClass.add(API_Server, suspiciousRxLogClasses);
                    }
                })));
                Task.WhenAll(tasks).Wait();
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


        [Route("order_controll_drug")]
        [HttpGet]
        public string order_controll_drug(string? BarCode, string? MRN)
        {
            bool flag_術中醫令 = false;
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
                    Logger.Log("BBAR_control", $"與HIS建立連線");

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
                            string ICD1 = reader["PAC_ICDX1"].ToString().Trim();
                            string ICD2 = reader["PAC_ICDX2"].ToString().Trim();
                            string ICD3 = reader["PAC_ICDX3"].ToString().Trim();


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
                Logger.Log("BBAR_control", $"條碼: {BarCode}");

                foreach (KeyValuePair<string, int> item in groupedByDate)
                {
                    Logger.Log("BBAR_control", $"日期: {item.Key}, 筆數: {item.Value}");
                }
                string 病歷號 = orderClasses[0].病歷號;
                string Today = DateTime.Now.ToString("yyyy-MM-dd");
                string tenDaysAgo = DateTime.Now.AddDays(-0).ToString("yyyy-MM-dd");
                //string tenDaysAgo = DateTime.Now.AddDays(-5).ToString("yyyy-MM-dd");

                //orderClasses = orderClasses.Where(temp => string.Compare(temp.就醫時間, tenDaysAgo) >= 0 && string.Compare(temp.就醫時間, Today) <= 0).ToList();
                orderClasses = orderClasses.Where(temp => string.Compare(temp.就醫時間, tenDaysAgo) >= 0).ToList();

                
                List<OrderClass> orders = OrderClass.get_by_PATCODE("http://127.0.0.1:4433", 病歷號);
                List<object[]> list_value = sQLControl_醫囑資料.GetRowsByDefult(null, (int)enum_醫囑資料.病歷號, 病歷號);
           
                //List<object[]> list_value = sQLControl.GetRowsByDefult(null, enum_醫囑資料.病歷號.GetEnumName(), 病歷號);
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
                            orderClass.產出時間 = DateTime.Now.ToDateTimeString_6();
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
                string conn_str = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.24.211)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=SISDCP)));User ID=tphphaadc;Password=tph@phaadc2860;";
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
        public static List<List<OrderClass>> GroupOrders(List<OrderClass> orders)
        {
            List<List<OrderClass>> groupedOrders = orders
                .GroupBy(o => new { o.藥品碼, o.病歷號, o.開方日期 })
                .Select(group => group.ToList())
                .ToList();

            return groupedOrders;
        }

        [HttpGet("order")]
        public string orderGET(string? BarCode)
        {
            MyTimerBasic myTimerBasic = new MyTimerBasic();
            returnData returnData = new returnData();
            returnData.Method = "api/bbar/order?barcode=";
            try
            {
                if (BarCode.StringIsEmpty())
                {
                    returnData.Code = -200;
                    returnData.Result = "Barcode空白";
                    return returnData.JsonSerializationt(true);
                }
                List<object[]> list_pha_order = sQLControl_醫囑資料.GetRowsByDefult(null, (int)enum_醫囑資料.藥袋條碼, BarCode);
                List<OrderClass> orders = list_pha_order.SQLToClass<OrderClass, enum_醫囑資料>();
                orders = (from temp in orders
                          where temp.產出時間.StringToDateTime() >= DateTime.Now.AddDays(-1).GetStartDate()
                          select temp).ToList();       
                List<cpoe> eff_cpoe = GroupOrderList(orders);
               

                string 病歷號 = orders[0].病歷號;
                List<object[]> list_order = sQLControl_醫囑資料.GetRowsByDefult(null, (int)enum_醫囑資料.病歷號, 病歷號);
                List<OrderClass> history_order = list_order.SQLToClass<OrderClass, enum_醫囑資料>();
                history_order = (from temp in history_order
                                 where temp.產出時間.StringToDateTime() >= DateTime.Now.AddDays(-1).AddMonths(-3).GetStartDate()
                                 where temp.產出時間.StringToDateTime() >= DateTime.Now.AddDays(-1).AddMonths(-3).GetStartDate()
                                 select temp).ToList();
                List<cpoe> old_cpoe = GroupOrderList(history_order);
                
                
                identify result = new identify
                {
                    有效處方 = eff_cpoe,
                    歷史處方 = old_cpoe
                };

                returnData.Data = result;
                returnData.Code = 200;
                returnData.Result = $"取得醫令資料";
                return returnData.JsonSerializationt(true);
            }
            catch (Exception ex)
            {
                returnData.Code = -200;
                returnData.Result = $"Exception:{ex.Message}";
                return returnData.JsonSerializationt(true);
            }
        }
        private List<cpoe> GroupOrderList(List<OrderClass> orderClasses)
        {
            List<medClass> medClasses = medClass.get_med_cloud("http://127.0.0.1:4433");
            Dictionary<string, List<medClass>> medClassDict = medClasses.CoverToDictionaryByCode();
            List<cpoe> cpoeList = orderClasses
                .GroupBy(temp => temp.藥袋條碼)
                .Select(group =>
                {
                    OrderClass orderClass = group.First();
                    List<order> orders = group
                    .Select(value =>
                    {
                        medClass med = medClassDict.SortDictionaryByCode(value.藥品碼).FirstOrDefault();
                        if (med == null) return null;
                        
                        return new order
                        {
                            藥品名稱 = value.藥品名稱,
                            費用別 = value.費用別,
                            交易量 = value.交易量.Replace("-", ""),
                            頻次 = value.頻次,
                            天數 = value.天數,
                            健保碼 = med.健保碼,
                            ATC = med.ATC,
                            藥品學名 = med.藥品學名,
                            藥品許可證號 = med.藥品許可證號,
                            管制級別 = med.管制級別,
                        };
                        
                    })
                    .Where(value => value != null)
                    .ToList();
                    return new cpoe
                    {
                        藥袋條碼 = group.Key,
                        產出時間 = orderClass.產出時間,
                        醫師代碼 = group.Any(item => item.醫師代碼 == item.病人姓名).ToString(),
                        處方 = orders
                    };
                }).ToList();
            return cpoeList;
            //List<cpoe> cpoeList = orderClasses
            //        .GroupBy(temp => temp.藥袋條碼)
            //        .Select(group =>
            //        {
            //            cpoe cpoe = group.First();
            //        }).tol
            //
            //科別 = group.FirstOrDefault().科別,
            //處方 = group.Select(value =>
            //{
            //    //List<medClass> medClassList = medClassDict.SortDictionaryByCode(value.藥品碼);
            //    medClass med = medClassDict.SortDictionaryByCode(value.藥品碼).FirstOrDefault();
            //    string flag = false.ToString();
            //    if (value.醫師代碼 == value.病人姓名) flag = true.ToString();
            //    return new order
            //    {
            //        藥品名稱 = value.藥品名稱,
            //        費用別 = value.費用別,
            //        交易量 = value.交易量.Replace("-", ""),
            //        頻次 = value.頻次,
            //        天數 = value.天數,
            //        健保碼 = med.健保碼,
            //        ATC = med.ATC,
            //        藥品學名 = med.藥品學名,
            //        藥品許可證號 = med.藥品許可證號,
            //        管制級別 = med.管制級別,
            //    };

            //}).ToList()




        }
        public class identify
        {           
            [JsonPropertyName("eff_order")]
            public List<cpoe> 有效處方 { get; set; }
            [JsonPropertyName("old_order")]
            public List<cpoe> 歷史處方 { get; set; }

        }
        public class order
        {
            [JsonPropertyName("CTYPE")]
            public string 費用別 { get; set; }
            [JsonPropertyName("NAME")]
            public string 藥品名稱 { get; set; }
            [JsonPropertyName("HI_CODE")]
            public string 健保碼 { get; set; }
            [JsonPropertyName("ATC")]
            public string ATC { get; set; }
            [JsonPropertyName("LICENSE")]
            public string 藥品許可證號 { get; set; }
            [JsonPropertyName("DIANAME")]
            public string 藥品學名 { get; set; }
            [JsonPropertyName("DRUGKIND")]
            public string 管制級別 { get; set; }
            [JsonPropertyName("TXN_QTY")]
            public string 交易量 { get; set; }
            [JsonPropertyName("FREQ")]
            public string 頻次 { get; set; }
            [JsonPropertyName("DAYS")]
            public string 天數 { get; set; }
        }
        public class cpoe
        {
            [JsonPropertyName("MED_BAG_SN")]
            public string 藥袋條碼 { get; set; }
            [JsonPropertyName("DOC")]
            public string 醫師代碼 { get; set; }
            [JsonPropertyName("CT_TIME")]
            public string 產出時間 { get; set; }
            [JsonPropertyName("order")]
            public List<order> 處方 { get; set; }
            [JsonPropertyName("SECTNO")]
            public string 科別 { get; set; }
        }

    }

}
