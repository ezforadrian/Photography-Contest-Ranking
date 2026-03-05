using photoCon.Dto;
using photoCon.Models;

namespace photoCon.Interface
{
    public interface IProcessParameterRepository
    {

        bool IsExistProcessParameterByDate(string date_);
        bool IsExistProcessParameter(string description, string process, string processCode);//
        bool CreateProcessParameter(ProcessParameter processParameter); //
        bool UpdateProcessParameter(string description, string process, string processCode, string parameterValue, string userId); //
        bool UpdateProcessParameterUsingIndex(int index, string processCode, string filler01, string userId); //
        bool UpdateDetailedDescriptionByIndex(int index, string userid);
        bool UpdateFiller08(int index, string filler08, string userid);
        bool CanDeleteProcessParameter(int index);
        bool CanDeleteFinalProcessParameter(int index);
        bool DeleteRecordByIndex(int index, string userid);
        bool UpdateRoundReferenceCode(string refTypeId, string refCodeId);
        bool IsRoundReferenceTrue(string refTypeId, string refCodeId);
        bool CanOpenFinalDate();
        //string date, string daynumber, string detailedDescription, string process, string code
        bool CloseDailyInitialRoundByIndex(int ppindex, int reindex, string filler03, int roundInfo, string userid);
        //bool CloseDailyFinalRoundByIndex(int ppindex, int reindex, string filler05, string userid);
        bool CloseDailyGrandFinalRoundByIndex(int ppindex, int reindex, string filler05, string userid);
        bool IsUpdateEnabledByFiller02(int index);
        bool IsUpdateCloseEnabledGrandFinal(int index);
        bool IsCloseInitialRoundEnable(int index, int roundInfo);
        //bool IsCompleteDailyRoundEnable(int index);
        bool IsCompleteDailyRoundGrandFinalEnable(int index);
        string CloseInitialRoundValue(int index, int roundInfo);
        //string CompleteDailyRoundValue(int index);
        bool IsAllActiveRegionalDateCompleted();
        bool IsThereAActiveGrandFinalDate();
        bool Save();
        

        List<ProcessParameter> GetProcessParameters(string description, string process, string processCode);
        List<ProcessParameter> GetAllPrelimActiveDates();
        List<ProcessParameter> GetAllGrandFinalDates();
        List<ProcessParameter> GetAllFinalActiveDates();
        ProcessParameter GetProcessParameterByIndex(int index);
        ProcessParameter GetProcessParameterByDetailedDescAndIsActive(string detailedDescription, int isActive);
        IQueryable<ProcessParameterView> GetAllActiveDates(string category);
        ReferenceCode GetReferenceCodeByIsActiveAndRefTypeId(int isActive, string refTypeId);
        List<CriteriaView> GetCriteria();
        ReferenceCode GetReferenceCodeByIndex(int index);
        List<ReferenceCode> GetAllRoundReferenceCodes();
        List<ReferenceCode> GetAllGrandFinalRoundReferenceCodes();

    }
}
