using Honeywell.TDC;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Honeywell.Database
{
    public class DbTdcParameter
    {
        [Key]
        [ImportAndExport(false, true)]
        public int Id { get; set; }

        [ImportAndExport(true, true)]
        public string Name { get; set; }
        [ImportAndExport(true, true)]
        public string? SortName { get; set; }

        [ImportAndExport(true, true)]
        public string? Value { get; set; }

        [ImportAndExport(true, true)]
        public string RawValue { get; set; }

        [ImportAndExport(true, true)]
        public int TagId { get; set; }

        [ImportAndExport(true, true)]
        public bool CDS { get; set; }

        [ImportAndExport(true, true)]
        public bool DeleteOnImport { get; set; }

        [ImportAndExport(false, true)]
        public virtual DbTdcTag Tag { get; set; }

        public DbTdcParameter()
        {

        }

        public DbTdcParameter(TdcParameter tdcParameter)
        {
            Id = tdcParameter.Id;
            Name = tdcParameter.Name;
            Value = tdcParameter.Value;
            RawValue = tdcParameter.RawValue;
            SortName = tdcParameter.SortName;
            TagId = tdcParameter.ParentTag.Id;
            CDS = (tdcParameter.CdsTag != null);
        }
    }
}
