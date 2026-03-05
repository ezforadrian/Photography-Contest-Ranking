using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Novell.Directory.Ldap;
using photoCon.Data;
using photoCon.Helper;
using photoCon.Interface;
using photoCon.Models;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.DirectoryServices;
using System.Formats.Asn1;
using System.Linq;

namespace photoCon.Repository
{
    public class ReportRepository : IReportRepository
    {
        private readonly TallyProgramContext _tallyProgramContext;

        public ReportRepository(TallyProgramContext tallyProgramContext)
        {
            _tallyProgramContext = tallyProgramContext;
        }

        public List<ReferenceCode> GetReferenceCodes()
        {
            var ReferenceList = _tallyProgramContext.ReferenceCode.ToList();

            return ReferenceList;
        }

        public List<TblRegion> GetRegionsList()
        {
            var RegionsList = _tallyProgramContext.TblRegions.ToList();

            return RegionsList;
        }

        public List<Category> GetCategoryList()
        {
            var CategoryList = _tallyProgramContext.Categories.ToList();

            return CategoryList;
        }

        public int GetRoundCount(string RoundCode, string RegionCode)
        {
            var TopSelectCount = 10;
            List<ProcessParameter> RoundDateTable;
            if (RoundCode == "0601")
            {
                RoundDateTable = _tallyProgramContext.ProcessParameters.Where(a => Convert.ToInt32(a.Code).ToString() == RegionCode && a.Process == "2").ToList();
            }
            else
            {
                RoundDateTable = _tallyProgramContext.ProcessParameters.Where(a => Convert.ToInt32(a.Code).ToString() == RegionCode && a.Process == "1").ToList();
            }

            if (RoundDateTable.Count > 0)
            {
                var RoundDateParameter_Top = RoundDateTable.First();
                if (RoundCode == "0501" && RoundDateParameter_Top.Filler02 == "True")
                {
                    TopSelectCount = Convert.ToInt32(RoundDateParameter_Top.Filler03);
                }
                else if (RoundCode == "0502" && RoundDateParameter_Top.Filler04 == "True")
                {
                    TopSelectCount = Convert.ToInt32(RoundDateParameter_Top.Filler05);
                }
                else if (RoundCode == "0503" && RoundDateParameter_Top.Filler06 == "True")
                {
                    TopSelectCount = Convert.ToInt32(RoundDateParameter_Top.Filler07);
                }
                else if (RoundCode == "0601" && RoundDateParameter_Top.Filler02 == "True")
                {
                    TopSelectCount = Convert.ToInt32(RoundDateParameter_Top.Filler03);
                }
            }

            return TopSelectCount;
        }
    }
}