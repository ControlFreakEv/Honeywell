using System.ComponentModel.DataAnnotations;

namespace Honeywell.Experion
{
    public class FunctionBlock
    {
        [Required]
        public string BlockName { get; set; } = null!;
        [Required]
        public string TemplateName { get; set; } = null!;
        public List<FunctionBlockParameter> Parameters { get; set; } = new();

        public void AddParameter(FunctionBlockParameter param)
        {
            param.ParentFunctionBlock = this;
            Parameters.Add(param);
        }
    }
}
