using Honeywell.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Honeywell.GUI.Mapper
{
    public class CLAll
    {
        public DbTdcCL CL { get; set; }
        public string FileName { get; set; }
        public string TagName { get; set; }
        public string PointType { get; set; }
        public string Address { get; set; }

        public CLAll(DbTdcCL dbCL, DbTdcTag? dbTdcTag)
        {
            CL = dbCL;
            if (dbCL.CLTagReferences.Where(x => x.CLAttachedToThisPoint).Count() > 1)
            {
                if (dbTdcTag.Params.Any(x => x.Name.Contains("PKGNAME") && x.Value == dbCL.FileName.Replace(".CL", null)))
                    FileName = $"{dbCL.FileName} (Package)";
                else
                    FileName = $"{dbCL.FileName} (Block)";
            }
            else
                FileName = dbCL.FileName;

            if (dbTdcTag != null)
            {
                TagName = dbTdcTag.Name;
                PointType = dbTdcTag.PointType;
                Address = Database.Helper.GetLcnAddress(dbTdcTag);
            }
        }

        public static List<CLAll> GetCL(List<DbTdcCL> cl)
        {
            var result = new List<CLAll>();
            for (int i = 0; i < cl.Count; i++)
            {
                foreach (var dbTdcCLRef in cl[i].CLTagReferences.Where(x => x.CLAttachedToThisPoint))
                    result.Add(new CLAll(cl[i], dbTdcCLRef.Tag));

                if (cl[i].CLTagReferences.Where(x => x.CLAttachedToThisPoint).Count() == 0)
                    result.Add(new CLAll(cl[i], null));

            }
                
            return result;
        }

        public static List<CLAll> GetCL()
        {
            using var db = new TdcContext();
            var cl = db.TdcCLs.ToList();

            return GetCL(cl);
        }
    }
}
