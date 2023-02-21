using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DB2VM
{
    public class MedClass
    {
        private string _藥品碼 = "";
        private string _藥品名稱 = "";
        private string _料號 = "";
        private string _ATC主碼 = "";
        private string _藥品條碼1 = "";
        private string _藥品條碼2 = "";
        private string _藥品條碼3 = "";
        private string _藥品條碼4 = "";
        private string _藥品條碼5 = "";

        public string 藥品碼 { get => _藥品碼; set => _藥品碼 = value; }
        public string 藥品名稱 { get => _藥品名稱; set => _藥品名稱 = value; }
        public string 料號 { get => _料號; set => _料號 = value; }
        public string ATC主碼 { get => _ATC主碼; set => _ATC主碼 = value; }
        public string 藥品條碼1 { get => _藥品條碼1; set => _藥品條碼1 = value; }
        public string 藥品條碼2 { get => _藥品條碼2; set => _藥品條碼2 = value; }
        public string 藥品條碼3 { get => _藥品條碼3; set => _藥品條碼3 = value; }
        public string 藥品條碼4 { get => _藥品條碼4; set => _藥品條碼4 = value; }
        public string 藥品條碼5 { get => _藥品條碼5; set => _藥品條碼5 = value; }
    }
}
