
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using photoCon.Dto;
using photoCon.Models;
using System.Net;
using photoCon.Interface;
using photoCon.Helper;
using Microsoft.Extensions.Options;

namespace photoCon.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ILogger<ReportsController> _logger;
        private readonly IReportRepository _reportRepository;
        private readonly IAuditLogsRepository _auditLogsRepository;
        private readonly ConfigurationAppInfo _appInfo;

        public ReportsController(
            ILogger<ReportsController> logger, 
            IReportRepository reportRepository, IAuditLogsRepository auditLogsRepository, IOptions<ConfigurationAppInfo> appInfo)
        {
            _logger = logger;
            _reportRepository = reportRepository;
            _auditLogsRepository = auditLogsRepository;
            _appInfo = appInfo.Value;
        }


        [Authorize(Roles = "SYSADMIN,APPADMIN,ECDUSER")]
        public IActionResult Index()
        {
            var ParameterReference = new vm_DisplayReportParameters()
            {
                ReferenceCodes = _reportRepository.GetReferenceCodes().Where(a => a.RefTypeID == "02" || a.RefTypeID == "03" || a.RefTypeID == "05" || a.RefTypeID == "06" || a.RefTypeID == "08").ToList(),
                Regions = _reportRepository.GetRegionsList().ToList(),
                Categories = _reportRepository.GetCategoryList().ToList()
            };
            return View(ParameterReference);
        }

        [Authorize(Roles = "SYSADMIN,APPADMIN,ECDUSER")]
        [HttpGet]
        public IActionResult ViewPDF(vm_ReportParameters_Selection ReportParamenters)
        {
            try
            {
                //Set Report Link
                string ReportLink = "";

                //Decrypt
                LocalEncryption localEncryption = new LocalEncryption();
                const string key = "thequickbrownfox";
                ReportParamenters.ReportCode = localEncryption.ANDecrypt(ReportParamenters.ReportCode, key);
                ReportParamenters.Region = localEncryption.ANDecrypt(ReportParamenters.Region, key);
                ReportParamenters.Round = localEncryption.ANDecrypt(ReportParamenters.Round, key);
                ReportParamenters.Category = localEncryption.ANDecrypt(ReportParamenters.Category, key);
                //ReportParamenters.EventName = localEncryption.ANDecrypt(ReportParamenters.EventName, key);
                //ReportParamenters.EventVenue = localEncryption.ANDecrypt(ReportParamenters.EventVenue, key);

                //Test Event Details
                ReportParamenters.EventName = "Testing Phase";
                ReportParamenters.EventVenue = "Corporate Office";

                string RankScreening = ReportParamenters.RankScreening;
                if (RankScreening == "0")
                {
                    //Regional
                    switch (ReportParamenters.ReportCode)
                    {
                        case "01": //Official
                            ReportParamenters.RankCount = _reportRepository.GetRoundCount(ReportParamenters.Round, ReportParamenters.Region);
                            ReportLink = _appInfo.AppReportURL + "01_Official_RegionalRanking_Report&rs:Command=Render";
                            ReportLink = ReportLink + "&Region=" + ReportParamenters.Region + "&Round=" + ReportParamenters.Round + "&Category=" + ReportParamenters.Category + "&EventName=" + "" + "&EventVenue=" + "" + "&TopSelection=" + ReportParamenters.RankCount + "&rs:Format=PDF";
                            break;
                        case "03": //Internal
                            ReportParamenters.RankCount = _reportRepository.GetRoundCount(ReportParamenters.Round, ReportParamenters.Region);
                            ReportLink = _appInfo.AppReportURL + "03_Internal_RegionalRanking+_Report&rs:Command=Render";
                            ReportLink = ReportLink + "&Region=" + ReportParamenters.Region + "&Round=" + ReportParamenters.Round + "&Category=" + ReportParamenters.Category + "&EventName=" + "" + "&EventVenue=" + "" + "&TopSelection=" + ReportParamenters.RankCount + "&rs:Format=PDF";
                            break;
                    }
                }
                else if (RankScreening == "1")
                {
                    //Finals
                    ReportParamenters.Round = "0601";
                    switch (ReportParamenters.ReportCode)
                    {
                        case "01": //Official
                            ReportParamenters.RankCount = _reportRepository.GetRoundCount(ReportParamenters.Round, ReportParamenters.Region);
                            ReportLink = _appInfo.AppReportURL + "02_Official_FinalRanking_Report&rs:Command=Render";
                            ReportLink = ReportLink + "&Category=" + ReportParamenters.Category + "&EventName=" + "" + "&EventVenue=" + "" + "&TopSelection=" + ReportParamenters.RankCount + "&rs:Format=PDF";
                            break;
                        case "03": //Internal
                            ReportParamenters.RankCount = _reportRepository.GetRoundCount(ReportParamenters.Round, ReportParamenters.Region);
                            ReportLink = _appInfo.AppReportURL + "04_Internal_FinalRanking+_Report&rs:Command=Render";
                            ReportLink = ReportLink + "&Category=" + ReportParamenters.Category + "&EventName=" + "" + "&EventVenue=" + "" + "&TopSelection=" + ReportParamenters.RankCount + "&rs:Format=PDF";
                            break;
                    }
                }

                WebRequest WebRequest = WebRequest.Create(ReportLink);
                WebRequest.Credentials = CredentialCache.DefaultCredentials;
                WebResponse WebResponse = WebRequest.GetResponse();
                var Stream = WebResponse.GetResponseStream();

                //Success Log
                _auditLogsRepository.SystemAuditLog("Controller", "Controller_ReportGeneration", 0, "Success | [" + User.Claims.First().Value + "] generated report [" + ReportParamenters.ReportCode + "]", User.Claims.First().Value);

                return new FileStreamResult(Stream, "application/pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message, ex.Source);
                _auditLogsRepository.SystemAuditLog("Controller", "Controller_ReportGeneration", 0, "Error | Report Generation Failed ", User.Claims.First().Value);
                return Json("");
            }

        }

        public IActionResult ReportsView(vm_ReportParameters_Selection ReportParamenters)
        {
            if (TempData["ReportParameters"] != null)
            {
                var TempDataString = TempData["ReportParameters"].ToString();
                ReportParamenters = System.Text.Json.JsonSerializer.Deserialize<vm_ReportParameters_Selection>(TempDataString);
            }
            return View(ReportParamenters);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GoToPDFView([FromBody] vm_ReportParameters_Selection ReportParamenters)
        {
            TempData["ReportParameters"] = System.Text.Json.JsonSerializer.Serialize(ReportParamenters);
            var Link = Url.Action("ReportsView");
            return Json(Link);
        }
    }
}