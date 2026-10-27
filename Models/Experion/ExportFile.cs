namespace Honeywell.Experion
{
    //CM = MultiBlock-Block-BlockDef
    //CM Parameters = MultiBlock-Block-Parameters
    //CM Configuration/Monitoring = MultiBlock-Block-SymbolAttrs
    //function blocks = MultiBlock-Block-EmbBlocks
    public class ExportFile
    {
        public string ParentBlockType { get; set; }

        public ExportFile(string xmlContent)
        {

        }
    }
}