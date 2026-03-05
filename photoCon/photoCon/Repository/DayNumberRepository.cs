using Microsoft.EntityFrameworkCore;
using photoCon.Data;
using photoCon.Dto;
using photoCon.Helper;
using photoCon.Interface;
using photoCon.Models;
using System;
using System.Linq;

namespace photoCon.Repository
{
    public class DayNumberRepository : IDayNumberRepository
    {
        private readonly TallyProgramContext _tallyProgramContext;
        private readonly ILogger<DayNumberRepository> _logger;
        LocalEncryption _encryption = new LocalEncryption();
        public DayNumberRepository(TallyProgramContext tallyProgramContext, ILogger<DayNumberRepository> logger)
        {
            _tallyProgramContext = tallyProgramContext;
            _logger = logger;
        }


        public bool AddRecordsByCategory(List<DayNumber> dayNumbers)
        {
            try
            {
                _tallyProgramContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking; // Set QueryTrackingBehavior to NoTracking
                _tallyProgramContext.DayNumbers.AddRange(dayNumbers); // AddRange without tracking
                return Save(); // Save changes after adding the new records
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                Console.WriteLine($"Error adding records: {ex.Message}");
                return false;
            }
        }



        public bool DeleteRecordsByCategory(string category)
        {
            try
            {
                var recordsToDelete = _tallyProgramContext.DayNumbers
                                      .Where(d => d.Category == category);

                _tallyProgramContext.DayNumbers.RemoveRange(recordsToDelete);
                return Save(); // Save changes after deleting records
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                Console.WriteLine($"Error deleting records: {ex.Message}");
                return false;
            }
        }

        public IQueryable<DayNumber_> GetAllDatesByCategory(string category)
        {
            //var query = from j in _tallyProgramContext.DayNumbers
            //            select new DayNumber_ { 
            //                HashId = _encryption.ANEncrypt(j.Id.ToString(), "thequickbrownfox"),
            //                Date_ = j.Date_,
            //                Category = j.Category,
            //                DayNumber = j.DayNumber_,
            //                Status = j.Status,
            //            };

            //var result = query.OrderBy(a => a.Category);

            //return result;

            var query = from j in _tallyProgramContext.DayNumbers
                        where j.Category == category
                        orderby j.Date_  // Optionally order by Date_ or any other field
                        select j;

            var groupedQuery = query.ToList()  // Materialize the query to perform grouping
                              .Select((j, index) => new DayNumber_
                              {
                                  Id_ = index + 1,  // Compute Id_ as 1-based index
                                  HashId = _encryption.ANEncrypt(j.Id.ToString(), "thequickbrownfox"),
                                  Date_ = j.Date_,
                                  Category = j.Category,
                                  DayNumber = j.DayNumber_,
                                  Status = j.Status
                              });

            return groupedQuery.AsQueryable();
        }

        public bool IsRecordExist(int id)
        {
            return _tallyProgramContext.DayNumbers.Any(d => d.Id == id);
        }

        public bool ToggleDayNumberStatus(int id)
        {
            try
            {
                var dayNumber = _tallyProgramContext.DayNumbers.Find(id);
                if (dayNumber == null)
                    return false;

                dayNumber.ToggleStatus_();
                return Save(); // Save changes
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                Console.WriteLine($"Error toggling status: {ex.Message}");
                return false;
            }
        }

        public bool Save()
        {
            try
            {
                return _tallyProgramContext.SaveChanges() > 0; // Save changes and return true if any changes were made
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                Console.WriteLine($"Error saving changes: {ex.Message}");
                return false;
            }
        }

        public string GetDayNumberStatus(int id)
        {
            var status_ = (from a in _tallyProgramContext.DayNumbers
                           where a.Id == id
                           select a.Status).FirstOrDefault();

            return status_;
        }

        public bool CloseOtherDateNumber(int id)
        {
            try
            {
                // Retrieve all DayNumbers where Id is not equal to the specified id
                var dayNumbersToClose = _tallyProgramContext.DayNumbers
                    .Where(a => a.Id != id)
                    .ToList();

                // Update the Status property for each retrieved DayNumber
                foreach (var dayNumber in dayNumbersToClose)
                {
                    dayNumber.Status = "Close";
                }

                // Save changes to the database
                _tallyProgramContext.SaveChanges();

                // Return true indicating that the update was successful
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                // Handle exceptions or logging here
                Console.WriteLine($"Error closing other date numbers: {ex.Message}");
                return false;
            }
        }
    }
}
