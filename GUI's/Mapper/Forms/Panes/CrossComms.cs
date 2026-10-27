using Honeywell.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Honeywell.GUI.Mapper
{
    public class CrossComms
    {
        public string SourceTagName { get; set; }
        public string TargetTagName { get; set; }
        public string SourceAddress { get; set; }
        public string TargetAddress { get; set; }
        public int GraphId { get; set; }

        public CrossComms(DbTdcNode sourceNode, DbTdcNode targetNode)
        {
            SourceTagName = sourceNode.Tag.Name;
            TargetTagName = targetNode.Tag.Name;
            SourceAddress = sourceNode.Tag.LcnAddress;
            TargetAddress = targetNode.Tag.LcnAddress;
            GraphId = sourceNode.GraphId;
        }

        public static List<CrossComms> GetCrossComms()
        {
            using var db = new TdcContext();
            var sourceNodes = db.SourceTdcCrossComms.OrderBy(x => x.Id).Select(x => x.Node).ToList();
            var targetNodes = db.TargetTdcCrossComms.OrderBy(x => x.Id).Select(x => x.Node).ToList();
            var result = new List<CrossComms>();
            for (int i = 0; i < sourceNodes.Count; i++)
                result.Add(new CrossComms(sourceNodes[i], targetNodes[i]));

            return result.OrderBy(x => x.SourceTagName).ToList();
        }
    }
}
