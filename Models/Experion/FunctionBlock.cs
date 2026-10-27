using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Honeywell.Experion
{
    public class FunctionBlock
    {
        [Required]
        public string BlockName { get; set; }
        [Required]
        public string TemplateName { get; set; }
        public List<FunctionBlockParameter> Parameters { get; set; } = new();

        public void AddParameter(FunctionBlockParameter param)
        {
            param.ParentFunctionBlock = this;
            Parameters.Add(param);
        }
    }
}
