namespace Honeywell.Database
{
    public class ImportAndExport : Attribute
    {
        public bool Import;
        public bool Export;

        public ImportAndExport(bool import, bool export)
        {
            Import = import;
            Export = export;
        }
    }
}
