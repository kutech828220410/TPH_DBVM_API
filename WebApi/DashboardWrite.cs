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

namespace WebApi
{
    public class Dashboard
    {
        public string PCK_VISITDT = "";
        public string PCK_PATID = "";
        public string PCK_SEQ = "";
        public string PCK_DIVNO = "";
        public string PCK_TYPE = "";
        public string PCK_DRUGNO = "";
        public string PCK_PROCDTTM = "";
        public string PCK_PROCOPID = "";
        public string PCK_CREATEDTTM = "";
    }
    [Route("dbvm/[controller]")]
    [ApiController]
    public class DashboardWrite : ControllerBase
    {
        /// <summary>
        /// 戰情回寫
        /// </summary>
        /// <remarks>
        ///  --------------------------------------------<br/> 
        /// 以下為範例JSON範例
        /// <code>
        ///   {
        ///     "Data": 
        ///     {
        ///        [OrderClass Ary]
        ///     }
        ///   }
        /// </code>
        /// </remarks>
        /// <param name="returnData">共用傳遞資料結構</param>
        /// <returns>[returnData.Data]</returns>
        [Route("write")]
        [HttpPost]
        public string POST_write([FromBody] returnData returnData)
        {

            MyTimerBasic myTimerBasic = new MyTimerBasic();
            returnData.Method = "write";
            try
            {
                           
                List<OrderClass> orderClasses = returnData.Data.ObjToClass<List<OrderClass>>();
                List<OrderClass> orderClasses_buf = new List<OrderClass>();
                if (orderClasses == null)
                {
                    returnData.Code = -200;
                    returnData.Result = $"傳入資料異常!";
                    return returnData.JsonSerializationt();
                }

                List<Dashboard> dashboards = new List<Dashboard>();
                for (int i = 0; i < orderClasses.Count; i++)
                {
                    if (orderClasses[i].藥師ID.StringIsEmpty()) continue;
                    Dashboard dashboard = new Dashboard();

                    dashboard.PCK_VISITDT = orderClasses[i].就醫時間.StringToDateTime().ToDateTinyString();
                    dashboard.PCK_PATID = orderClasses[i].病歷號;
                    dashboard.PCK_SEQ = orderClasses[i].住院序號;
                    dashboard.PCK_DIVNO = "0";
                    dashboard.PCK_TYPE = "1";
                    dashboard.PCK_DRUGNO = orderClasses[i].領藥號;
                    dashboard.PCK_PROCDTTM = orderClasses[i].開方日期.StringToDateTime().ToDateTimeTiny(TypeConvert.Enum_Year_Type.Anno_Domini);
                    dashboard.PCK_PROCOPID = orderClasses[i].藥師ID;
                    dashboard.PCK_CREATEDTTM = DateTime.Now.ToDateTimeTiny(TypeConvert.Enum_Year_Type.Anno_Domini);
                    dashboards.Add(dashboard);
                }

                string conn_str = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.24.211)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=SISDCP)));User ID=tphphaadc;Password=tph@phaadc2860;";
                OracleConnection conn_oracle;
                OracleDataReader reader;
                string commandText = "INSERT INTO PHACHK (PCK_VISITDT, PCK_PATID ,PCK_SEQ ,PCK_DIVNO ,PCK_TYPE ,PCK_DRUGNO ,PCK_PROCDTTM ,PCK_PROCOPID ,PCK_CREATEDTTM) VALUES " +
                    "(:PCK_VISITDT, :PCK_PATID ,:PCK_SEQ ,:PCK_DIVNO ,:PCK_TYPE ,:PCK_DRUGNO ,:PCK_PROCDTTM ,:PCK_PROCOPID ,:PCK_CREATEDTTM)";

                conn_oracle = new OracleConnection(conn_str);
                conn_oracle.Open();

                for (int i = 0; i < dashboards.Count; i++)
                {
                    using (OracleCommand cmd = new OracleCommand(commandText, conn_oracle))
                    {
                        // 添加参数并赋值
                        cmd.Parameters.Add(new OracleParameter("PCK_VISITDT", dashboards[i].PCK_VISITDT));
                        cmd.Parameters.Add(new OracleParameter("PCK_PATID", dashboards[i].PCK_PATID));
                        cmd.Parameters.Add(new OracleParameter("PCK_SEQ", dashboards[i].PCK_SEQ));
                        cmd.Parameters.Add(new OracleParameter("PCK_DIVNO", dashboards[i].PCK_DIVNO));
                        cmd.Parameters.Add(new OracleParameter("PCK_TYPE", dashboards[i].PCK_TYPE));
                        cmd.Parameters.Add(new OracleParameter("PCK_DRUGNO", dashboards[i].PCK_DRUGNO));
                        cmd.Parameters.Add(new OracleParameter("PCK_PROCDTTM", dashboards[i].PCK_PROCDTTM));
                        cmd.Parameters.Add(new OracleParameter("PCK_PROCOPID", dashboards[i].PCK_PROCOPID));
                        cmd.Parameters.Add(new OracleParameter("PCK_CREATEDTTM", dashboards[i].PCK_CREATEDTTM));

                        // 执行插入操作
                        int rowsInserted = cmd.ExecuteNonQuery();
                        Console.WriteLine($"{rowsInserted} row(s) inserted");
                    }
                }
               


                returnData.Code = 200;
                returnData.Data = "";
                returnData.Result = $"寫入戰情資料共<{dashboards.Count}>筆";
                returnData.TimeTaken = myTimerBasic.ToString();
                string json = returnData.JsonSerializationt();
                Logger.Log("DashboardWrite", json);
                return json;
            }
            catch (Exception e)
            {
                returnData.Code = -200;
                returnData.Result = e.Message;
                returnData.Data = "";
                string json = returnData.JsonSerializationt();
                Logger.Log("DashboardWrite", json);
                return json;

            }
        }
    }
}
