using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HIS_DB_Lib;
using SQLUI;
using Basic;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Text.Json.Serialization;



// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApi.Controller
{
    [Route("dbvm/[controller]")]
    [ApiController]
    public class BBARTEST : ControllerBase
    {
        [HttpGet("pragnant")]
        public string pragnant(string? ID)
        {
            string conn_str = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.24.211)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=SISDCP)));User ID=tphphaadc;Password=tph@phaadc2860;";
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
