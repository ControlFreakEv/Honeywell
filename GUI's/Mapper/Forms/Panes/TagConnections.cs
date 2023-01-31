using Honeywell.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Honeywell.GUI.Mapper
{
    public class TagConnections
    {
        public string Parameter { get; set; }
        public string Connection { get; set; }

        public TagConnections(DbTdcConnections connection)
        {
            Parameter = connection.Parameter;
            Connection = connection.ConnectedNodeName;
            if (connection.ConnectedNodeParameter == null)
                Connection = connection.ConnectedNodeName;
            else
                Connection = $"{connection.ConnectedNodeName}.{connection.ConnectedNodeParameter}";
        }

        public static List<TagConnections> GetConnections(DbTdcNode dbTdcNode)
        {
            using var db = new TdcContext();
            var nodes = db.ParameterConnections.Where(x => x.NodeId == dbTdcNode.Id).ToList();
            var result = new List<TagConnections>();
            for (int i = 0; i < nodes.Count(); i++)
                result.Add(new TagConnections(nodes[i]));

            return result.OrderBy(x => x.Connection).ToList();
        }

        public static List<TagConnections> GetConnections(List<DbTdcNode> dbTdcNodes)
        {
            var result = new List<TagConnections>();
            foreach (var dbTdcNode in dbTdcNodes)
                result.AddRange(GetConnections(dbTdcNode));

            return result.OrderBy(x => x.Connection).ToList();
        }
    }
}
