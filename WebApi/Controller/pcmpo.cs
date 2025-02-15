using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SQLUI;
using Basic;
using HIS_DB_Lib;
using System.Globalization;
using MySql.Data.MySqlClient;




namespace WebApi.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class pcmpo : ControllerBase
    {
        static private MySqlSslMode SSLMode = MySqlSslMode.None;
        static private string API = "http://192.168.23.54:4433";
        static private string API_AI = "http://192.168.5.210:3100/po_vision";
        private static string project = "PO_vision";
        private static string Message = "---------------------------------------------------------------------------";
        /// <summary>
        /// 執行文字辨識
        /// </summary>
        /// <remarks>
        /// 以下為JSON範例
        /// <code>
        ///     {
        ///         "ValueAry":
        ///         [
        ///             "GUID":
        ///         ]
        ///         
        ///     }
        /// </code>
        /// </remarks>
        /// <param name="returnData">共用傳遞資料結構</param>
        /// <returns></returns>
        [HttpPost("analyze")]
        public string analyze([FromBody] returnData returnData)
        {
            try
            {
                if (returnData.ValueAry.Count == 0)
                {
                    returnData.Result = "returnData.ValueAry無傳入資料";
                    returnData.Code = -200;
                    return returnData.JsonSerializationt(true);
                }
                if (returnData.ValueAry.Count != 1)
                {
                    returnData.Result = "returnData.ValueAry應為[\"GUID\"]";
                    returnData.Code = -200;
                    return returnData.JsonSerializationt(true);
                }
                string GUID = returnData.ValueAry[0];
                returnData returnData_AI = textVisionClass.analyze(API, GUID, "Y");
                textVisionClass out_textVisionClasses = returnData_AI.Data.ObjToClass<textVisionClass>();
                if(out_textVisionClasses == null)
                {
                    returnData.Code = -200;
                    returnData.Result = $"AI辨識未完成";
                    return returnData.JsonSerializationt(true);
                }
                if (out_textVisionClasses.單號.StringIsEmpty())
                {
                    returnData.Code = -200;
                    returnData.Result = $"無單號";
                    return returnData_AI.JsonSerializationt(true);
                }
                out_textVisionClasses.單號 = FixOrderNumber(out_textVisionClasses.單號);
                returnData returnData_poNum = textVisionClass.analyze_by_po_num(API, out_textVisionClasses);
                return returnData_poNum.JsonSerializationt(true);


            }
            catch (Exception ex)
            {
                returnData.Code = -200;
                returnData.Result = $"Exception : {ex.Message}";
                Logger.Log(project, returnData.JsonSerializationt());
                Logger.Log(project, Message);
                return returnData.JsonSerializationt(true);
            }
        }

        private (string width, string height, string center) GetSquare(string[] position)
        {
            int xMax = position[2].Split(",")[0].StringToInt32();
            int yMax = position[2].Split(",")[1].StringToInt32();

            int xMin = position[0].Split(",")[0].StringToInt32();
            int yMin = position[0].Split(",")[1].StringToInt32();

            string width = (xMax - xMin).ToString();
            string height = (yMax - yMin).ToString();

            double centerX = (xMax + xMin) / 2.0;
            double centerY = (yMax + yMin) / 2.0;

            return (width, height, $"{centerX},{centerY}");
        }
        private positionClass GetPosition(string 位置, string 信心分數, string keyword)
        {
            string[] position = 位置.Split(";");
            (string width, string height, string center) = GetSquare(position);
            positionClass positionClass = new positionClass
            {
                高 = height,
                寬 = width,
                中心 = center,
                信心分數 = 信心分數,
                keyWord = keyword,
            };
            return positionClass;
        }
        private string FixOrderNumber(string orderNumber)
        {
            var parts = orderNumber.Split('-');
            if (parts.Length < 2) return orderNumber;

            string mainPart = parts[0];
            string lastPart = int.Parse(parts[1]).ToString(); // 確保兩位數格式

            return $"{mainPart}-{lastPart}";
        }
        
    }

}


