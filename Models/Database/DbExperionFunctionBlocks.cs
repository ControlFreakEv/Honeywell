using System.ComponentModel.DataAnnotations;

namespace Honeywell.Database
{
    public class DbExperionFunctionBlock
    {
        [Key]
        [ImportAndExport(false, true)]
        public int Id { get; set; }
        public int ConfigTemplateId { get; set; }

        [ImportAndExport(true, true)]
        public string Name { get; set; } = null!;

        [ImportAndExport(true, true)]
        public string NewName { get; set; } = null!;

        [ImportAndExport(true, true)]
        public string Type { get; set; } = null!;

        [ImportAndExport(true, true)]
        public bool DeleteOnImport { get; set; }

        public virtual DbConfigTemplates? ConfigTemplate { get; set; }
        public virtual ICollection<DbExperionParameter> Parameters { get; set; } = null!;

        public DbExperionFunctionBlock()
        {

        }
    }
}
