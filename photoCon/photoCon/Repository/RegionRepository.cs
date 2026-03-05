using Microsoft.EntityFrameworkCore;
using photoCon.Data;
using photoCon.Dto;
using photoCon.Interface;
using photoCon.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace photoCon.Repository
{
    public class RegionRepository : IRegionRepository
    {
        private readonly TallyProgramContext _context;

        public RegionRepository(TallyProgramContext tallyProgramContext)
        {
            _context = tallyProgramContext;
        }

        public async Task<ICollection<TblRegion>> GetAllRegion()
        {
            return await _context.TblRegions.ToListAsync();
        }

        public List<TblRegion> GetAllRegionView()
        {
            List<TblRegion> regions = new List<TblRegion>();

            // Use ToList() to explicitly convert IQueryable<RegionView> to List<RegionView>
            regions = (from r in _context.TblRegions
                       select new TblRegion
                       {
                           RegionId = r.RegionId,
                           RegionName = r.RegionName,
                       }).ToList();
            return regions;
        }


        public int GetRegionId(string regionName)
        {
            var regionId = (from a in _context.TblRegions
                            where a.RegionName == regionName
                            select a.RegionId).FirstOrDefault();

            return regionId;
        }

        public bool HasRegion(string regionName)
        {
            return _context.TblRegions.Any(r => r.RegionName == regionName);
        }
    }
}
