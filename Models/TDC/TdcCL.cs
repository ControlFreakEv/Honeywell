using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Honeywell.TDC
{
    public class TdcCL
    {
        private static int count = 0;
        public int Id { get; set; }
        public string FileName { get; set; }
        public string? CLName { get; set; }
        public string OriginalContent { get; set; }
        public string Content { get; set; }
        public List<string> TagsCLIsAttachedTo { get; set; } = new(1);

        public List<TdcTag> TdcTags { get; set; } = new List<TdcTag>();

        public TdcCL(string fileName, string? clName, string content)
        {
            FileName = fileName;
            CLName = clName;
            OriginalContent = content;
            Content = content;
            Id = ++count;
        }
    }
}
