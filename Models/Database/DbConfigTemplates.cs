using System.ComponentModel.DataAnnotations;

namespace Honeywell.Database
{
    public class DbConfigTemplates : IDeleteOnImport
    {
        [Key]
        [ImportAndExport(false, true)]
        public int Id { get; set; }

        [ImportAndExport(true, true)]
        public string TypicalName { get; set; } = null!;

        [ImportAndExport(true, true)]
        public bool DeleteOnImport { get; set; }
        public virtual ICollection<DbTdcModule> Modules { get; set; } = null!;
        public virtual ICollection<DbExperionFunctionBlock> FunctionBlocks { get; set; } = null!;

        public DbConfigTemplates()
        {

        }
    }
}
