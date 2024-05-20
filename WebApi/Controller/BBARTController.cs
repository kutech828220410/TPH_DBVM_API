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
    public class BBARTController : ControllerBase
    {
        public enum enum_中藥藥袋
        {
            領藥號,
            開方日期,
            病歷號,
            批序,
            藥碼,
            頻次,
        }
    
        [HttpGet]
        public string Get(string BarCode)
        {
            MyTimerBasic myTimerBasic = new MyTimerBasic();
            returnData returnData = new returnData();
            try
            {
                SQLControl sQLControl_醫囑資料 = new SQLControl("127.0.0.1", "dbvm", "ordert_list", "user", "66437068", 3306, MySql.Data.MySqlClient.MySqlSslMode.None);
                string[] str_ary = BarCode.Split(";");
                if(str_ary.Length < 5)
                {
                    returnData.Code = -200;
                    returnData.Result = $"BarCode:{BarCode},傳入資料異常";
                    return returnData.JsonSerializationt(true);
                }
                string 領藥號 = str_ary[(int)enum_中藥藥袋.領藥號];
                string 開方日期 = str_ary[(int)enum_中藥藥袋.開方日期];
                if (開方日期.Length != 8)
                {
                    returnData.Code = -200;
                    returnData.Result = $"開方日期:{開方日期},傳入資料異常";
                    return returnData.JsonSerializationt(true);
                }
                開方日期 = $"{開方日期.Substring(0, 4)}-{開方日期.Substring(4, 2)}-{開方日期.Substring(6, 2)}";
                if (開方日期.Check_Date_String() == false)
                {
                    returnData.Code = -200;
                    returnData.Result = $"開方日期:{開方日期},傳入資料異常";
                    return returnData.JsonSerializationt(true);
                }
                string 病歷號 = str_ary[(int)enum_中藥藥袋.病歷號];
                string[] serchColNames = new string[] { "領藥號", "病歷號" };
                string[] serchValues = new string[] { 領藥號, 病歷號 };
                List<object[]> list_醫囑資料 = sQLControl_醫囑資料.GetRowsByDefult(null, serchColNames, serchValues);

                list_醫囑資料 = list_醫囑資料.GetRowsInDate((int)HIS_DB_Lib.enum_OrderT.開方日期, 開方日期.StringToDateTime());

                List<OrderTClass> orderTClasses = list_醫囑資料.SQLToClass<OrderTClass, enum_OrderT>();

                returnData.Data = orderTClasses;
                returnData.TimeTaken = $"{myTimerBasic}";
                returnData.Code = 200;
                returnData.Result = $"已搜尋到醫令共<{orderTClasses.Count}>筆";
                return returnData.JsonSerializationt();
            }
            catch (Exception e)
            {
                returnData.Code = -200;
                returnData.Result = $"Exception : {e.Message}";
                return returnData.JsonSerializationt(true);
            }
            finally
            {
            
            }
        }
    }
}
