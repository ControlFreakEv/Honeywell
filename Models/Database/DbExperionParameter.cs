using System.ComponentModel.DataAnnotations;

namespace Honeywell.Database
{
    public class DbExperionParameter : IDeleteOnImport
    {
        [Key]
        [ImportAndExport(false, true)]
        public int Id { get; set; }

        [ImportAndExport(true, true)]
        public int FunctionBlockId { get; set; }

        [ImportAndExport(true, true)]
        public string Name { get; set; } = null!;

        [ImportAndExport(true, true)]
        public string? Value { get; set; }

        [ImportAndExport(true, true)]
        public bool DeleteOnImport { get; set; }

        [ImportAndExport(false, true)]
        public virtual DbExperionFunctionBlock? FunctionBlock { get; set; }

        public DbExperionParameter()
        {

        }
    }
}
