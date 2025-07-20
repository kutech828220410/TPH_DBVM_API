using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Basic;
using HIS_DB_Lib;
using MyOffice;
using System.IO;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApi
{
    [Route("dbvm/[controller]")]
    [ApiController]
    public class suspiciousRxLog : ControllerBase
    {
        [HttpPost("download_datas_excel")]
        public async Task<ActionResult> download_datas_excel([FromBody] returnData returnData)
        {
            try
            {
                MyTimerBasic myTimerBasic = new MyTimerBasic();
                List<suspiciousRxLogClass> suspiciousRxLogClasses = returnData.Data.ObjToClass<List<suspiciousRxLogClass>>();
                List<suspiciousRxLog_excell_class> suspiciousRxLog_Excell = new List<suspiciousRxLog_excell_class>();
                foreach(var item in suspiciousRxLogClasses)
                {
                    suspiciousRxLog_excell_class suspiciousRxLog_excell_class = new suspiciousRxLog_excell_class();
                    if (item.加入時間.StringIsEmpty() == false) suspiciousRxLog_excell_class.時間戳記 = item.加入時間;
                    if (item.加入時間.StringIsEmpty() == false) suspiciousRxLog_excell_class.日期 = item.加入時間.StringToDateTime().ToDateString();
                    if (item.錯誤類別.StringIsEmpty() == false) suspiciousRxLog_excell_class.錯誤類別 = item.錯誤類別;
                    if (item.加入時間.StringIsEmpty() == false) suspiciousRxLog_excell_class.錯誤時間 = item.加入時間.StringToDateTime().ToShortTimeString();
                    if (item.醫生姓名.StringIsEmpty() == false) suspiciousRxLog_excell_class.處方醫師 = item.醫生姓名;
                    if (item.病歷號.StringIsEmpty() == false) suspiciousRxLog_excell_class.病歷號碼 = item.病歷號;
                    if (item.簡述事件.StringIsEmpty() == false) suspiciousRxLog_excell_class.簡述事件 = item.簡述事件;
                    if (item.藥袋類型.StringIsEmpty() == false) suspiciousRxLog_excell_class.處方類別 = item.藥袋類型;

                    if (item.狀態.StringIsEmpty() == false)
                    {
                        if (item.狀態 == enum_suspiciousRxLog_status.確認提報.GetEnumName()) suspiciousRxLog_excell_class.是否更改 = "是";
                        if (item.狀態 == enum_suspiciousRxLog_status.未更改.GetEnumName()) suspiciousRxLog_excell_class.是否更改 = "否";
                    }
                    if (item.提報人員.StringIsEmpty() == false) suspiciousRxLog_excell_class.提報藥師 = item.提報人員;
                    if (item.處理人員.StringIsEmpty() == false) suspiciousRxLog_excell_class.處理藥師= item.處理人員;
                    suspiciousRxLog_Excell.Add(suspiciousRxLog_excell_class);
                }
                List<object[]> list_transactionsClasses = suspiciousRxLog_Excell.ClassToSQL<suspiciousRxLog_excell_class, enum_suspiciousRxLog_excell>();
                System.Data.DataTable dataTable = list_transactionsClasses.ToDataTable(new enum_suspiciousRxLog_excell());
                //dataTable = dataTable.ReorderTable(new enum_suspiciousRxLog_export());
                string xlsx_command = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                string xls_command = "application/vnd.ms-excel";
                List<System.Data.DataTable> dataTables = new List<System.Data.DataTable>();
                dataTables.Add(dataTable);
                byte[] excelData = MyOffice.ExcelClass.NPOI_GetBytes(dataTable, Excel_Type.xlsx);
                Stream stream = new MemoryStream(excelData);
                return await Task.FromResult(File(stream, xlsx_command, $"{DateTime.Now.ToDateString("-")}_醫師處方疑義紀錄表.xlsx"));
            }
            catch
            {
                return null;
            }

        }
    }
    public enum enum_suspiciousRxLog_excell
    {
        時間戳記,
        日期,
        錯誤類別,
        處方類別,
        錯誤時間,
        處方醫師,
        病歷號碼,
        簡述事件,
        是否更改,
        提報藥師,
        處理藥師,
        通報TPR,
    }
    public class suspiciousRxLog_excell_class
    {
        public string 時間戳記 { get; set; }
        public string 日期 { get; set; }
        public string 錯誤類別 { get; set; }
        public string 處方類別 { get; set; }
        public string 錯誤時間 { get; set; }
        public string 處方醫師 { get; set; }
        public string 病歷號碼 { get; set; }
        public string 簡述事件 { get; set; }
        public string 是否更改 { get; set; }
        public string 提報藥師 { get; set; }
        public string 處理藥師 { get; set; }
        public string 通報TPR { get; set; }
    }
}
