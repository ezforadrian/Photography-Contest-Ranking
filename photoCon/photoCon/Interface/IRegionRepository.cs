using System.Collections.Generic;
using System.Threading.Tasks;
using photoCon.Dto;
using photoCon.Models;

namespace photoCon.Interface
{
    public interface IRegionRepository
    {
        Task<ICollection<TblRegion>> GetAllRegion();
        List<TblRegion> GetAllRegionView();
        bool HasRegion(string regionName);
        int GetRegionId(string regionName);
    }
}
