using Honeywell.TDC;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Honeywell.Database
{
    public class DbTdcTag
    {
        [Key]
        [ImportAndExport(false, true)]
        public int Id { get; set; }

        [ImportAndExport(true, true)]
        public int? ProjectId { get; set; }

        [ImportAndExport(true, true)]
        public string Name { get; set; }

        [ImportAndExport(true, true)]
        public string PointType { get; set; }

        [ImportAndExport(true, true)]
        public string EbFile { get; set; }

        [ImportAndExport(true, true)]
        public string? Desc { get; set; }

        public bool PackageExists { get; set; }

        [ImportAndExport(true, true)]
        public string? AM { get; set; }

        [ImportAndExport(true, true)]
        public string? HWYNUM { get; set; }

        [ImportAndExport(true, true)]
        public string? BOXNUM { get; set; }

        [ImportAndExport(true, true)]
        public string? NTWKNUM { get; set; }

        [ImportAndExport(true, true)]
        public string? NODENUM { get; set; }

        [ImportAndExport(true, true)]
        public string? SLOTNUM { get; set; }

        [ImportAndExport(true, true)]
        public string? INPTSSLT { get; set; }

        [ImportAndExport(true, true)]
        public string? INTVARNM { get; set; }

        [ImportAndExport(true, true)]
        public string? OUTBOXNM { get; set; }

        [ImportAndExport(true, true)]
        public string? OUTSLTNM { get; set; }

        [ImportAndExport(true, true)]
        public string? OUTSSLT { get; set; }

        [ImportAndExport(true, true)]
        public string? MODNUM { get; set; }

        [ImportAndExport(true, true)]
        public string? NODETYP { get; set; }

        [ImportAndExport(true, true)]
        public string? PNTBOXTY { get; set; }

        [ImportAndExport(true, true)]
        public string? PVALGID { get; set; }

        [ImportAndExport(true, true)]
        public string? CTLALGID { get; set; }

        [ImportAndExport(true, true)]
        public string? ALGIDDAC { get; set; }

        [ImportAndExport(true, true)]
        public string? LcnAddress { get; set; }

        [ImportAndExport(true, true)]
        public string? Notes { get; set; }

        [ImportAndExport(true, true)]
        public bool DeleteOnImport { get; set; }

        [ImportAndExport(false, true)]
        public virtual DbProject? Project { get; set; }
        public virtual ICollection<DbTdcParameter> Params { get; set; }
        public virtual ICollection<DbTdcNode> Nodes { get; set; }
        public virtual ICollection<DbTdcCLRefs> CLTagReferences { get; set; }
        public virtual ICollection<DbTdcFileRef> TdcFileRefs { get; set; }

        public DbTdcTag()
        {

        }

        public DbTdcTag(TdcTag tdcTag)
        {
            Id = tdcTag.Id;
            Name = tdcTag.Name;
            PointType = tdcTag.PointType;
            EbFile = tdcTag.EbFile;
            Desc = tdcTag.Desc;
            PackageExists = tdcTag.PackageExists;
            AM = tdcTag.AM;
            HWYNUM = tdcTag.HWYNUM;
            BOXNUM = tdcTag.BOXNUM;
            NTWKNUM = tdcTag.NTWKNUM;
            NODENUM = tdcTag.NODENUM;
            SLOTNUM = tdcTag.SLOTNUM;
            INPTSSLT = tdcTag.INPTSSLT;
            INTVARNM = tdcTag.INTVARNM;
            OUTBOXNM = tdcTag.OUTBOXNM;
            OUTSLTNM = tdcTag.OUTSLTNM;
            OUTSSLT = tdcTag.OUTSSLT;
            MODNUM = tdcTag.MODNUM;
            NODETYP = tdcTag.NODETYP;
            PNTBOXTY = tdcTag.PNTBOXTY;
            PVALGID = tdcTag.PVALGID;
            CTLALGID = tdcTag.CTLALGID;
            ALGIDDAC = tdcTag.ALGIDDAC;

            LcnAddress = Helper.GetLcnAddress(this);
        }
    }
}
