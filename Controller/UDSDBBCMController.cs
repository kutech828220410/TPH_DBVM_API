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

namespace DB2VM.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class UDSDBBCMController : ControllerBase
    {
        string DB2_server = $"{ConfigurationManager.AppSettings["DB2_server"]}:{ConfigurationManager.AppSettings["DB2_port"]}";
        string DB2_database = $"{ConfigurationManager.AppSettings["DB2_database"]}";
        string DB2_userid = $"{ConfigurationManager.AppSettings["DB2_user"]}";
        string DB2_password = $"{ConfigurationManager.AppSettings["DB2_password"]}";
        string DB2_schema = $"{ConfigurationManager.AppSettings["DB2_schema"]}";
        [HttpGet]
        public string Get(string? Code)
        {
            if (Code.StringIsEmpty()) Code = "";
            String MyDb2ConnectionString = $"server={DB2_server};database={DB2_database};userid={DB2_userid};password={DB2_password};";
            DB2Connection MyDb2Connection = new DB2Connection(MyDb2ConnectionString);

            MyDb2Connection.Open();

            String procName = $"{DB2_schema}.UDSDBBCM";
            DB2Command cmd = new DB2Command();
            cmd.Connection = MyDb2Connection;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = procName;
            //cmd.Transaction = trans;
            cmd.Parameters.Add("@TSYSD", DB2Type.VarChar, 5).Value = "ADMUD";
            cmd.Parameters.Add("@TUDDRGNO", DB2Type.VarChar, 5).Value = "";

            DB2Parameter RET = cmd.Parameters.Add("@RET", DB2Type.Integer);
            RET.Direction = ParameterDirection.Output;

            DB2Parameter RETMSG = cmd.Parameters.Add("@RETMSG", DB2Type.VarChar, 200);
            RETMSG.Direction = ParameterDirection.Output;
            var reader = cmd.ExecuteReader();
            List<MedClass> medClasses = new List<MedClass>();
            while (reader.Read())
            {
                MedClass medClass = new MedClass();
                medClass.藥品碼 = reader["UDDRGNO"].ToString().Trim();
                medClass.藥品名稱 = reader["UDARNAME"].ToString().Trim();
                medClass.料號 = reader["UDSTOKNO"].ToString().Trim();
                medClass.ATC主碼 = reader["UDATC"].ToString().Trim();
                medClass.藥品條碼1 = reader["UDBARCD1"].ToString().Trim();
                medClass.藥品條碼2 = reader["UDBARCD2"].ToString().Trim();
                medClass.藥品條碼3 = reader["UDBARCD3"].ToString().Trim();
                medClass.藥品條碼4 = reader["UDBARCD4"].ToString().Trim();
                medClass.藥品條碼5 = reader["UDBARCD5"].ToString().Trim();


                medClasses.Add(medClass);
            }

            MyDb2Connection.Close();
            if (medClasses.Count == 0) return null;
            string jsonString = medClasses.JsonSerializationt();
            return jsonString;
        }
    }
}
