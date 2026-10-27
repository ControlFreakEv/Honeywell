using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Honeywell.TDC
{
    public class TdcModule
    {
        private static int count = 0;
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Desc { get; set; }
        public List<TdcTag> TdcTags { get; set; } = new List<TdcTag>();

        public TdcModule(string name)
        {
            Name = name;
            Id = ++count;
        }

        public TdcModule(string name, string? desc)
        {
            Name = name;
            Desc = desc;
            Id = ++count;
        }
    }
}
