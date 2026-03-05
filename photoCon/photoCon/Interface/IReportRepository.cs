using photoCon.Models;

namespace photoCon.Interface
{
    public interface IReportRepository
    {
        List<ReferenceCode> GetReferenceCodes();
        List<TblRegion> GetRegionsList();
        List<Category> GetCategoryList();
        int GetRoundCount(string RoundCode, string RegionCode);
    }
}
