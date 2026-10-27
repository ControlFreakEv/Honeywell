using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Honeywell.Experion
{
    public class FunctionBlockParameter
    {
        [Required]
        public FunctionBlock ParentFunctionBlock { get; set; }
        [Required]
        public string ParamName { get; set; }
        [Required]
        public string? ParamValue { get; set; }
        public bool BlockPinEnabled { get; set; }
        public bool ConfigViewEnabled { get; set; }
        public bool MonitoringViewEnabled { get; set; }
    }
}
