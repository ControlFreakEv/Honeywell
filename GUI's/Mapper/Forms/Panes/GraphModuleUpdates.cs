//using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Honeywell.GUI.Mapper
{
    public class GraphModuleUpdates
    {
        public Node NodeInGraph { get; set; }
        public string? ModuleName { get; set; }

        public GraphModuleUpdates(Node nodeInGraph, string? moduleName)
        {
            NodeInGraph = nodeInGraph;
            ModuleName = moduleName;
        }
    }
}
