using Honeywell.Database;
using Honeywell.GUI.Mapper.ScintillaExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mapper.Samples
{
    public static class SampleCl
    {
        public static string SampleClFileName { get; } = "Sample.CL";
        public static List<string> SampleTagNames { get; private set; }
        public static List<DbTdcTag> SampleTags { get; private set; }
        static readonly string sampleFilePath = AppDomain.CurrentDomain.BaseDirectory + "Samples\\";

        public static DbTdcCL GetSampleCL()
        {
            var sampleCl = File.ReadAllText(sampleFilePath + SampleClFileName);

            var cl = new DbTdcCL
            {
                CLName = SampleClFileName,
                FileName = SampleClFileName,
                Content = sampleCl,
                OriginalContent = sampleCl,
                Id = -1
            };
            var tdcTags = new List<DbTdcTag>();
            var tdcFileRefs = new List<DbTdcFileRef>();
            var clRefs = new List<DbTdcCLRefs>();
            var nodes = new List<DbTdcNode>();

            var tdcParams1 = new List<DbTdcParameter>();
            tdcParams1.Add(new() { Id = -1, Name = "PKGNAME(1)", Value = "Sample ", TagId = -1 });
            tdcTags.Add(new() { Id = -1,  Name = "SAMPLEREGAM1", PointType = "REGAM", PackageExists = true, Params = tdcParams1, TdcFileRefs = tdcFileRefs, Nodes = nodes });

            var tdcParams2 = new List<DbTdcParameter>();
            tdcParams2.Add(new() { Id = -2, Name = "PKGNAME(1)", Value = "Sample ", TagId = -2 });
            tdcTags.Add(new() { Id = -2, Name = "SAMPLEREGAM2", PointType = "REGAM", PackageExists = true, Params = tdcParams1, TdcFileRefs = tdcFileRefs, Nodes = nodes });

            var tdcParams3 = new List<DbTdcParameter>();
            tdcParams3.Add(new() { Id = -3, Name = "DESC", Value = "TEST ", TagId = -3 });
            tdcTags.Add(new() { Id = -3, Name = "SAMPLEDICMPNIM", PointType = "DICMPNIM", Params = tdcParams3, TdcFileRefs = tdcFileRefs, Nodes = nodes });

            var tdcParams4 = new List<DbTdcParameter>();
            tdcParams4.Add(new() { Id = -4, Name = "DESC", Value = "TEST ", TagId = -4 });
            tdcTags.Add(new() { Id = -4, Name = "SAMPLEDIINNIM", PointType = "DIINNIM", Params = tdcParams4, TdcFileRefs = tdcFileRefs, Nodes = nodes });

            foreach (var tag in tdcTags)
            {
                var clRef = new DbTdcCLRefs() { CLAttachedToThisPoint = tag.PackageExists, Tag = tag, CL = cl, CLId = cl.Id };
                clRefs.Add(clRef);

                var list = new List<DbTdcCLRefs>(1);
                list.Add(clRef);
                tag.CLTagReferences = list;
            }
            cl.CLTagReferences = clRefs;

            SampleTags = tdcTags;
            SampleTagNames = tdcTags.Select(x => x.Name).ToList();
            CLLexer.SampleTdcTags = SampleTagNames;

            return cl;
        }

        public static bool IsSampleFile(string content)
        {
            var key = "--!!!mapper sample package!!!";

            if (content.Length <= key.Length)
                return false;

            var compare = content[..key.Length];
            return key == compare;
        }
    }
}
