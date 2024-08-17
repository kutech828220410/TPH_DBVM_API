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
using Oracle.ManagedDataAccess.Client;
using HIS_DB_Lib;
using SQLUI;
namespace WebApi
{
    public class v_misadc
    {
        public string 料號 = "";
        public string 庫別 = "";
        public string 庫存 = "";
    }

    [Route("dbvm/[controller]")]
    [ApiController]
    public class MIS_Stock : ControllerBase
    {
        static private string API_Server = "http://127.0.0.1:4433";
        [Route("get_all")]
        [HttpPost]
        public string POST_get_all([FromBody] returnData returnData)
        {
            MyTimerBasic myTimer = new MyTimerBasic();
            myTimer.StartTickTime(50000);
            returnData.Method = "get_all";
            string MyDb2ConnectionString = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.24.211)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=SISDCP)));User ID=tphphaadc;Password=tph@phaadc2860;";
            OracleConnection conn_oracle = new OracleConnection(MyDb2ConnectionString);
            OracleDataReader reader;
            OracleCommand cmd;

            try
            {
                conn_oracle.Open();

                List<medClass> medClasses = medClass.get_med_cloud(API_Server);
                List<medClass> medClasses_buf = new List<medClass>();
                cmd = new OracleCommand("select * from v_misadc", conn_oracle);
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    v_misadc v_Misadc = new v_misadc();
                    v_Misadc.料號 = reader["MBT_SKDIACODE"].ToString().Trim();
                    v_Misadc.庫別 = reader["MBT_DPTCODE"].ToString().Trim();
                    v_Misadc.庫存 = reader["MBT_AMT"].ToString().Trim();

                    medClasses_buf = (from temp in medClasses
                                      where temp.料號 == v_Misadc.料號
                                      select temp).ToList();
                    if(medClasses_buf.Count > 0)
                    {
                        if (v_Misadc.庫別 == "8501")
                        {
                            medClasses_buf[0].藥庫庫存 = v_Misadc.庫存;
                        }
                        else
                        {
                            
                        }
                        if (v_Misadc.庫別 == "8503")
                        {
                            medClasses_buf[0].藥局庫存 = v_Misadc.庫存;
                        }
                    }

                }
                conn_oracle.Close();
                conn_oracle.Dispose();

                returnData.Code = 200;
                returnData.Result = $"取得MIS庫存成功共<{medClasses.Count}>筆";
                returnData.Data = medClasses;
                returnData.TimeTaken = $"{myTimer}";
                return returnData.JsonSerializationt(true);
            }
            catch (Exception e)
            {
                returnData.Code = -200;
                returnData.Result = e.Message;
                return returnData.JsonSerializationt(true);

            }
        }
    }
}
