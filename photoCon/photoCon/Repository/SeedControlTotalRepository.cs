using Microsoft.EntityFrameworkCore;
using photoCon.Data;
using photoCon.Interface;
using photoCon.Models;

namespace photoCon.Repository
{
    public class SeedControlTotalRepository : ISeedControlTotalRepository
    {
        private readonly TallyProgramContext _tallyProgramContext;

        public SeedControlTotalRepository(TallyProgramContext tallyProgramContext)
        {
            _tallyProgramContext = tallyProgramContext;
        }

        public int CountAllPhotoPerRegionAndCategory(int regionId, int categoryId)
        {
            var Count = (from a in _tallyProgramContext.PhotoLocations
                        where a.RegionId == regionId
                        && a.CategoryId == categoryId
                        && a.PhotoCode == "10"
                        select a).Count();

            return Count;
        }

        public bool CreateSeedControlTotal(SeedControlTotal seedControlTotal)
        {
            _tallyProgramContext.SeedControlTotals.Add(seedControlTotal);
            return Save();
        }

        public async Task<SeedControlTotal> GetLastRecordPerRegionandPerCategory(int regionId, int categoryId)
        {
            var result = await (from a in _tallyProgramContext.SeedControlTotals
                                where a.RegionId == regionId
                                && a.CategoryId == categoryId
                                orderby a.ExecutedOn descending // Assuming there's a timestamp property to determine the latest record
                                select a).FirstOrDefaultAsync();

            return result;
        }

        public bool Save()
        {
            return _tallyProgramContext.SaveChanges() > 0;
        }
    }
}
