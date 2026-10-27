using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
