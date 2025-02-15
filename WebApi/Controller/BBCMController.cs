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
using System.Text;
using HIS_DB_Lib;
namespace DB2VM.Controller
{
   

    [Route("dbvm/[controller]")]
    [ApiController]
    public class BBCMController : ControllerBase
    {
        private SQLControl sQLControl_UDSDBBCM = new SQLControl(MySQL_server, MySQL_database, "medicine_page_cloud", MySQL_userid, MySQL_password, (uint)MySQL_port.StringToInt32(), MySql.Data.MySqlClient.MySqlSslMode.None);

        static string MySQL_server = $"{ConfigurationManager.AppSettings["MySQL_server"]}";
        static string MySQL_database = $"{ConfigurationManager.AppSettings["MySQL_database"]}";
        static string MySQL_userid = $"{ConfigurationManager.AppSettings["MySQL_user"]}";
        static string MySQL_password = $"{ConfigurationManager.AppSettings["MySQL_password"]}";
        static string MySQL_port = $"{ConfigurationManager.AppSettings["MySQL_port"]}";
        [HttpGet]
        public string Get(string? Code)
        {
            string conn_str = "Data Source=192.168.24.211:1521/SISDCP;User ID=tphphaadc;Password=tph@phaadc2860;";
            OracleConnection conn_oracle = new OracleConnection(conn_str);
            conn_oracle.Open();
            string commandText = "";
            if (Code.StringIsEmpty()) commandText = $"select * from v_hisdrugdia";
            else commandText = $"select * from v_hisdrugdia where DIA_DIACODE='{Code}'";
            OracleCommand cmd = new OracleCommand(commandText, conn_oracle);
            List<object[]> list_v_hisdrugdia = new List<object[]>();
            var reader = cmd.ExecuteReader();
            List<string> columnNames = new List<string>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                string columnName = reader.GetName(i);
                columnNames.Add(columnName);
            }

            while (reader.Read())
            {
                object[] value = new object[new enum_雲端藥檔().GetLength()];

                value[(int)enum_雲端藥檔.藥品碼] = reader["DIA_DIACODE"].ToString().Trim();
                value[(int)enum_雲端藥檔.料號] = reader["DIA_SKDIACODE"].ToString().Trim();
                value[(int)enum_雲端藥檔.中文名稱] = reader["DIA_CNAME"].ToString().Trim();
                value[(int)enum_雲端藥檔.藥品名稱] = reader["DIA_EGNAME"].ToString().Trim();
                value[(int)enum_雲端藥檔.藥品學名] = reader["DIA_CHNAME"].ToString().Trim();
                value[(int)enum_雲端藥檔.健保碼] = reader["DIA_INSCODE"].ToString().Trim();
                value[(int)enum_雲端藥檔.包裝單位] = reader["DIA_ATTACHUNIT"].ToString().Trim();
                value[(int)enum_雲端藥檔.包裝數量] = "1";
                value[(int)enum_雲端藥檔.最小包裝單位] = reader["DIA_UNIT"].ToString().Trim();
                value[(int)enum_雲端藥檔.最小包裝數量] = "1";
                //value[(int)enum_雲端藥檔.藥品條碼1] = reader["DIA_SKDIACODE"].ToString().Trim();
                //value[(int)enum_雲端藥檔.藥品條碼2] = reader["DIA_SKDIACODE"].ToString().Trim();
                value[(int)enum_雲端藥檔.警訊藥品] = (reader["MED_HWARNING"].ToString().Trim() == "Y") ? "True" : "False";
                value[(int)enum_雲端藥檔.管制級別] = reader["DIA_RESTRIC"].ToString().Trim();
                value[(int)enum_雲端藥檔.類別] = reader["DIA_DRUGKINDNAME"].ToString().Trim();
                list_v_hisdrugdia.Add(value);
            }
            cmd.Dispose();
            conn_oracle.Close();
            conn_oracle.Dispose();
            medClass _medClass = medClass.get_med_clouds_by_code("http://127.0.0.1:4433", Code);
            List<object[]> list_BBCM = sQLControl_UDSDBBCM.GetAllRows(null);
            List<object[]> list_BBCM_buf = new List<object[]>();
            List<object[]> list_BBCM_Add = new List<object[]>();
            List<object[]> list_BBCM_Replace = new List<object[]>();
            for (int i = 0; i < list_v_hisdrugdia.Count; i++)
            {
                list_BBCM_buf = list_BBCM.GetRows((int)enum_雲端藥檔.藥品碼, list_v_hisdrugdia[i][(int)enum_雲端藥檔.藥品碼].ObjectToString());
                if (list_BBCM_buf.Count == 0)
                {
                    list_v_hisdrugdia[i][(int)enum_雲端藥檔.GUID] = Guid.NewGuid().ToString();
                    list_BBCM_Add.Add(list_v_hisdrugdia[i]);
                }
                else
                {

                    list_v_hisdrugdia[i][(int)enum_雲端藥檔.GUID] = list_BBCM_buf[0][(int)enum_雲端藥檔.GUID].ObjectToString();
                    if (!list_v_hisdrugdia[i].IsEqual(list_BBCM_buf[0], (int)enum_雲端藥檔.藥品條碼1, (int)enum_雲端藥檔.藥品條碼2))
                    {
                        list_v_hisdrugdia[i][(int)enum_雲端藥檔.藥品條碼1] = list_BBCM_buf[0][(int)enum_雲端藥檔.藥品條碼1];
                        list_v_hisdrugdia[i][(int)enum_雲端藥檔.藥品條碼2] = list_BBCM_buf[0][(int)enum_雲端藥檔.藥品條碼2];
                        list_BBCM_Replace.Add(list_v_hisdrugdia[i]);
                    }


                }
            }
            if (list_BBCM_Add.Count > 0) sQLControl_UDSDBBCM.AddRows(null, list_BBCM_Add);
            for (int i = 0; i < list_BBCM_Replace.Count; i++)
            {
                string str = list_BBCM_Replace[i][(int)enum_雲端藥檔.中文名稱].ObjectToString();
                byte[] bytes = Encoding.Default.GetBytes(str);
                byte[] BIG5 = Encoding.Convert(Encoding.Default, Encoding.GetEncoding("BIG5"), bytes);//進行轉碼,參數1,來源編碼,參數二,目標編碼,參數三,欲編碼變
                string decodedString = Encoding.GetEncoding("BIG5").GetString(BIG5);
                list_BBCM_Replace[i][(int)enum_雲端藥檔.中文名稱] = decodedString;
            }
            if (list_BBCM_Replace.Count > 0) sQLControl_UDSDBBCM.UpdateByDefulteExtra(null, list_BBCM_Replace);


            return $"新增<{list_BBCM_Add.Count}>筆資料,修改<{list_BBCM_Replace.Count}>筆資料";
        }
        [HttpGet("getMed")]
        public string getMed(string ? Code)
        {
            string conn_str = "Data Source=192.168.24.211:1521/SISDCP;User ID=tphphaadc;Password=tph@phaadc2860;";
            OracleConnection conn_oracle = new OracleConnection(conn_str);
            conn_oracle.Open();
            string commandText = "";
            if (Code.StringIsEmpty()) commandText = $"select * from v_hisdrugdia";
            else commandText = $"select * from v_hisdrugdia where DIA_DIACODE='{Code}'";
            OracleCommand cmd = new OracleCommand(commandText, conn_oracle);
            List<object[]> list_v_hisdrugdia = new List<object[]>();
            var reader = cmd.ExecuteReader();
            List<string> columnNames = new List<string>();
            List<string> rowvalues = new List<string>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                string columnName = reader.GetName(i);
                columnNames.Add(columnName);
            }

            while (reader.Read())
            {
                for(int i = 0; i < columnNames.Count; i++)
                {
                    string rowvalue = reader[columnNames[i]].ToString().Trim();
                    rowvalues.Add(rowvalue);
                };
              
            }
            cmd.Dispose();
            conn_oracle.Close();
            conn_oracle.Dispose();
            return rowvalues.JsonSerializationt(true);
        }
    }
}
