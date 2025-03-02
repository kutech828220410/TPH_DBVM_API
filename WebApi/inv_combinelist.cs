using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using SQLUI;
using Basic;
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
using System.Text;


namespace WebApi
{
    [Route("dbvm/[controller]")]
    [ApiController]
    public class inv_combinelist : ControllerBase
    {
        /// <summary>
        /// 以合併單號取得完整合併單DataTable
        /// </summary>
        /// <remarks>
        /// [必要輸入參數說明]<br/> 
        ///  1.[returnData.Value] : 合併單號 <br/> 
        ///  --------------------------------------------<br/> 
        /// 以下為範例JSON範例
        /// <code>
        ///  {
        ///    "Value" : "I20240103-14",
        ///    "Data": 
        ///    {                 
        ///    
        ///    }
        /// }
        /// </code>
        /// </remarks>
        /// <param name="returnData">共用傳遞資料結構</param>
        /// <returns>[returnData.Data]為合併單結構</returns>
        [Route("get_full_inv_DataTable_by_SN")]
        [HttpPost]
        public string POST_get_full_inv_DataTable_by_SN([FromBody] returnData returnData)
        {
            MyTimerBasic myTimer = new MyTimerBasic();
            myTimer.StartTickTime(50000);
            returnData.Method = "get_full_inv_DataTable_by_SN";
            sys_serverSettingClass serverSettingClass = sys_serverSettingClass.get_server("http://127.0.0.1:4433", "Main", "網頁", "API01"); ;
            List<sys_serverSettingClass> serverSettingClasses = new List<sys_serverSettingClass>();
            serverSettingClasses.Add(serverSettingClass);
            serverSettingClasses = serverSettingClasses.MyFind("Main", "網頁", "API01");
            if (serverSettingClasses.Count == 0)
            {
                returnData.Code = -200;
                returnData.Result = $"找無Server資料!";
                return returnData.JsonSerializationt(true);
            }
            if (returnData.Value.StringIsEmpty() == true)
            {
                returnData.Code = -200;
                returnData.Result = "returnData.Value 空白,請輸入合併單號!";
                return returnData.JsonSerializationt(true);
            }
            string api01_url = serverSettingClasses[0].Server;
            string Server = serverSettingClasses[0].Server;
            string DB = serverSettingClasses[0].DBName;
            string UserName = serverSettingClasses[0].User;
            string Password = serverSettingClasses[0].Password;
            uint Port = (uint)serverSettingClasses[0].Port.StringToInt32();
          
        

            inv_combinelistClass inv_CombinelistClass = inv_combinelistClass.get_full_inv_by_SN("http://127.0.0.1:4433", returnData.Value);
            //inv_CombinelistClass.get_all_full_creat("http://127.0.0.1:4433");
            List<inventoryClass.creat> creats = inv_CombinelistClass.Creats;
            List<inventoryClass.content> contents = new List<inventoryClass.content>();
            List<inventoryClass.content> contents_buf = new List<inventoryClass.content>();
            List<System.Data.DataTable> dataTables_creat = new List<System.Data.DataTable>();
            List<medClass> medClasses_cloud = medClass.get_med_cloud("http://127.0.0.1:4433");
            Dictionary<string, List<medClass>> keyValuePairs_med_cloud = medClasses_cloud.CoverToDictionaryByCode();
            string 藥品碼 = "";
            string 料號 = "";
            for (int i = 0; i < creats.Count; i++)
            {
                List<object[]> list_creat_buf = new List<object[]>();
                System.Data.DataTable dataTable_buf = new System.Data.DataTable();
                for (int k = 0; k < creats[i].Contents.Count; k++)
                {
                    藥品碼 = creats[i].Contents[k].藥品碼;
                    料號 = creats[i].Contents[k].料號;
                   medClass _medClass =  medClassMethod.SerchByBarcode(medClasses_cloud, 料號);
                    if (_medClass == null) continue;
                    creats[i].Contents[k].料號 = _medClass.料號;
                    creats[i].Contents[k].藥品名稱 = _medClass.藥品名稱;
                    object[] value = new object[new enum_盤點定盤_Excel().GetLength()];
                    value[(int)enum_盤點定盤_Excel.藥碼] = creats[i].Contents[k].藥品碼;
                    value[(int)enum_盤點定盤_Excel.料號] = creats[i].Contents[k].料號;
                    value[(int)enum_盤點定盤_Excel.藥名] = creats[i].Contents[k].藥品名稱;
                    //value[(int)enum_盤點定盤_Excel.庫存量] = creats[i].Contents[k].理論值;
                    //value[(int)enum_盤點定盤_Excel.盤點量] = creats[i].Contents[k].盤點量;



                    list_creat_buf.Add(value);


                    contents_buf = (from temp in contents
                                    where temp.料號 == 料號
                                    select temp).ToList();
                    if (contents_buf.Count == 0)
                    {
                        inventoryClass.content content = creats[i].Contents[k];
                        content.GUID = "";
                        content.Master_GUID = "";
                        content.理論值 = "";
                        content.新增時間 = "";
                        content.盤點單號 = "";
                        content.Sub_content.Clear();
                        contents.Add(content);
                    }
                    else
                    {
                        contents_buf[0].盤點量 = (creats[i].Contents[k].盤點量.StringToInt32() + contents_buf[0].盤點量.StringToInt32()).ToString();
                    }
                }
                dataTable_buf = list_creat_buf.ToDataTable(new enum_盤點定盤_Excel());
                dataTable_buf.TableName = $"{i}.{creats[i].盤點名稱}";
                dataTables_creat.Add(dataTable_buf);
            }



            List<object[]> list_value = new List<object[]>();
            System.Data.DataTable dataTable;
            SheetClass sheetClass;
            Console.WriteLine($"取得creats {myTimer.ToString()}");



            for (int i = 0; i < contents.Count; i++)
            {
                bool flag_覆盤 = false;
                string 藥碼 = contents[i].藥品碼;

                object[] value = new object[new enum_盤點定盤_Excel().GetLength()];
                value[(int)enum_盤點定盤_Excel.GUID] = Guid.NewGuid().ToString();
                value[(int)enum_盤點定盤_Excel.藥碼] = contents[i].藥品碼;
                value[(int)enum_盤點定盤_Excel.料號] = contents[i].料號;
                value[(int)enum_盤點定盤_Excel.藥名] = contents[i].藥品名稱;
                value[(int)enum_盤點定盤_Excel.盤點量] = contents[i].盤點量;
                value[(int)enum_盤點定盤_Excel.單價] = "0";
                value[(int)enum_盤點定盤_Excel.誤差百分率] = "0";
                value[(int)enum_盤點定盤_Excel.消耗量] = "0";
                inv_combinelist_stock_Class inv_Combinelist_Stock_Class = inv_CombinelistClass.GetStockByCode(藥碼);
                inv_combinelist_price_Class inv_Combinelist_Price_Class = inv_CombinelistClass.GetMedPriceByCode(藥碼);
                inv_combinelist_note_Class inv_Combinelist_Note_Class = inv_CombinelistClass.GetMedNoteByCode(藥碼);
                inv_combinelist_review_Class inv_Combinelist_Review_Class = inv_CombinelistClass.GetMedReviewByCode(藥碼);
                if (inv_Combinelist_Stock_Class != null)
                {
                    value[(int)enum_盤點定盤_Excel.庫存量] = inv_Combinelist_Stock_Class.數量;
                }
                if (inv_Combinelist_Note_Class != null)
                {
                    value[(int)enum_盤點定盤_Excel.別名] = inv_Combinelist_Note_Class.備註;
                }

                if (inv_Combinelist_Review_Class != null)
                {
                    value[(int)enum_盤點定盤_Excel.覆盤量] = inv_Combinelist_Review_Class.數量;
                }

                if (inv_Combinelist_Price_Class != null)
                {
                    if (inv_Combinelist_Price_Class.單價.StringIsDouble()) value[(int)enum_盤點定盤_Excel.單價] = inv_Combinelist_Price_Class.單價;
                }
                value[(int)enum_盤點定盤_Excel.庫存金額] = value[(int)enum_盤點定盤_Excel.庫存量].StringToDouble() * value[(int)enum_盤點定盤_Excel.單價].StringToDouble();

                if (value[(int)enum_盤點定盤_Excel.覆盤量].ObjectToString().StringIsEmpty())
                {
                    value[(int)enum_盤點定盤_Excel.結存金額] = value[(int)enum_盤點定盤_Excel.盤點量].StringToDouble() * value[(int)enum_盤點定盤_Excel.單價].StringToDouble();
                    value[(int)enum_盤點定盤_Excel.誤差量] = value[(int)enum_盤點定盤_Excel.盤點量].StringToDouble() - value[(int)enum_盤點定盤_Excel.庫存量].StringToDouble();
                }
                else
                {
                    value[(int)enum_盤點定盤_Excel.結存金額] = value[(int)enum_盤點定盤_Excel.覆盤量].StringToDouble() * value[(int)enum_盤點定盤_Excel.單價].StringToDouble();
                    value[(int)enum_盤點定盤_Excel.誤差量] = value[(int)enum_盤點定盤_Excel.覆盤量].StringToDouble() - value[(int)enum_盤點定盤_Excel.庫存量].StringToDouble();
                }

                value[(int)enum_盤點定盤_Excel.誤差金額] = value[(int)enum_盤點定盤_Excel.誤差量].StringToDouble() * value[(int)enum_盤點定盤_Excel.單價].StringToDouble();

                value[(int)enum_盤點定盤_Excel.庫存金額] = value[(int)enum_盤點定盤_Excel.庫存金額].StringToDouble().ToString("0.00");
                value[(int)enum_盤點定盤_Excel.結存金額] = value[(int)enum_盤點定盤_Excel.結存金額].StringToDouble().ToString("0.00");
                value[(int)enum_盤點定盤_Excel.誤差金額] = value[(int)enum_盤點定盤_Excel.誤差金額].StringToDouble().ToString("0.00");

                if (value[(int)enum_盤點定盤_Excel.消耗量].StringToInt32() > 0)
                {
                    value[(int)enum_盤點定盤_Excel.誤差百分率] = ((value[(int)enum_盤點定盤_Excel.誤差量].StringToDouble() / value[(int)enum_盤點定盤_Excel.消耗量].StringToDouble()) * 100).ToString("0.00");
                }

                if (inv_CombinelistClass.誤差總金額致能.StringToBool())
                {
                    double 上限 = inv_CombinelistClass.誤差總金額上限.StringToDouble();
                    double 下限 = inv_CombinelistClass.誤差總金額下限.StringToDouble();
                    double temp = value[(int)enum_盤點定盤_Excel.誤差金額].StringToDouble();
                    if (temp < 下限 || temp > 上限)
                    {
                        flag_覆盤 = true;
                    }
                }

                if (inv_CombinelistClass.誤差數量致能.StringToBool())
                {
                    double 上限 = inv_CombinelistClass.誤差數量上限.StringToDouble();
                    double 下限 = inv_CombinelistClass.誤差數量下限.StringToDouble();
                    double temp = value[(int)enum_盤點定盤_Excel.誤差量].StringToDouble();
                    if (temp < 下限 || temp > 上限)
                    {
                        flag_覆盤 = true;
                    }
                }
                if (inv_CombinelistClass.誤差百分率致能.StringToBool())
                {
                    double 上限 = inv_CombinelistClass.誤差百分率上限.StringToDouble();
                    double 下限 = inv_CombinelistClass.誤差百分率下限.StringToDouble();
                    double temp = value[(int)enum_盤點定盤_Excel.誤差百分率].StringToDouble();
                    if (temp < 下限 || temp > 上限)
                    {
                        flag_覆盤 = true;
                    }
                }
                if (flag_覆盤) value[(int)enum_盤點定盤_Excel.註記] = "覆盤";
                list_value.Add(value);
            }
            List<System.Data.DataTable> dataTables = new List<System.Data.DataTable>();
            dataTable = list_value.ToDataTable(new enum_盤點定盤_Excel());
            dataTable.TableName = "盤點總表";
            dataTables.Add(dataTable);


            for (int i = 0; i < dataTables_creat.Count; i++)
            {
                dataTables.Add(dataTables_creat[i]);
            }
            if (returnData.ValueAry != null)
            {
                for (int i = 0; i < returnData.ValueAry.Count; i++)
                {
                    foreach (System.Data.DataTable dt in dataTables)
                    {
                        dt.Columns.Remove(returnData.ValueAry[i]);
                    }
                }
            }
            returnData.Code = 200;
            returnData.Data = dataTables.JsonSerializeDataTable();
            returnData.TimeTaken = myTimer.ToString();
            returnData.Result = $"成功轉換表單<{dataTables.Count}>張";
            return returnData.JsonSerializationt();
        }
    }
}
