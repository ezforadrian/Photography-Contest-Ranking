using photoCon.Models;

namespace photoCon.Interface
{
    public interface ISeedControlTotalRepository
    {
        int CountAllPhotoPerRegionAndCategory(int regionId, int categoryId);

        Task<SeedControlTotal> GetLastRecordPerRegionandPerCategory(int regionId, int categoryId);

        bool CreateSeedControlTotal(SeedControlTotal seedControlTotal);
        bool Save();

    }
}
