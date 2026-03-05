using Microsoft.EntityFrameworkCore;
using photoCon.Data;
using photoCon.Dto;
using photoCon.Helper;
using photoCon.Interface;
using photoCon.Models;

namespace photoCon.Repository
{
    public class ProcessParameterRepository : IProcessParameterRepository
    {
        private readonly TallyProgramContext _tallyProgramContext;
        private readonly ILogger<ProcessParameterRepository> _logger;
        LocalEncryption _encryption = new LocalEncryption();

        public ProcessParameterRepository(TallyProgramContext tallyProgramContext, ILogger<ProcessParameterRepository> logger)
        {
            _tallyProgramContext = tallyProgramContext;
            _logger = logger;
        }

        public bool CreateProcessParameter(ProcessParameter processParameter)
        {
            _tallyProgramContext.Add(processParameter);
            return Save();
        }



        public bool Save()
        {
            var saved = _tallyProgramContext.SaveChanges();

            return saved > 0 ? true : false;
        }


        public bool UpdateProcessParameter(string description, string process, string processCode, string parameterValue, string userId)
        {
            using (var transaction = _tallyProgramContext.Database.BeginTransaction())
            {
                try
                {
                    var processParameter = _tallyProgramContext.ProcessParameters
                                        .SingleOrDefault(p => p.Description == description
                                                           && p.Process == process
                                                           && p.Code == processCode);

                    if (processParameter == null)
                    {
                        // Log an error or handle the case where the parameter is not found
                        return false;
                    }

                    processParameter.ParameterValue = parameterValue;
                    processParameter.ModifiedBy = userId;
                    processParameter.ModifiedDateTime = DateTime.Now;

                    _tallyProgramContext.SaveChanges();
                    transaction.Commit();

                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                    // Log the exception or handle it as needed
                    transaction.Rollback();
                    return false;
                }
            }
        }


        public bool UpdateProcessParameterUsingIndex(int index, string processCode, string filler01, string userId)
        {
            using (var transaction = _tallyProgramContext.Database.BeginTransaction())
            {
                try
                {
                    var processParameter = _tallyProgramContext.ProcessParameters
                                        .SingleOrDefault(p => p.Index == index);

                    if (processParameter == null)
                    {
                        // Handle the case where the parameter is not found
                        return false;
                    }

                    processParameter.Code = processCode;
                    processParameter.Filler01 = filler01;
                    processParameter.ModifiedBy = userId;
                    processParameter.ModifiedDateTime = DateTime.Now;

                    _tallyProgramContext.SaveChanges();
                    transaction.Commit();

                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                    // Log the exception or handle it as needed
                    transaction.Rollback();
                    return false;
                }
            }
        }



        public bool IsExistProcessParameterByDate(string date_)
        {
            return _tallyProgramContext.ProcessParameters.Any(p => p.ParameterValue == date_ && p.IsActive == 1);
        }

        public bool IsExistProcessParameter(string description, string process, string processCode)
        {
            return _tallyProgramContext.ProcessParameters.Any(p => p.Description == description && p.Process == process && p.Code == processCode);
        }

        public string GetParameterValue(string description, string process, string processCode)
        {
            var paramValue = (from p in _tallyProgramContext.ProcessParameters
                              where p.Description == description
                              && p.Process == process
                              && p.Code == processCode
                              select p.ParameterValue).FirstOrDefault();

            return paramValue;
        }

        




 

        public List<ProcessParameter> GetProcessParameters(string description, string process, string processCode)
        {
            var result = _tallyProgramContext.ProcessParameters.Where(x => x.Description == description && x.Process == process && x.Code == processCode);

            return result.ToList();
        }

        

        public IQueryable<ProcessParameterView> GetAllActiveDates(string category)
        {
            var query = from j in _tallyProgramContext.ProcessParameters
                        where j.Description == category
                        && j.IsActive == 1
                        orderby j.ParameterValue  // Optionally order by Date_ or any other field
                        select j;

            var groupedQuery = query.ToList()
                                .Select((j, index) => new ProcessParameterView
                                {
                                    Id_ = index + 1, 
                                    HashIndex = _encryption.ANEncrypt(j.Index.ToString(), "thequickbrownfox"),
                                    Process = j.Process,
                                    Code = j.Code,
                                    Description = j.Description,
                                    DetailedDescription = j.DetailedDescription,
                                    ParameterValue = j.ParameterValue,
                                    IsActive = j.IsActive,
                                    Filler01 = j.Filler01,
                                    Filler02 = j.Filler02,
                                    Filler03 = j.Filler03,
                                    Filler04 = j.Filler04,
                                    Filler05 = j.Filler05,
                                    Filler06 = j.Filler06,
                                    Filler07 = j.Filler07,
                                    Filler08 = j.Filler08,
                                    EffectivityDate = j.EffectivityDate,
                                    CreatedBy   = j.CreatedBy,
                                    CreatedDateTime = j.CreatedDateTime,
                                    ModifiedBy = j.ModifiedBy,
                                    ModifiedDateTime = j.ModifiedDateTime,
                                }); ;  // Materialize the query to perform grouping
                              

            return groupedQuery.AsQueryable();
        }

        public List<ProcessParameter> GetAllPrelimActiveDates()
        {
            var result = (from a in _tallyProgramContext.ProcessParameters
                          where a.Process == "1"
                          && a.Description == "PrelimDate"
                          && a.IsActive == 1
                          select a).ToList().OrderBy(a => a.Index);

            return result.ToList();
        }

        public List<ProcessParameter> GetAllGrandFinalDates()
        {
            var result = (from a in _tallyProgramContext.ProcessParameters
                          where a.Process == "2"
                          && a.Description == "GrandFinalDate"
                          && a.IsActive == 1
                          select a).ToList().OrderBy(a => a.Index);

            return result.ToList();
        }

        public List<ProcessParameter> GetAllFinalActiveDates()
        {
            var result = (from a in _tallyProgramContext.ProcessParameters
                          where a.Process == "2"
                          && a.Description == "GrandFinalDate"
                          && a.IsActive == 1
                          select a).ToList().OrderBy(a => a.Index);

            return result.ToList();
        }

        public ProcessParameter GetProcessParameterByIndex(int index)
        {
            var result = _tallyProgramContext.ProcessParameters.FirstOrDefault(p => p.Index == index);
            return result;
        }

        public bool CanDeleteProcessParameter(int index)
        {
            var processParameter = _tallyProgramContext.ProcessParameters.FirstOrDefault(p => p.Index == index);

            if (processParameter == null)
            {
                return false;
            }

            bool canDelete = processParameter.Filler02 == "False" &&
                             processParameter.Filler04 == "False" &&
                             processParameter.Filler06 == "False";

            return canDelete;
        }

        public bool CanDeleteFinalProcessParameter(int index)
        {
            var processParameter = _tallyProgramContext.ProcessParameters.FirstOrDefault(p => p.Index == index);

            if (processParameter == null)
            {
                return false;
            }

            bool canDelete = processParameter.Filler02 == "False";

            return canDelete;
        }

        public bool DeleteRecordByIndex(int index, string userId)
        {
            using (var transaction = _tallyProgramContext.Database.BeginTransaction())
            {
                try
                {
                    var processParameter = _tallyProgramContext.ProcessParameters
                                                .SingleOrDefault(p => p.Index == index);

                    if (processParameter == null)
                    {
                        // Handle the case where the parameter is not found
                        return false;
                    }

                    processParameter.IsActive = 0;
                    processParameter.DetailedDescription = "Close";
                    processParameter.ModifiedBy = userId;
                    processParameter.ModifiedDateTime = DateTime.Now;

                    _tallyProgramContext.SaveChanges();
                    transaction.Commit();

                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                    // Log the exception or handle it as needed
                    transaction.Rollback();
                    return false;
                }
            }
        }


        public bool UpdateDetailedDescriptionByIndex(int index, string userId)
        {
            using (var transaction = _tallyProgramContext.Database.BeginTransaction())
            {
                try
                {
                    var processParameter = _tallyProgramContext.ProcessParameters
                                                .SingleOrDefault(p => p.Index == index && p.IsActive == 1);

                    if (processParameter == null)
                    {
                        // Handle the case where the parameter is not found
                        return false;
                    }

                    processParameter.DetailedDescription = "Open";
                    processParameter.ModifiedBy = userId;
                    processParameter.ModifiedDateTime = DateTime.Now;

                    _tallyProgramContext.SaveChanges();
                    transaction.Commit();

                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                    // Log the exception or handle it as needed
                    transaction.Rollback();
                    return false;
                }
            }
        }


        public bool UpdateRoundReferenceCode(string refTypeId, string refCodeId)
        {
            using (var transaction = _tallyProgramContext.Database.BeginTransaction())
            {
                try
                {
                    // Find and update the entry that matches the given refCodeId
                    var referenceCodeTrue = _tallyProgramContext.ReferenceCode
                        .SingleOrDefault(r => r.RefTypeID == refTypeId && r.RefCodeID == refCodeId);

                    if (referenceCodeTrue == null)
                    {
                        // Log the issue (replace with actual logging)
                        Console.WriteLine($"No ReferenceCode found with RefTypeID: {refTypeId} and RefCodeID: {refCodeId}");
                        return false;
                    }

                    referenceCodeTrue.IsActive = 1;

                    // Find and update all entries that don't match the given refCodeId
                    var referenceCodesToDeactivate = _tallyProgramContext.ReferenceCode
                        .Where(r => r.RefTypeID == refTypeId && r.RefCodeID != refCodeId);

                    foreach (var referenceCode in referenceCodesToDeactivate)
                    {
                        referenceCode.IsActive = 0;
                    }

                    // Save changes to the database
                    _tallyProgramContext.SaveChanges();
                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                    // Log the exception (replace with actual logging)
                    Console.WriteLine($"Exception: {ex.Message}");
                    transaction.Rollback();
                    return false;
                }
            }
        }


        public bool IsRoundReferenceTrue(string refTypeId, string refCodeId)
        {
            return _tallyProgramContext.ReferenceCode.Where(p => p.RefTypeID == refTypeId && p.RefCodeID == refCodeId && p.IsActive == 1).Any();
        }

        public ProcessParameter GetProcessParameterByDetailedDescAndIsActive(string detailedDescription, int isActive)
        {
            //get the current open date
            return _tallyProgramContext.ProcessParameters
                                            .Where(
                                                    a => a.DetailedDescription == detailedDescription
                                                    && a.IsActive == isActive
                                                    ).FirstOrDefault();

        }


        //referenceCode --- get the current Rounds
        public ReferenceCode GetReferenceCodeByIsActiveAndRefTypeId(int isActive, string refTypeId)
        {
            return _tallyProgramContext.ReferenceCode
                                    .Where(
                                               a => a.IsActive == isActive
                                               && a.RefTypeID == refTypeId
                                            ).FirstOrDefault();
        }

        public List<CriteriaView> GetCriteria()
        {
            var record =  (from a in _tallyProgramContext.ReferenceCode
                           where a.IsActive == 1
                           && a.RefTypeID == "07"
                           select new CriteriaView
                           { 
                               RefCodeId = a.RefCodeID,
                               RefTypeId = a.RefTypeID,
                               CriteriaCode = a.RefTypeID.ToString() + a.RefCodeID.ToString(),
                               CriteriaDesc = a.RefCodeDesc
                           }).ToList();

            return record;


        }

        public bool CloseDailyInitialRoundByIndex(int ppindex, int reindex, string filler03, int roundInfo, string userid)
        {
            
            var processParameter = _tallyProgramContext.ProcessParameters
                                    .SingleOrDefault(p => p.Index == ppindex && p.IsActive == 1);

            string clRefTypeId = "05";
            string opRefTypeId = "05"; 
            string clRefCodeId, opRefCodeId;

            if (processParameter == null)
                throw new Exception("ProcessParameter not found or inactive.");

            if (roundInfo == 1)
            {
                //Closing Round 1
                processParameter.Filler02 = "True";
                processParameter.Filler03 = filler03;
                processParameter.ModifiedBy = userid;
                processParameter.ModifiedDateTime = DateTime.Now;

                clRefCodeId = roundInfo.ToString("00");
                opRefCodeId = (roundInfo + 1).ToString("00");


            }
            else if (roundInfo == 2)
            {
                //Closing Round 2
                processParameter.Filler04 = "True";
                processParameter.Filler05 = filler03;
                processParameter.ModifiedBy = userid;
                processParameter.ModifiedDateTime = DateTime.Now;

                clRefCodeId = roundInfo.ToString("00");
                opRefCodeId = (roundInfo + 1).ToString("00");
            }
            else if (roundInfo == 3)
            {
                //Closing Round 2
                processParameter.DetailedDescription = "Close";
                processParameter.Filler06 = "True";
                processParameter.Filler07 = filler03;
                processParameter.ModifiedBy = userid;
                processParameter.ModifiedDateTime = DateTime.Now;

                opRefTypeId = "06";
                clRefCodeId = roundInfo.ToString("00");
                opRefCodeId = "01";
            }
            else
            {
                throw new Exception("Invalid Round Information");

            }



            

            var referenceCode = _tallyProgramContext.ReferenceCode
                                .SingleOrDefault(r => r.RefTypeID == clRefTypeId && r.RefCodeID == clRefCodeId);

            var refCodeMsg = clRefTypeId + clRefCodeId;
            if (referenceCode == null)
                throw new Exception("ReferenceCode not " + refCodeMsg + " found.");

            referenceCode.IsActive = 0;

            if (roundInfo == 1 || roundInfo == 2)
            {
                var referenceCode1 = _tallyProgramContext.ReferenceCode
                                    .SingleOrDefault(r => r.RefTypeID == opRefTypeId && r.RefCodeID == opRefCodeId);
                if (referenceCode1 == null)
                    throw new Exception("ReferenceCode not " + refCodeMsg + " found.");

                referenceCode1.IsActive = 1;
            }

            

           

            

            return Save();


   

        }


        //public bool CloseDailyFinalRoundByIndex(
        //                                            int ppindex,
        //                                            int reindex,
        //                                            string filler05,
        //                                            string userid
        //                                        )
        //{
            
        //    var processParameter = _tallyProgramContext.ProcessParameters
        //                            .SingleOrDefault(p => p.Index == ppindex && p.IsActive == 1);

        //    if (processParameter == null)
        //        throw new Exception("ProcessParameter not found or inactive.");

        //    processParameter.Filler04 = "True";
        //    processParameter.Filler05 = filler05;
        //    processParameter.DetailedDescription = "Close";
        //    processParameter.ModifiedBy = userid;
        //    processParameter.ModifiedDateTime = DateTime.Now;

        //    var referenceCode = _tallyProgramContext.ReferenceCode
        //                        .SingleOrDefault(r => r.RefTypeID == "05" && r.RefCodeID == "02");

        //    if (referenceCode == null)
        //        throw new Exception("ReferenceCode 0502 not found.");

        //    referenceCode.IsActive = 0;




        //    return Save();
        //}

        public bool CloseDailyGrandFinalRoundByIndex(
                                                    int ppindex,
                                                    int reindex,
                                                    string filler05,
                                                    string userid
                                                )
        {

            var processParameter = _tallyProgramContext.ProcessParameters
                                    .SingleOrDefault(p => p.Index == ppindex && p.IsActive == 1);

            if (processParameter == null)
                throw new Exception("ProcessParameter not found or inactive.");

            processParameter.Filler03 = filler05;
            processParameter.Filler02 = "True";
            processParameter.DetailedDescription = "Close";
            processParameter.ModifiedBy = userid;
            processParameter.ModifiedDateTime = DateTime.Now;

            var referenceCode = _tallyProgramContext.ReferenceCode
                                .SingleOrDefault(r => r.RefTypeID == "06" && r.RefCodeID == "01");

            if (referenceCode == null)
                throw new Exception("ReferenceCode 0601 not found.");

            referenceCode.IsActive = 0;




            return Save();
        }

        public bool IsUpdateEnabledByFiller02(int index)
        {
            var record = (
                    from p in _tallyProgramContext.ProcessParameters
                    where p.Index == index
                    select p
                ).SingleOrDefault();

            var detailedDescription = record.DetailedDescription;
            var isActive = record.IsActive;
            var filler01 = record.Filler01;
            var filler02 = record.Filler02;
            var filler03 = record.Filler03;
            var filler04 = record.Filler04;
            var filler05 = record.Filler05;
            var filler06 = record.Filler06;
            var filler07 = record.Filler07;

            var refRecord = (
                                from r in _tallyProgramContext.ReferenceCode
                                where r.RefTypeID == "05"
                                && r.IsActive == 1
                                select r

                ).SingleOrDefault();

            if (refRecord == null)
            {
                return false;
            }
            else
            {
                var round = refRecord.RefTypeID.ToString() + refRecord.RefCodeID.ToString();

                if (
                        detailedDescription == "Open"
                        && isActive == 1
                        && filler02 == "False"
                        && filler03 == "0"
                        && filler04 == "False"
                        && filler05 == "0"
                        && filler06 == "False"
                        && filler07 == "0"
                        && round == "0501"

                   )
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }


        }


        public bool IsUpdateCloseEnabledGrandFinal(int index)
        {
            var record = (
                    from p in _tallyProgramContext.ProcessParameters
                    where p.Index == index
                    select p
                ).SingleOrDefault();

            var detailedDescription = record.DetailedDescription;
            var isActive = record.IsActive;
            var filler01 = record.Filler01;
            var filler02 = record.Filler02;
            var filler03 = record.Filler03;
            var filler04 = record.Filler04;
            var filler05 = record.Filler05;

            var refRecord = (
                                from r in _tallyProgramContext.ReferenceCode
                                where r.RefTypeID == "06"
                                && r.IsActive == 1
                                select r

                ).SingleOrDefault();

            if (refRecord == null)
            {
                return false;
            }
            else
            {
                var round = refRecord.RefTypeID.ToString() + refRecord.RefCodeID.ToString();

                if (
                        detailedDescription == "Open"
                        && isActive == 1
                        && filler02 == "False"
                        && filler03 == "0"
                        && round == "0601"

                   )
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }


        }

        public bool IsCloseInitialRoundEnable(int index, int roundInfo)
        {

            var record = (
                    from p in _tallyProgramContext.ProcessParameters
                    where p.Index == index
                    select p
                ).SingleOrDefault();

            var refRecord = (
                                from r in _tallyProgramContext.ReferenceCode
                                where r.RefTypeID == "05"
                                && r.IsActive == 1
                                select r

                ).SingleOrDefault();

            if (refRecord == null)
            {
                return false;
            }
            else 
            {
                var round = refRecord.RefTypeID.ToString() + refRecord.RefCodeID.ToString();

                var detailedDescription = record.DetailedDescription;
                var isActive = record.IsActive;
                var filler01 = record.Filler01;
                var filler02 = record.Filler02;
                var filler03 = record.Filler03;
                var filler04 = record.Filler04;
                var filler05 = record.Filler05;
                var filler06 = record.Filler06;
                var filler07 = record.Filler07;

                if (roundInfo == 1) //initial Round
                {
                    if (
                        detailedDescription == "Open"
                        && isActive == 1
                        && filler02 == "False"
                        && filler03 == "0"
                        && filler04 == "False"
                        && filler05 == "0"
                        && filler06 == "False"
                        && filler07 == "0"
                        && round == "0501"

                   )
                    {
                        return true;
                    }
                    else
                    { return false; }
                }
                else if (roundInfo == 2) //initial Round
                {
                    if (
                        detailedDescription == "Open"
                        && isActive == 1
                        && filler02 == "True"
                        && filler03 != "0"
                        && filler04 == "False"
                        && filler05 == "0"
                        && filler06 == "False"
                        && filler07 == "0"
                        && round == "0502"

                   )
                    {
                        return true;
                    }
                    else
                    { return false; }
                }
                else if (roundInfo == 3)
                {
                    if (
                        detailedDescription == "Open"
                        && isActive == 1
                        && filler02 == "True"
                        && filler03 != "0"
                        && filler04 == "True"
                        && filler05 != "0"
                        && filler06 == "False"
                        && filler07 == "0"
                        && round == "0503"

                   )
                    {
                        return true;
                    }
                    else
                    { return false; }
                }
                else
                {
                    return false;
                }
            }


        }

        public bool IsCompleteDailyRoundGrandFinalEnable(int index)
        {
            var record = (
                    from p in _tallyProgramContext.ProcessParameters
                    where p.Index == index
                    select p
                ).SingleOrDefault();

            var refRecord = (
                                from r in _tallyProgramContext.ReferenceCode
                                where r.RefTypeID == "06"
                                && r.IsActive == 1
                                select r

                ).SingleOrDefault();

            if (refRecord == null)
            {
                return false;
            }
            else
            {
                var round = refRecord.RefTypeID.ToString() + refRecord.RefCodeID.ToString();

                var detailedDescription = record.DetailedDescription;
                var isActive = record.IsActive;
                var filler01 = record.Filler01;
                var filler02 = record.Filler02;
                var filler03 = record.Filler03;

                if (
                        detailedDescription == "Open"
                        && isActive == 1
                        && filler02 == "False"
                        && filler03 == "0"
                        && round == "0601"

                   )
                {
                    return true;
                }
                else
                {

                    return false;
                }
            }


        }



        public string CloseInitialRoundValue(int index, int roundInfo)
        {
            string res = null;


            if (roundInfo == 1)
            {
                res = (from p in _tallyProgramContext.ProcessParameters
                         where (p.Index == index)
                         select p.Filler03
                               ).SingleOrDefault();
            }
            else if (roundInfo == 2)
            {
                res = (from p in _tallyProgramContext.ProcessParameters
                       where (p.Index == index)
                       select p.Filler05
                               ).SingleOrDefault();
            }
            else if (roundInfo == 3)
            {
                res = (from p in _tallyProgramContext.ProcessParameters
                       where (p.Index == index)
                       select p.Filler07
                               ).SingleOrDefault();
            }
            else
            {
                res = null;
            }
          

            return res != null ? res : "";
        }

        public string CompleteDailyRoundValue(int index)
        {
            //filler05

            var res = (from p in _tallyProgramContext.ProcessParameters
                       where (p.Index == index)
                       select p.Filler05
                       ).SingleOrDefault();

            return res != null ? res : "";
        }

        public List<ReferenceCode> GetAllRoundReferenceCodes()
        {
            var record = (from r in _tallyProgramContext.ReferenceCode
                          where r.RefTypeID == "05"
                          select r).ToList();

            return record;
        }

        public List<ReferenceCode> GetAllGrandFinalRoundReferenceCodes()
        {
            var record = (from r in _tallyProgramContext.ReferenceCode
                          where r.RefTypeID == "06"
                          select r).ToList();

            return record;
        }

        public ReferenceCode GetReferenceCodeByIndex(int index)
        {
            return _tallyProgramContext.ReferenceCode
                                        .Where(x => x.Index == index)
                                        .FirstOrDefault();
        }

        public bool CanOpenFinalDate()
        {
            var recordP = _tallyProgramContext.ProcessParameters.Where(a => a.Process == "1" && a.IsActive == 1).ToList();
            var recordG = _tallyProgramContext.ProcessParameters.Where(a => a.Process == "2" && a.IsActive == 1).ToList();
            
            bool isThereOpenPrelim = recordP.Where(
                p => p.DetailedDescription == "Open" 
                || p.Filler04 == "False" 
                ).Any();
            bool isThereOpenFinal = recordG.Where(p => p.DetailedDescription == "Open").Any();

            if (!isThereOpenPrelim || !isThereOpenFinal)
            {
                return true;
            }

            return false;
        }

        public bool UpdateFiller08(int index, string filler08, string userid)
        {
            using (var transaction = _tallyProgramContext.Database.BeginTransaction())
            {
                try
                {
                    var processParameter = _tallyProgramContext.ProcessParameters
                                        .SingleOrDefault(p => p.Index == index);

                    if (processParameter == null)
                    {
                        // Handle the case where the parameter is not found
                        return false;
                    }

                    processParameter.Filler08 = filler08;

                    _tallyProgramContext.SaveChanges();
                    transaction.Commit();

                    return true;
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it as needed
                    transaction.Rollback();
                    return false;
                }
            }
        }

        public bool IsAllActiveRegionalDateCompleted()
        {
            var pActiveDates = _tallyProgramContext.ProcessParameters.Where(x =>
                                                                                x.Description == "PrelimDate"
                                                                                && x.IsActive == 1
                                                                                && (x.Filler06 == null || x.Filler06 != "True")
                                                                           ).Any();

            return !pActiveDates;
        }

        public bool IsThereAActiveGrandFinalDate()
        {
            var gActiveDates = _tallyProgramContext.ProcessParameters.Where(x =>
                                                                                x.Description == "GrandFinalDate"
                                                                                && x.IsActive == 1
                                                                           ).Any();

            return gActiveDates;
        }
    }
}
