﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using SQLUI;
using Basic;
using System.Drawing;
using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Configuration;
using MyOffice;
using NPOI;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using HIS_DB_Lib;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace WebApi
{
    public enum enum_drugStotreDistribution_Excel_exprot
    {
        物品代碼,
        撥補數量,
    }
    [Route("dbvm/[controller]")]
    [ApiController]
    public class drugStotreDistribution : Controller
    { 
       /// <summary>
       /// 以請領時間範圍下載撥補單
       /// </summary>
       /// <remarks>
       /// 以下為範例JSON範例
       /// <code>
       ///   {
       ///     "Data": 
       ///     {
       ///     
       ///     },
       ///     "ValueAry" : 
       ///     [
       ///        "起始時間",
       ///        "結束時間"
       ///     ]
       ///   }
       /// </code>
       /// </remarks>
       /// <param name="returnData">共用傳遞資料結構</param>
       /// <returns>Excel</returns>
        [Route("download_excel_by_addTime")]
        [HttpPost]
        public async Task<ActionResult> download_excel_by_addTime([FromBody] returnData returnData)
        {
            MyTimerBasic myTimerBasic = new MyTimerBasic();
            myTimerBasic.StartTickTime(50000);
            returnData.Method = "download_excel_by_addTime";
            try
            {
                if (returnData.ValueAry.Count != 2)
                {
                    returnData.Code = -200;
                    returnData.Result = $"returnData.ValueAry 內容應為[起始時間][結束時間]";
                    return null;
                }
                string 起始時間 = returnData.ValueAry[0];
                string 結束時間 = returnData.ValueAry[1];
                if (起始時間.Check_Date_String() == false || 結束時間.Check_Date_String() == false)
                {
                    returnData.Code = -200;
                    returnData.Result = $"時間範圍格式錯誤";
                    return null;
                }
                DateTime dateTime_st = 起始時間.StringToDateTime();
                DateTime dateTime_end = 結束時間.StringToDateTime();
                List<medClass> med_cloud = medClass.get_med_cloud("http://127.0.0.1:4433");
                List<medClass> med_cloud_buf = new List<medClass>();
                Dictionary<string, List<medClass>> keyValuePairs_cloud = med_cloud.CoverToDictionaryByCode();
                List<drugStotreDistributionClass> drugStotreDistributionClasses = drugStotreDistributionClass.get_by_addedTime("http://127.0.0.1:4433", dateTime_st, dateTime_end);
                List<object[]> list_drugStotreDistributionClasses = new List<object[]>();
                for (int i = 0; i < drugStotreDistributionClasses.Count; i++)
                {
                    object[] value = new object[new enum_drugStotreDistribution_Excel_exprot().GetLength()];
                    med_cloud_buf = keyValuePairs_cloud.SortDictionaryByCode(drugStotreDistributionClasses[i].藥碼);
                    value[(int)enum_drugStotreDistribution_Excel_exprot.物品代碼] = drugStotreDistributionClasses[i].藥碼;
                    if (med_cloud_buf.Count > 0)
                    {
                        value[(int)enum_drugStotreDistribution_Excel_exprot.物品代碼] = med_cloud_buf[0].料號;
                    }

                    value[(int)enum_drugStotreDistribution_Excel_exprot.撥補數量] = drugStotreDistributionClasses[i].實撥量;
                    list_drugStotreDistributionClasses.Add(value);
                }

                System.Data.DataTable dataTable = list_drugStotreDistributionClasses.ToDataTable(new enum_drugStotreDistribution_Excel_exprot());
                string xlsx_command = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                string xls_command = "application/vnd.ms-excel";

                byte[] excelData = dataTable.NPOI_GetBytes(Excel_Type.xlsx, new[] { (int)enum_drugStotreDistribution_Excel_exprot.撥補數量 });
                Stream stream = new MemoryStream(excelData);
                return await Task.FromResult(File(stream, xlsx_command, $"{DateTime.Now.ToDateString("-")}_申領明細.xlsx"));
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {

            }

        }
    }
}