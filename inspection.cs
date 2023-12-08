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
using System.IO;
namespace DB2VM_API
{
    [Route("dbvm/[controller]")]
    [ApiController]
    public class inspection : ControllerBase
    {
        [Route("excel_upload")]
        [HttpPost]
        public async Task<string> POST_excel([FromForm] IFormFile file, [FromForm] string IC_NAME, [FromForm] string PON, [FromForm] string CT)
        {
            var formFile = Request.Form.Files.FirstOrDefault();
            string filename = formFile.FileName;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                await formFile.CopyToAsync(memoryStream);
                byte[] file_bytes = memoryStream.ToArray();
                List<string> names = new List<string>();
                List<string> values = new List<string>();
                names.Add("IC_NAME");
                names.Add("PON");
                names.Add("CT");
                values.Add(IC_NAME);
                values.Add(PON);
                values.Add(CT);


                string json_result = Basic.Net.WEBApiPost("http://192.168.23.54:4433/api/inspection/excel_upload", filename, file_bytes, names, values);
                return json_result;

            }

          

        }
    }
}
