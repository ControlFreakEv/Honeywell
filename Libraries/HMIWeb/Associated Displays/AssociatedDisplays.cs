using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Honeywell.HMIWeb
{
    public class AssociatedDisplays
    {
        
        public static ConcurrentBag<CustomShapeReplacement> GetShapeReplacements(string gfxPath, string shapeReplacementTextBox)
        {
            var replaceShapeList = new ConcurrentBag<CustomShapeReplacement>();

            var htmPath = Directory.GetFiles(gfxPath, shapeReplacementTextBox).FirstOrDefault();
            if (htmPath == null)
                return replaceShapeList;

            var gfxName = Path.GetFileNameWithoutExtension(htmPath);
            var gfxFilesPath = $@"{gfxPath}\{gfxName}_files";

            var htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.Load(htmPath);

            string xPathSrc = $@"//*[@src]";
            var customShapeNode = htmlDoc.DocumentNode.SelectNodes(xPathSrc);

            Parallel.ForEach(customShapeNode, node =>
            {
                replaceShapeList.Add(new CustomShapeReplacement(node, gfxFilesPath));
            });

            return replaceShapeList;
        }
    }
}
