using Microsoft.AspNetCore.Mvc;
using photoCon.Interface;
using photoCon.Helper;
using photoCon.Models;
using System.Dynamic;
using Microsoft.AspNetCore.Authorization;
using photoCon.Dto;
using System.Security.Claims;


namespace photoCon.Controllers
{
    public class RankController : Controller
    {
        LocalEncryption localEncryption = new LocalEncryption();
        private readonly ILogger<RankController> _logger;
        private readonly IRankRepository _rankRepository;
        private readonly IRegionRepository _regionRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IJudgeRepository _judgeRepository;
        private readonly IProcessParameterRepository _processParameterRepository;
        private readonly IBatchImageRepository _batchImageRepository;
        private readonly IPhotoLocationsRepository _photoLocationsRepository;
        private readonly IAuditLogsRepository _auditLogsRepository;
        const string key = "thequickbrownfox";

        public RankController(
                                ILogger<RankController> logger,
                                IRankRepository rankRepository, 
                                IRegionRepository regionRepository, 
                                ICategoryRepository categoryRepository, 
                                IJudgeRepository judgeRepository,
                                IProcessParameterRepository processParameterRepository,
                                IBatchImageRepository batchImageRepository,
                                IPhotoLocationsRepository photoLocationsRepository,
                                IAuditLogsRepository auditLogsRepository
                            )
        {
            _logger = logger;
            _rankRepository = rankRepository;
            _regionRepository = regionRepository;
            _categoryRepository = categoryRepository;
            _judgeRepository = judgeRepository;
            _processParameterRepository = processParameterRepository;
            _batchImageRepository = batchImageRepository;
            _photoLocationsRepository = photoLocationsRepository;
            _auditLogsRepository = auditLogsRepository;
        }


        

        [Authorize(Roles = "SYSADMIN,ECDUSER,APPADMIN")]
        public IActionResult Index()
        {
            return View();
        }


        [Authorize(Roles = "SYSADMIN,ECDUSER,APPADMIN")]
        public IActionResult DailyRank(string? date, string? roundParam, string? region, string? category, string? other, string? top)
        {

            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            try
            {

                ProcessParameter processParameter = null;
                var listParam = _processParameterRepository.GetAllPrelimActiveDates();

                if (date != null)
                {
                    try
                    {
                        var indexDate = localEncryption.ANDecrypt(date.ToString(), key);

                        if (int.TryParse(indexDate, out int parsedIndexDate))
                        {
                            processParameter = _processParameterRepository.GetProcessParameterByIndex(parsedIndexDate);
                        }
                        else
                        {
                            processParameter = listParam?.FirstOrDefault();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                        processParameter = listParam?.FirstOrDefault();
                    }
                }
                else
                {
                    processParameter = listParam?.FirstOrDefault();
                }

                if (processParameter == null)
                {
                    return View("WarningNoData");
                }

                var dateParam_ = processParameter.Index;


                ReferenceCode referenceCodeParam = null;
                var listRef = _processParameterRepository.GetAllRoundReferenceCodes();
                if (roundParam != null)
                {
                    try
                    {
                        var indexRound = localEncryption.ANDecrypt(roundParam.ToString(), key);

                        if (int.TryParse(indexRound, out int parsedIndexDate))
                        {
                            referenceCodeParam = _processParameterRepository.GetReferenceCodeByIndex(parsedIndexDate);
                        }
                        else
                        {
                            referenceCodeParam = listRef?.FirstOrDefault();
                        }

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                        referenceCodeParam = listRef?.FirstOrDefault();
                    }
                }
                else
                {
                    referenceCodeParam = listRef?.FirstOrDefault();
                }

                var roundParam_ = referenceCodeParam.Index;
                var round = referenceCodeParam.RefTypeID.ToString() + referenceCodeParam.RefCodeID.ToString();


                
                var regionList = _regionRepository.GetAllRegionView().OrderBy(x => x.RegionId);
                var categoryList = _categoryRepository.GetAllCategoryView().OrderBy(x => x.CategoryId);
                var prelimDateList = _processParameterRepository.GetAllPrelimActiveDates();
                var prelimRoundList = _processParameterRepository.GetAllRoundReferenceCodes();



                var otherOptions = new List<string> { "Sort", "Highest-Lowest", "Lowest-Highest" };
                var topOptions = new List<int> { 4, 10, 40, 10000 };

                int region__, category__, top__;

                region__ = ParseAndValidate(region, 4);
                category__ = ParseAndValidate(category, 3);
                top__ = ParseAndValidateTop(top);
                ProcessParameter record__ = ParseAndValidateDate(date);
                ReferenceCode round__ = ParseAndValidateRound(roundParam);
                string other__ = ParseAndValidateOth(other);
  
                List<OverallPhotoRankView> result_ = new List<OverallPhotoRankView>();


                //display is two decimal place but the computation is based on 4 decimal places
                result_ = _rankRepository.GetOverallRankingView(
                                                                record__.ParameterValue,
                                                                int.Parse(record__.Filler01),
                                                                (round__.RefTypeID.ToString() + round__.RefCodeID.ToString()),
                                                                region__,
                                                                category__
                                                                ).ToList();

                if (other__ == "Lowest-Highest")
                {
                    result_ = result_.OrderBy(a => a.AverageScore).Take(top__).ToList();
                }
                else 
                {
                    var tempresult_ = result_.OrderByDescending(a => a.AverageScore).Take(top__).ToList();
                    decimal minAve = 0.00m;
                    if (tempresult_.Count >= top__)
                    {
                        minAve = tempresult_[top__ - 1].AverageScore;
                    }
                    
  
   
                    result_ = result_
                            .Where(x => x.AverageScore >= minAve)
                            .OrderByDescending(a => a.AverageScore)
                            .ToList();
                }
                





                //if (other__ == "Highest-Lowest")
                //{
                //    result_ = result_.Where(x => x.Rank <= top__).OrderByDescending(a => a.AverageScore).ToList();
                //}

                



                



                dynamic multipleModel = new ExpandoObject();

                multipleModel.regionList = regionList;
                multipleModel.categoryList = categoryList;
                multipleModel.result_ = result_;
                multipleModel.topOptions = topOptions;
                multipleModel.otherOptions = otherOptions;
                multipleModel.dateList = prelimDateList;
                multipleModel.roundList = prelimRoundList;

                ViewBag.RegionId = region__;
                ViewBag.CategoryId = category__;
                ViewBag.PrelimDates = record__.Index;
                ViewBag.RoundIndex = round__.Index;
                ViewBag.Top = top__;
                ViewBag.Oth = other__;


                return View(multipleModel);
          





            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                return StatusCode(500, new { Errors = new List<string> { "My rank encountered error." } });
            }
        }

        [Authorize(Roles = "SYSADMIN,ECDUSER,APPADMIN")]
        public IActionResult GrandFinalRank(string? date, string? roundParam, string? category, string? other, string? top)
        {

            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            try
            {

                ProcessParameter processParameter = null;
                var listParam = _processParameterRepository.GetAllGrandFinalDates();

                if (date != null)
                {
                    try
                    {
                        var indexDate = localEncryption.ANDecrypt(date.ToString(), key);

                        if (int.TryParse(indexDate, out int parsedIndexDate))
                        {
                            processParameter = _processParameterRepository.GetProcessParameterByIndex(parsedIndexDate);
                        }
                        else
                        {
                            processParameter = listParam?.FirstOrDefault();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                        processParameter = listParam?.FirstOrDefault();
                    }
                }
                else
                {
                    processParameter = listParam?.FirstOrDefault();
                }

                if (processParameter == null)
                {
                    return View("WarningNoData");
                }

                var dateParam_ = processParameter.Index;


                ReferenceCode referenceCodeParam = null;
                var listRef = _processParameterRepository.GetAllGrandFinalRoundReferenceCodes();
                if (roundParam != null)
                {
                    try
                    {
                        var indexRound = localEncryption.ANDecrypt(roundParam.ToString(), key);

                        if (int.TryParse(indexRound, out int parsedIndexDate))
                        {
                            referenceCodeParam = _processParameterRepository.GetReferenceCodeByIndex(parsedIndexDate);
                        }
                        else
                        {
                            referenceCodeParam = listRef?.FirstOrDefault();
                        }

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                        referenceCodeParam = listRef?.FirstOrDefault();
                    }
                }
                else
                {
                    referenceCodeParam = listRef?.FirstOrDefault();
                }

                var roundParam_ = referenceCodeParam.Index;
                var round = referenceCodeParam.RefTypeID.ToString() + referenceCodeParam.RefCodeID.ToString();



               
                var categoryList = _categoryRepository.GetAllCategoryView().OrderBy(x => x.CategoryId);
                var gfDateList = _processParameterRepository.GetAllFinalActiveDates();
                var gfRoundList = _processParameterRepository.GetAllGrandFinalRoundReferenceCodes();



                var otherOptions = new List<string> { "Sort", "Highest-Lowest", "Lowest-Highest" };
                var topOptions = new List<int> { 8, 16, 10000 };

                int region__, category__, top__;

                //region__ = ParseAndValidate(region, 4);
                category__ = ParseAndValidate(category, 3);
                top__ = ParseAndValidateTopGf(top);
                ProcessParameter record__ = ParseAndValidateDateF(date);
                ReferenceCode round__ = ParseAndValidateRoundF(roundParam);
                string other__ = ParseAndValidateOth(other);


                List<OverallPhotoRankView> result_ = new List<OverallPhotoRankView>();


                result_ = _rankRepository.GetGrandFinalRanking(
                                                                record__.ParameterValue,
                                                                int.Parse(record__.Filler01),
                                                                (round__.RefTypeID.ToString() + round__.RefCodeID.ToString()),
                                                                category__
                                                                ).ToList();

                if (other__ == "Lowest-Highest")
                {
                    result_ = result_.OrderBy(a => a.AverageScore).Take(top__).ToList();
                }
                else
                {
                    var tempresult_ = result_.OrderByDescending(a => a.AverageScore).Take(top__).ToList();
                    decimal minAve = 0.00m;
                    if (tempresult_.Count >= top__)
                    {
                        minAve = tempresult_[top__ - 1].AverageScore;
                    }



                    result_ = result_
                            .Where(x => x.AverageScore >= minAve)
                            .OrderByDescending(a => a.AverageScore)
                            .ToList();
                }

                //var result_ = _rankRepository.GetGrandFinalRanking(
                //                                                record__.ParameterValue,
                //                                                int.Parse(record__.Filler01),
                //                                                (round__.RefTypeID.ToString() + round__.RefCodeID.ToString()),
                //                                                category__
                //                                                ).Take(top__).ToList();


                //if (other__ == "Highest-Lowest")
                //{
                //    result_ = result_.OrderByDescending(a => a.AverageScore).ToList();
                //}

                //if (other__ == "Lowest-Highest")
                //{
                //    result_ = result_.OrderBy(a => a.AverageScore).ToList();
                //}







                dynamic multipleModel = new ExpandoObject();

                //multipleModel.regionList = regionList;
                multipleModel.categoryList = categoryList;
                multipleModel.result_ = result_;
                multipleModel.topOptions = topOptions;
                multipleModel.otherOptions = otherOptions;
                multipleModel.dateList = gfDateList;
                multipleModel.roundList = gfRoundList;

                //ViewBag.RegionId = region__;
                ViewBag.CategoryId = category__;
                ViewBag.PrelimDates = record__.Index;
                ViewBag.RoundIndex = round__.Index;
                ViewBag.Top = top__;
                ViewBag.Oth = other__;


                return View(multipleModel);






            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                return StatusCode(500, new { Errors = new List<string> { "My rank encountered error. Contact System Administrator" } });
            }
        }




        [Authorize(Roles = "JUDGE")]
        public IActionResult MyRank(string? region, string? category, string? other, string? top)
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            try
            {
                //parameterValues
                //ProcessParameter --get the Open and IsActive 1
                var processParam = _processParameterRepository.GetProcessParameterByDetailedDescAndIsActive("Open", 1);

                //----What Round?
                var refCodeParam = _processParameterRepository.GetReferenceCodeByIsActiveAndRefTypeId(1, "05");

                if (processParam == null || refCodeParam == null)
                {
                    return View("WarningNoData");
                }


                var round = refCodeParam.RefTypeID.ToString() + refCodeParam.RefCodeID.ToString();

                var dailyRes = _batchImageRepository.GetImagesByParamValues(
                    processParam.ParameterValue,
                    processParam.Filler01,
                    processParam.IsActive,
                    true, "10"
                );

                if (dailyRes.Count <= 0 || dailyRes == null)
                {
                    return View("WarningNoData");
                }
                else
                {
                    var regionList = _regionRepository.GetAllRegionView().OrderBy(x => x.RegionId);
                    var categoryList = _categoryRepository.GetAllCategoryView().OrderBy(x => x.CategoryId);

                    List<RankCount> ranksReg = new List<RankCount>();

                    foreach (var reg in regionList)
                    {
                        // Filter result_ based on the current region's RegionId
                        var count_ = dailyRes.Count(a => a.PhotoLocations_RegionId == reg.RegionId);

                        if (count_ > 0)
                        {
                            // Create a new RankCount object with the current region's GenId and the count
                            RankCount rankCount = new RankCount
                            {
                                GenId = reg.RegionId,
                                GenName = reg.RegionName,
                                Count_ = count_
                            };

                            // Add the RankCount object to the ranks list
                            ranksReg.Add(rankCount);
                        }


                    }

                    List<RankCount> ranksCat = new List<RankCount>();

                    foreach (var cat in categoryList)
                    {
                        // Filter result_ based on the current region's RegionId
                        var count_ = dailyRes.Count(a => a.PhotoLocations_CategoryId == cat.CategoryId);

                        if (count_ > 0)
                        {
                            // Create a new RankCount object with the current region's GenId and the count
                            RankCount rankCount = new RankCount
                            {
                                GenId = cat.CategoryId,
                                GenName = cat.CategoryName,
                                Count_ = count_
                            };

                            // Add the RankCount object to the ranks list
                            ranksCat.Add(rankCount);
                        }

                    }

                    int regionIdCount = ranksReg.Count;
                    int categoryIdCount = ranksCat.Count;

                    var otherOptions = new List<string> { "Sort", "Highest-Lowest", "Lowest-Highest" };
                    var topOptions = new List<int> { 4, 10, 40, 10000 };

                    int region__, category__, top__;

                    region__ = ParseAndValidate(region, regionIdCount, ranksReg[0]);
                    category__ = ParseAndValidate(category, categoryIdCount, ranksCat[0]);
                    top__ = ParseAndValidateTop(top);
                    string other__ = ParseAndValidateOth(other);


                    var result_ = _rankRepository.GetMyRankScores(
                                                                    processParam.ParameterValue,
                                                                    int.Parse(processParam.Filler01),
                                                                    true,
                                                                    round,
                                                                    userIdClaim.Value,
                                                                    region__,
                                                                    category__
                                                                    ).ToList();

                    if (other__ == "Lowest-Highest")
                    {
                        result_ = result_.OrderBy(a => a.MyScore).Take(top__).ToList();
                    }
                    else
                    {
                        var tempresult_ = result_.OrderByDescending(a => a.MyScore).Take(top__).ToList();
                        decimal minAve = 0.00m;
                        if (tempresult_.Count >= top__)
                        {
                            minAve = decimal.Parse(tempresult_[top__ - 1].MyScore.ToString());
                        }



                        result_ = result_
                                .Where(x => x.MyScore >= minAve)
                                .OrderByDescending(a => a.MyScore)
                                .ToList();
                    }






                    dynamic multipleModel = new ExpandoObject();

                    multipleModel.regionList = ranksReg.OrderBy(a => a.GenId).ToList();
                    multipleModel.categoryList = ranksCat.OrderBy(a => a.GenId).ToList();
                    multipleModel.result_ = result_;
                    multipleModel.topOptions = topOptions;
                    multipleModel.otherOptions = otherOptions;

                    ViewBag.RegionId = region__;
                    ViewBag.CategoryId = category__;
                    ViewBag.Top = top__;
                    ViewBag.Oth = other__;
                    ViewBag.Round = localEncryption.ANEncrypt(round,key);


                    return View(multipleModel);
                }

                



            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                return StatusCode(500, new { Errors = new List<string> { "My rank encountered error." } });
            }
        }

        [Authorize(Roles = "JUDGE")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MyRankGetInfo([FromBody] GenericDataScore hashPhotoId)
        {
            try
            {
                var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized();
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Errors = new List<string> { "Invalid parameter." } });
                } 

                if (!_photoLocationsRepository.IsImageExist(hashPhotoId.HashId)) 
                {
                    return BadRequest(new { Errors = new List<string> { "Image does not exist." } });
                }

                var refCodeParam = _processParameterRepository.GetReferenceCodeByIsActiveAndRefTypeId(1, "05");

                if (refCodeParam == null)
                {
                    return BadRequest(new { Errors = new List<string> { "No Reference for round." } });
                }

                var round = refCodeParam.RefTypeID.ToString() + refCodeParam.RefCodeID.ToString();

                try
                {
                    string decRound = localEncryption.ANDecrypt(hashPhotoId.HashRound, key);

                    if (decRound.Trim() != round.Trim())
                    {
                        return BadRequest(new { Errors = new List<string> { "Round is closed. Please refresh the page." } });
                    }
                }
                catch (Exception ex)
                {

                    _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                    return BadRequest(new { Errors = new List<string> { "Data Manipulation Row Number. Please refresh the page." } });
                }


                //get the image info using hashid
                //imageURL, hashPhotoId, MyScore, RegionName, CategoryName, Location, Description, Title

                var imageInfo = _rankRepository.GetImageInfoByHashId(hashPhotoId.HashId, userIdClaim.Value, round);

                return Ok(new { ServerCode = 200, ImageInfo = imageInfo });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);

                return BadRequest(new { Errors = new List<string> { "Error: Contact System Administrator" } }); ;

            }

        }

        [Authorize(Roles = "JUDGE")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddUpdateScore([FromBody] ImageScoreModal imageScoreModal)
        {
            var ProcessStatus = 0;
            string ProcessMessage = "";
            string logDescription = "";
            try
            {
                var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized();
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Errors = new List<string> { "Invalid parameter." } });
                }

                //ProcessParameter --get the Open and IsActive 1
                var processParam = _processParameterRepository.GetProcessParameterByDetailedDescAndIsActive("Open", 1);

                //----What Round?
                var refCodeParam = _processParameterRepository.GetReferenceCodeByIsActiveAndRefTypeId(1, "05");

                try
                {
                    string decRound = localEncryption.ANDecrypt(imageScoreModal.Round, key);
                    var currentRound = refCodeParam.RefTypeID.ToString() + refCodeParam.RefCodeID.ToString();

                    if (decRound.Trim() != currentRound.Trim())
                    {
                        return BadRequest(new { Errors = new List<string> { "Round is closed. Please refresh the page." } });
                    }
                }
                catch (Exception ex)
                {

                    _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                    return BadRequest(new { Errors = new List<string> { "Data Manipulation Row Number. Please refresh the page." } });
                }

                var xVal = imageScoreModal.Score.ToString();
                decimal parsedScore;
                if (!Decimal.TryParse(xVal, out parsedScore))
                {
                    // Handle the case where parsing fails
                    return BadRequest(new { Errors = new List<string> { "Invalid Score. Score shoul be minimum of 1 and maximum of 10." } });
                }


                if (parsedScore >= 10.01m || parsedScore < 1.00m)
                {
                    return BadRequest(new { Errors = new List<string> { "Invalid Score. Score shoul be minimum of 1 and maximum of 10." } });
                }

                if (!_judgeRepository.IsImageScoreByUser(userIdClaim.Value, imageScoreModal.PhotoId, refCodeParam.RefTypeID.ToString(), refCodeParam.RefCodeID.ToString()))
                {

                    var imageScore = new ImageScore
                    {
                        UserId = userIdClaim.Value,
                        PhotoId = imageScoreModal.PhotoId,
                        Score = imageScoreModal.Score,
                        Round = refCodeParam.RefTypeID.ToString() + refCodeParam.RefCodeID.ToString(),
                        LastUpdateDate = DateTime.UtcNow
                    };


                    //Add Record
                    if (_judgeRepository.SaveScoreByJudgeInImage(imageScore))
                    {
                        logDescription = "Success | Add Record | Score for photo [" + imageScoreModal.PhotoId + "] has been updated to [" + imageScoreModal.Score + "] by User [" + userIdClaim.Value + "] | Execution Date: " + DateTime.Now.ToString();
                        _auditLogsRepository.SystemAuditLog("Controller", "RankController_AddUpdateScore", 0, logDescription, User.Claims.First().Value);

                        return Json(new { ServerCode = 200, Message = "Score Added." });
                    }
                    else
                    {
                        logDescription = "Error | Add Record | Unable to post score for photo [" + imageScoreModal.PhotoId + "] | Execution Date: " + DateTime.Now.ToString();
                        _auditLogsRepository.SystemAuditLog("Controller", "RankController_AddUpdateScore", 0, logDescription, User.Claims.First().Value);

                        return Json(new { ServerCode = 400, Message = "Something went wrong. Try to refresh the page." });
                    }


                }
                else
                {
                    //Update Record
                    if (_judgeRepository.UpdateScoreByJudgeInImage
                                                (
                                                    userIdClaim.Value,
                                                    imageScoreModal.PhotoId,
                                                    imageScoreModal.Score,
                                                    refCodeParam.RefTypeID.ToString(),
                                                    refCodeParam.RefCodeID.ToString(),
                                                    DateTime.UtcNow
                                                 ))
                    {
                        logDescription = "Success | Update Record | Posted Score for photo [" + imageScoreModal.PhotoId + "] has been updated to [" + imageScoreModal.Score + "] by User [" + userIdClaim.Value + "] | Execution Date: " + DateTime.Now.ToString();
                        _auditLogsRepository.SystemAuditLog("Controller", "RankController_AddUpdateScore", 0, logDescription, User.Claims.First().Value);

                        return Json(new { ServerCode = 200, Message = "Score Updated." });
                    }
                    else
                    {
                        logDescription = "Error | Update Record | Unable to update posted score for photo [" + imageScoreModal.PhotoId + "] due to false return | Execution Date: " + DateTime.Now.ToString();
                        _auditLogsRepository.SystemAuditLog("Controller", "RankController_AddUpdateScore", 0, logDescription, User.Claims.First().Value);

                        return Json(new { ServerCode = 410, Message = "Something went wrong. Try to refresh the page." });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);

                return BadRequest(new { Errors = new List<string> { "Error: Contact System Administrator" } });;

            }





        }

        private int ParseAndValidate(string? value, int maxValue, RankCount rankCount)
        {
            if (string.IsNullOrWhiteSpace(value))
                return rankCount.GenId;

            value = value!.Trim();

            try
            {
                value = localEncryption.ANDecrypt(value, key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                return rankCount.GenId;
            }

            if (int.TryParse(value, out int result))
                return result;

            return rankCount.GenId;
        }


        private int ParseAndValidate(string? value, int maxValue)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 1;

            value = value!.Trim();

            try
            {
                value = localEncryption.ANDecrypt(value, key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                return 1;
            }

            if (int.TryParse(value, out int result) && result >= 1 && result <= maxValue)
                return result;

            return 1;
        }

        private ReferenceCode ParseAndValidateRound(string? value)
        {
            var record = _processParameterRepository.GetAllRoundReferenceCodes().FirstOrDefault();

            if (string.IsNullOrWhiteSpace(value))
                return record;

            value = value!.Trim();

            try
            {
                value = localEncryption.ANDecrypt(value, key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                return record;
            }

            if (int.TryParse(value, out int result))
            {
                var record__ = _processParameterRepository.GetReferenceCodeByIndex(result);
                if (record == null)
                {
                    return record;
                }

                
                return record__;
            }



            return record;
        }

        private ReferenceCode ParseAndValidateRoundF(string? value)
        {
            var record = _processParameterRepository.GetAllGrandFinalRoundReferenceCodes().FirstOrDefault();

            if (string.IsNullOrWhiteSpace(value))
                return record;

            value = value!.Trim();

            try
            {
                value = localEncryption.ANDecrypt(value, key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                return record;
            }

            if (int.TryParse(value, out int result))
            {
                var record__ = _processParameterRepository.GetReferenceCodeByIndex(result);
                if (record == null)
                {
                    return record;
                }


                return record__;
            }



            return record;
        }

        private ProcessParameter ParseAndValidateDate(string? value)
        {
            var record = _processParameterRepository.GetAllPrelimActiveDates().FirstOrDefault();
  
            
            if (string.IsNullOrWhiteSpace(value))
                return record;

            value = value!.Trim();

            try
            {
                value = localEncryption.ANDecrypt(value, key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                return record;
            }

            if (int.TryParse(value, out int result))
            {
                var record_ = _processParameterRepository.GetProcessParameterByIndex(result);
                if (record == null)
                {
                    return record;
                }


                return record_;
            }



            return record;
        }

        private ProcessParameter ParseAndValidateDateF(string? value)
        {
            var record = _processParameterRepository.GetAllFinalActiveDates().FirstOrDefault();


            if (string.IsNullOrWhiteSpace(value))
                return record;

            value = value!.Trim();

            try
            {
                value = localEncryption.ANDecrypt(value, key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                return record;
            }

            if (int.TryParse(value, out int result))
            {
                var record_ = _processParameterRepository.GetProcessParameterByIndex(result);
                if (record == null)
                {
                    return record;
                }


                return record_;
            }



            return record;
        }




        private int ParseAndValidateTop(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 10;

            value = value!.Trim();

            try
            {
                value = localEncryption.ANDecrypt(value, key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                return 10;
            }

            if (int.TryParse(value, out int result))
                return result;

            return 10;
        }

        private int ParseAndValidateTopGf(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 8;

            value = value!.Trim();

            try
            {
                value = localEncryption.ANDecrypt(value, key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                return 8;
            }

            if (int.TryParse(value, out int result))
                return result;

            return 8;
        }


        private string ParseAndValidateOth(string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                value = value!.Trim();

                try
                {
                    value = localEncryption.ANDecrypt(value, key);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                    return "Sort";
                }

                return value;
            }

            return "Sort"; // Default value
        }




    }
}
