using photoCon.Dto;
using photoCon.Models;

namespace photoCon.Interface
{
    public interface ICSVDetailsRepository 
    {
        CSV_Details isRecordExist(string photoname);
        //List<CSVGenericDataView> GetAllRecords();
        bool UpdateCSVDetails(string userId);
        bool AddCSVPhotoDetails(CSV_Details cSV_Details);
        bool Save();
        //List<string> GetDatabaseColumnNames();

    }
}
