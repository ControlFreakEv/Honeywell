using System.ComponentModel.DataAnnotations;

namespace Honeywell.Experion
{
    public class FunctionBlockParameter
    {
        [Required]
        public FunctionBlock ParentFunctionBlock { get; set; } = null!;
        [Required]
        public string ParamName { get; set; } = null!;
        [Required]
        public string? ParamValue { get; set; }
        public bool BlockPinEnabled { get; set; }
        public bool ConfigViewEnabled { get; set; }
        public bool MonitoringViewEnabled { get; set; }
    }
}
