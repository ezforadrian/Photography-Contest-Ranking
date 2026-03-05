using photoCon.Dto;
using photoCon.Models;

namespace photoCon.Interface
{
    public interface IDayNumberRepository
    {
        bool Save();
        bool DeleteRecordsByCategory(string category);
        bool AddRecordsByCategory(List<DayNumber> dayNumber);
        IQueryable<DayNumber_> GetAllDatesByCategory(string category);
        bool IsRecordExist(int id);
        bool ToggleDayNumberStatus(int id);
        string GetDayNumberStatus(int id);
        bool CloseOtherDateNumber(int id);
    }
}
