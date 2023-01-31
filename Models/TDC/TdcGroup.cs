using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Honeywell.TDC
{
    public class TdcGroup
    {
        public string EbFile { get; set; }
        public int OENTNUMP { get; set; }
        public string TITLE { get; set; }
        public string[] EXTIDLST { get; set; } = new string[8];
        public string[] TRNDSET { get; set; } = new string[8];
        public string TRNDBASE { get; set; }
        public string[] SCREENUM { get; set; } = new string[2];
        public string[] DISPID { get; set; } = new string[2];

        public static int OffsetEXTIDLST = 1;
        public static int OffsetTRNDSET = 1;
        public static int OffsetSCREENUM = 101;
        public static int OffsetDISPID = 101;
    }
}
