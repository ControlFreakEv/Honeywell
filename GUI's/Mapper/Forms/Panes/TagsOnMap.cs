using Honeywell.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Honeywell.GUI.Mapper
{
    //todo: doesn't work right with modules containing multiple graphs
    public class TagsOnMap
    {
        public DbTdcNode Node { get; set; }
        public string Name { get; set; }
        public string? PointType { get; set; }
        public string Address { get; set; }

        public TagsOnMap(DbTdcNode node)
        {
            Node = node;
            Name = node.NodeId;
            PointType = node.Tag?.PointType;
            Address = Database.Helper.GetLcnAddress(node.Tag);
        }

        public static List<TagsOnMap> GetZoomNodes(List<DbTdcNode> nodes)
        {
            var result = new List<TagsOnMap>();
            for (int i = 0; i < nodes.Count; i++)
                result.Add(new TagsOnMap(nodes[i]));

            return result;
        }

        public static List<TagsOnMap> GetZoomNodes()
        {
            using var db = new TdcContext();
            var nodes = db.TdcNodes.ToList();

            return GetZoomNodes(nodes);
        }
    }
}
