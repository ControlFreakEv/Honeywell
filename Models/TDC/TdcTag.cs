using Microsoft.Msagl.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Honeywell.TDC
{
    [DataContract]
    public class TdcTag
    {
        public TdcModule? Module { get; set; } = null;
        public int Id { get; set; }
        public string Name { get; set; }
        public string PointType { get; set; }
        public string EbFile
        {
            get
            {
                return _ebFile;
            }

            set
            {
                _ebFile = value;

                var ebFile = Params.FirstOrDefault(x => x.Name == nameof(EbFile));
                if (ebFile == null)
                    Params.Add(new TdcParameter(this, nameof(EbFile), value, value));
                else
                    ebFile.Value = value;
            }
        }
        public string? Desc { get; set; }
        public List<TdcParameter> Params { get; set; } = new(100);

        public List<string> Packages { get; set; } = new(0);
        public bool PackageExists { get; set; }
        public List<string> Blocks { get; set; } = new(0);
        public bool BlockExists { get; set; }
        public string? AM { get; set; } = null;
        public string? HWYNUM { get; set; } = null;
        public string? BOXNUM { get; set; } = null;
        public string? NTWKNUM { get; set; } = null;
        public string? NODENUM { get; set; } = null;
        public string? SLOTNUM { get; set; } = null;
        public string? INPTSSLT { get; set; } = null;
        public string? INTVARNM { get; set; } = null;
        public string? OUTBOXNM { get; set; } = null;
        public string? OUTSLTNM { get; set; } = null;
        public string? OUTSSLT { get; set; } = null;
        public string? MODNUM { get; set; } = null;
        public string? NODETYP { get; set; } = null;
        public string? PNTBOXTY { get; set; } = null;
        public string? PVALGID { get; set; } = null;
        public string? CTLALGID { get; set; } = null;
        public string? ALGIDDAC { get; set; }
        public List<Node> Nodes { get; set; } = new(1);
        public List<TdcCL> CL { get; set; } = new List<TdcCL>(0);
        /// <summary>
        /// contains LOGICNIM or DEVCTL
        /// </summary>
        public object? Logic { get; set; } = null;

        private string _ebFile = string.Empty;
        private static int count = 0;

        public TdcTag(string name, string pointType, string ebFile)
        {
            Name = name;
            PointType = pointType;
            EbFile = ebFile;
            Id = ++count;
        }
    }
}
