//using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Drawing;

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
