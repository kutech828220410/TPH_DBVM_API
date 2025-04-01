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

            List<medClass> medClasses_his = new List<medClass>();
            while (reader.Read())
            {
                object[] value = new object[new enum_雲端藥檔().GetLength()];

                medClass medClass = new medClass();

                medClass.藥品碼 = reader["DIA_DIACODE"].ToString().Trim();
                medClass.料號 = reader["DIA_SKDIACODE"].ToString().Trim();
                medClass.中文名稱 = ConvertToBig5(reader["DIA_CNAME"].ToString().Trim());          
                medClass.藥品名稱 = reader["DIA_EGNAME"].ToString().Trim();
                medClass.藥品學名 = reader["DIA_CHNAME"].ToString().Trim();
                medClass.健保碼 = reader["DIA_INSCODE"].ToString().Trim();
                medClass.包裝單位 = reader["DIA_ATTACHUNIT"].ToString().Trim();
                medClass.最小包裝單位 = reader["DIA_UNIT"].ToString().Trim();
                medClass.警訊藥品 = (reader["MED_HWARNING"].ToString().Trim() == "Y") ? "True" : "False";
                medClass.管制級別 = reader["DIA_RESTRIC"].ToString().Trim();
                medClass.類別 = reader["DIA_DRUGKINDNAME"].ToString().Trim();
                medClass.ATC = reader["DIA_ATCCODE"].ToString().Trim();
                medClass.懷孕用藥級別 = reader["MED_PREGNANCY"].ToString().Trim();
                if(medClass.類別 == "中醫飲片" || medClass.類別 == "中藥" || medClass.類別 == "中藥錠劑" || medClass.類別 == "外用中藥")
                {
                    medClass.中西藥 = "中藥";
                }
                else
                {
                    medClass.中西藥 = "西藥";
                }
                medClass.包裝數量 = "1";
                medClass.最小包裝數量 = "1";

                medClasses_his.Add(medClass);

            }
            cmd.Dispose();
            conn_oracle.Close();
            conn_oracle.Dispose();

            List<medClass> medClasses_cloud = medClass.get_med_cloud("http://127.0.0.1:4433");
            List<medClass> medClasses_add = new List<medClass>();
            List<medClass> medClasses_replace = new List<medClass>();

            Dictionary<string, List<medClass>> keyValuePairs_med_cloud = medClasses_cloud.CoverToDictionaryByCode();


            List<object[]> list_BBCM_Add = new List<object[]>();
            List<object[]> list_BBCM_Replace = new List<object[]>();
            for (int i = 0; i < medClasses_his.Count; i++)
            {
                string code = medClasses_his[i].藥品碼;

                List<medClass> medClasses_cloud_buf = keyValuePairs_med_cloud.SortDictionaryByCode(code);

                if (medClasses_cloud_buf.Count == 0)
                {
                    medClasses_his[i].GUID = Guid.NewGuid().ToString();
                    medClasses_add.Add(medClasses_his[i]);
                }
                else
                {
                    medClasses_his[i].GUID = medClasses_cloud_buf[0].GUID;

                    object[] value_his = medClasses_his[i].ClassToSQL<medClass, enum_雲端藥檔>();
                    object[] value_cloud = medClasses_cloud_buf[0].ClassToSQL<medClass, enum_雲端藥檔>();

                    if (value_his.IsEqual(value_cloud, (int)enum_雲端藥檔.藥品條碼1, (int)enum_雲端藥檔.藥品條碼2, (int)enum_雲端藥檔.開檔狀態) == false)
                    {
                        medClasses_his[i].藥品條碼1 = medClasses_cloud_buf[0].藥品條碼1;
                        medClasses_his[i].藥品條碼2 = medClasses_cloud_buf[0].藥品條碼2;
                        medClasses_his[i].開檔狀態 = medClasses_cloud_buf[0].開檔狀態;
                        medClasses_replace.Add(medClasses_his[i]);
                    }


                }
            }

            list_BBCM_Add = medClasses_add.ClassToSQL<medClass, enum_雲端藥檔>();
            list_BBCM_Replace = medClasses_replace.ClassToSQL<medClass, enum_雲端藥檔>();
            if (list_BBCM_Add.Count > 0) sQLControl_UDSDBBCM.AddRows(null, list_BBCM_Add);
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

        /// <summary>
        /// 將輸入的字串由預設編碼轉換為 BIG5 編碼的字串
        /// </summary>
        /// <param name="input">欲轉碼的字串</param>
        /// <returns>轉換後的 BIG5 編碼字串</returns>
        public static string ConvertToBig5(string input)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            // 將字串轉換為位元組陣列
            byte[] bytes = Encoding.Default.GetBytes(input);
            // 將位元組由預設編碼轉換為 BIG5 編碼
            byte[] big5Bytes = Encoding.Convert(Encoding.Default, Encoding.GetEncoding("BIG5"), bytes);
            // 取得轉換後的字串

            return Encoding.GetEncoding("BIG5").GetString(big5Bytes);
        }
    }
}
