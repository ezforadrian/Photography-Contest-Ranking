using Microsoft.Data.SqlClient;
using photoCon.Data;
using photoCon.Dto;
using photoCon.Interface;
using photoCon.Models;

namespace photoCon.Repository
{
    public class CSVDetailsRepository : ICSVDetailsRepository
    {
        private readonly TallyProgramContext _tallyProgramContext;
        
        private readonly IConfiguration _configuration;

        public CSVDetailsRepository(TallyProgramContext tallyProgramContext, IConfiguration configuration)
        {
            _tallyProgramContext = tallyProgramContext;
            _configuration = configuration;
        }

        public bool AddCSVPhotoDetails(CSV_Details cSV_Details)
        {
            _tallyProgramContext.CSV_PhotoDetails.Add(cSV_Details);
            return Save();
        }

        public bool UpdateCSVDetails(string userId)
        {
            var query = from a in _tallyProgramContext.CSV_PhotoDetails
                        join b in _tallyProgramContext.PhotoMetaData on a.photoname equals b.FileName 
                        select new { PhotoDetails_ = a, PhotoMeta = b };

            foreach (var item in query)
            {
                item.PhotoDetails_.photohash = item.PhotoMeta.HashPhotoId;
                item.PhotoDetails_.datetimeupdate = DateTime.Now.ToString();
                item.PhotoDetails_.userid = userId;
            }

            return Save();
        }

        public CSV_Details isRecordExist(string photoname)
        {
            return _tallyProgramContext.CSV_PhotoDetails.FirstOrDefault(p => p.photoname == photoname);
        }

        public bool Save()
        {
            return _tallyProgramContext.SaveChanges() > 0;
        }
      
        //public List<string> GetDatabaseColumnNames()
        //{
        //    var columns = new List<string>();
        //    var connectionString = _configuration.GetConnectionString("SqlServerConnStr"); // Replace with your connection string
        //    var tableName = "CSV_PhotoDetails"; // Replace with your table name

        //    using (var connection = new SqlConnection(connectionString))
        //    {
        //        connection.Open();
        //        var command = new SqlCommand($"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}'", connection);
        //        using (var reader = command.ExecuteReader())
        //        {
        //            while (reader.Read())
        //            {
        //                columns.Add(reader.GetString(0));
        //            }
        //        }
        //    }
        //    return columns;
        //}

    }
}
