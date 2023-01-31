using Honeywell.Database;
using Honeywell.TDC;
using Mapper.Samples;
using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Honeywell.GUI.Mapper
{
    public class TagsInCL
    {
        public string Name { get; set; }
        public string? PointType { get; set; }
        public string Address { get; set; }

        public TagsInCL(DbTdcCLRefs clRef)
        {
            Name = clRef.Tag.Name;
            PointType = clRef.Tag?.PointType;
            Address = Database.Helper.GetLcnAddress(clRef.Tag);
        }

        public static List<TagsInCL> GetCLTags(DbTdcCL? cl, string? packageTag = null)
        {
            var db = new TdcContext();
            var dbTdcCL = db.TdcCLs.FirstOrDefault(x => x.Id == cl.Id);

            if (dbTdcCL == null && cl.FileName == SampleCl.SampleClFileName)
                dbTdcCL = cl;

            var result = new List<TagsInCL>();
            if (dbTdcCL == null)
                return result;

            List<string?> cdsToRemove = new(0);
            if (packageTag != null)
            {
                var cdsToKeep = dbTdcCL.CLTagReferences.SelectMany(x => x.Tag.Params).Where(x => x.CDS && x.Tag.Name == packageTag).Select(x => x.Value).ToList();
                cdsToRemove = dbTdcCL.CLTagReferences.SelectMany(x => x.Tag.Params).Where(x => x.CDS && !cdsToKeep.Contains(x.Value) && x.Tag.Name != packageTag).Select(x => x.Value).ToList();
            }

            foreach (var cltagRef in dbTdcCL.CLTagReferences)
            {
                if (packageTag != null)
                {
                    if (cltagRef.CLAttachedToThisPoint && cltagRef.Tag.Name != packageTag) //another instance of package
                        continue;
                    else if (cdsToRemove.Contains(cltagRef.Tag.Name)) //cds that only exists in another package
                        continue;
                }
                result.Add(new TagsInCL(cltagRef));
            } 

            return result;
        }

        public static List<TagsInCL> GetCLTags(string filename, string? packageTag = null)
        {
            using var db = new TdcContext();
            var cl = db.TdcCLs.FirstOrDefault(x => x.FileName == filename);

            if (cl == null && filename == SampleCl.SampleClFileName)
                cl = SampleCl.GetSampleCL();

            return GetCLTags(cl, packageTag);
        }
    }
}
