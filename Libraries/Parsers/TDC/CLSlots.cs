namespace Honeywell.Parsers.TDC
{
    public class CLSlot
    {
        public string Block { get; set; }
        public string Tag { get; set; }

        public CLSlot(string block, string tag)
        {
            Block = block;
            Tag = tag;
        }
    }
}
