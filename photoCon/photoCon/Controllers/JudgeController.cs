using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using photoCon.Dto;
using photoCon.Helper;
using photoCon.Interface;
using photoCon.Models;
using System;
using System.Dynamic;
using System.Security.Claims;
using System.Xml.Linq;



namespace photoCon.Controllers
{
    [Authorize(Roles = "JUDGE")]
    public class JudgeController : Controller
    {
        private readonly ILogger<JudgeController> _logger;
        private readonly IJudgeRepository _judgeRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProcessParameterRepository _processParameterRepository;
        private readonly IBatchImageRepository _batchImageRepository;
        private readonly IRegionRepository _regionRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IRankRepository _rankRepository;
        private readonly IAuditLogsRepository _auditLogsRepository;

        LocalEncryption localEncryption = new LocalEncryption();

        const string key_ = "thequickbrownfox";
        

        public JudgeController(ILogger<JudgeController> logger,
            IRankRepository rankRepository,
                                IJudgeRepository judgeRepository, 
                                UserManager<ApplicationUser> userManager,
                                IProcessParameterRepository processParameterRepository,
                                IBatchImageRepository batchImageRepository,
                                IRegionRepository regionRepository,
                                ICategoryRepository categoryRepository,
                                IAuditLogsRepository auditLogsRepository
                                )
        {
            _logger = logger;
            _rankRepository = rankRepository;
            _judgeRepository = judgeRepository;
            _userManager = userManager;
            _processParameterRepository = processParameterRepository;
            _batchImageRepository = batchImageRepository;
            _regionRepository = regionRepository;
            _categoryRepository = categoryRepository;
            _auditLogsRepository = auditLogsRepository;
        }





        public async Task<IActionResult> Index()
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                // User ID claim not found, handle accordingly
                return RedirectToAction("Login", "Home");
            }

            string userId = userIdClaim.Value;
            var processParam = _processParameterRepository.GetProcessParameterByDetailedDescAndIsActive("Open", 1);
            var refCodeParam = _processParameterRepository.GetReferenceCodeByIsActiveAndRefTypeId(1, "05");

            if (processParam == null || refCodeParam == null)
            {
                return View("NoImageUploaded");
            }

            var judgeImageViews = _batchImageRepository.GetImagesByParamValues(
                processParam.ParameterValue,
                processParam.Filler01,
                processParam.IsActive,
                true, "10"
            );

            var totalCount = judgeImageViews.Count;
            ViewBag.TotalCount = totalCount;

            judgeImageViews = judgeImageViews.Take(1).ToList();

            if (judgeImageViews.Count < 1)
            {
                return View("NoImageUploaded");
            }

 

            string imageId = judgeImageViews[0].ImageBatch_ImageHashId;
            var judgeScores = _judgeRepository.GetImageScoreByJudgeAndRound(userId, refCodeParam.RefTypeID, refCodeParam.RefCodeID, imageId);

            dynamic multipleModel = new ExpandoObject();
            multipleModel.judgeImageViews = judgeImageViews;
            multipleModel.judgeScores = judgeScores;

            return View(multipleModel);
        }


        [HttpGet("Judge/Score/{xcat}")]
        public async Task<IActionResult> Score(string xcat)
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                // User ID claim not found, handle accordingly
                return RedirectToAction("Login", "Home");
            }

            string catId = "";
            try
            {
                catId = localEncryption.ANDecrypt(xcat, key_);
            }
            catch (Exception)
            {

                return View("InvalidParameter");
            }
             
            int catIdDec = 0;

            var catList = _categoryRepository.GetAllCategoryView();

            if (int.TryParse(catId, out catIdDec))
            {
                if (catIdDec < catList[0].CategoryId || catIdDec > catList[catList.Count - 1].CategoryId)
                {
                    return View(BadRequest("Invalid Category"));
                }
            }

            var processParam = _processParameterRepository.GetProcessParameterByDetailedDescAndIsActive("Open", 1);
            var refCodeParam = _processParameterRepository.GetReferenceCodeByIsActiveAndRefTypeId(1, "05");
            


            if (processParam == null || refCodeParam == null)
            {
                return View("NoImageUploaded");
            }

            var round = refCodeParam.RefTypeID.ToString() + refCodeParam.RefCodeID.ToString();

            var judgeImageViews = _batchImageRepository.GetImagesByParamValues(
                processParam.ParameterValue,
                processParam.Filler01,
                processParam.IsActive,
                true, "10"
            );

            if (judgeImageViews.Count < 1)
            {
                return View("NoImageUploaded");
            }

            var catCount = judgeImageViews.Where(x => x.PhotoLocations_CategoryId == catIdDec).ToList();

            if (catCount.Count < 1)
            {
                return View("NoImageUploaded");
            }


            int sort = 1;
            var recordList = _batchImageRepository.GetImageBatchByDateDayNumberCategory(processParam.ParameterValue, int.Parse(processParam.Filler01), catIdDec, round, userIdClaim.Value);
            if (recordList.Count > 0 && recordList != null)
            {
                recordList = recordList.OrderBy(x => x.Sort).ToList();
                sort = recordList[0].Sort;
            }

            string sort_ = localEncryption.ANEncrypt(sort.ToString(), key_);
            string xcat_ = localEncryption.ANEncrypt(catIdDec.ToString(), key_);
            return RedirectToAction("Score", new { xcat = xcat_, xnum = sort_});

        }


        [HttpGet("Judge/Score/{xcat}/{xnum}")]
        public async Task<IActionResult> Score(string xcat, string xnum)
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                // User ID claim not found, handle accordingly
                return RedirectToAction("Login", "Home");
            }

            string catId = "";
            string xnum_ = "";
            int catIdDec = 0;
            int xnumDec = 1;

            try
            {
                catId = localEncryption.ANDecrypt(xcat, key_);
            }
            catch (Exception)
            {
                return View("InvalidParameter");
            }

            var catList = _categoryRepository.GetAllCategoryView();

            if (int.TryParse(catId, out catIdDec))
            {
                if (catIdDec < catList[0].CategoryId || catIdDec > catList[catList.Count - 1].CategoryId)
                {
                    return View("InvalidParameter");
                }
            }

            try
            {
                xnum_ = localEncryption.ANDecrypt(xnum, key_);
                xnumDec = int.Parse(xnum_);
            }
            catch (Exception)
            {
                return View("InvalidParameter");
            }

            string userId = userIdClaim.Value;
            var processParam = _processParameterRepository.GetProcessParameterByDetailedDescAndIsActive("Open", 1);
            var refCodeParam = _processParameterRepository.GetReferenceCodeByIsActiveAndRefTypeId(1, "05");

            if (processParam == null || refCodeParam == null)
            {
                return View("NoImageUploaded");
            }

            var judgeImageViews = _batchImageRepository.GetImagesByParamValues(
                processParam.ParameterValue,
                processParam.Filler01,
                processParam.IsActive,
                true, "10"
            );
            var totalCount = judgeImageViews.Where(x => x.PhotoLocations_CategoryId == catIdDec).ToList().Count;
            ViewBag.TotalCount = totalCount;

            judgeImageViews = judgeImageViews.Where(x => x.PhotoLocations_CategoryId == catIdDec && x.ImageBatch_Sort == xnumDec).ToList();

            
            ViewBag.xcat = xcat;
            ViewBag.xnum = xnum;
            ViewBag.round = localEncryption.ANEncrypt((refCodeParam.RefTypeID.ToString() + refCodeParam.RefCodeID.ToString()), key_ );

            judgeImageViews = judgeImageViews.Take(1).ToList();

            if (judgeImageViews.Count < 1)
            {
                return View("NoImageUploaded");
            }

            string imageId = judgeImageViews[0].ImageBatch_ImageHashId;
            var judgeScores = _judgeRepository.GetImageScoreByJudgeAndRound(userId, refCodeParam.RefTypeID, refCodeParam.RefCodeID, imageId);

            dynamic multipleModel = new ExpandoObject();
            multipleModel.judgeImageViews = judgeImageViews;
            multipleModel.judgeScores = judgeScores;


            return View(multipleModel);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NextPhoto([FromBody] DisplayUpdateRegional displayUpdate)
        {

            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Errors = new List<string> { "Invalid parameter." } });
                }

                string catId = "";
                string xnum_ = "";
                int catIdDec = 0;
                int xnumDec = 1;

                try
                {
                    catId = localEncryption.ANDecrypt(displayUpdate.XCat, key_);
                }
                catch (Exception)
                {
                    return BadRequest(new { Errors = new List<string> { "Data Manipulation Invalid Parameter. Reselect Category." } });
                }

                var catList = _categoryRepository.GetAllCategoryView();

                if (int.TryParse(catId, out catIdDec))
                {
                    if (catIdDec < catList[0].CategoryId || catIdDec > catList[catList.Count - 1].CategoryId)
                    {
                        return BadRequest(new { Errors = new List<string> { "Data Manipulation Invalid Parameter. Reselect Category." } });
                    }
                }

                try
                {
                    xnum_ = localEncryption.ANDecrypt(displayUpdate.XNum, key_);
                    xnumDec = int.Parse(xnum_);
                }
                catch (Exception)
                {
                    return BadRequest(new { Errors = new List<string> { "Data Manipulation Invalid Parameter. Reselect Category." } });
                }

                //ProcessParameter --get the Open and IsActive 1
                var processParam = _processParameterRepository.GetProcessParameterByDetailedDescAndIsActive("Open", 1);

                //----What Round?
                var refCodeParam = _processParameterRepository.GetReferenceCodeByIsActiveAndRefTypeId(1, "05");


                int parsedrownumberEn;
                try
                {
                    var rownumberEn = localEncryption.ANDecrypt(displayUpdate.ViewPhotoIdx, key_);
                    if (!int.TryParse(rownumberEn, out parsedrownumberEn))
                    {
                        return BadRequest(new { Errors = new List<string> { "Data Manipulation Row Number." } });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                    return BadRequest(new { Errors = new List<string> { "Data Manipulation Row Number. Please refresh the page." } });
                }

                if (parsedrownumberEn != displayUpdate.RowNumber)
                {
                    return BadRequest(new { Errors = new List<string> { "Data Manipulation Row Number. Please refresh the page." } });
           
                }


                var dailyRoundCount = _batchImageRepository.CountActiveImageForTheDayPerCat(processParam.ParameterValue
                                        , processParam.Filler01
                                        , processParam.IsActive, true, catIdDec);

                if (displayUpdate.RowNumber < 0)
                {
                    return BadRequest(new { Errors = new List<string> { "Invalid row number. Row number must be non-negative." } });

                }

                if (displayUpdate.RowNumber > dailyRoundCount)
                {
                    return BadRequest(new { Errors = new List<string> { "Invalid row number. " } });

                }

                if (parsedrownumberEn == dailyRoundCount)
                {
                    parsedrownumberEn = parsedrownumberEn - 1;
                }


                
                //GetListofImage by processParam Dates and Day and ImageBatch = 1
                var judgeImageView = _batchImageRepository
                                    .GetImagesByParamValues(
                                        processParam.ParameterValue
                                        , processParam.Filler01
                                        , processParam.IsActive, true , "10"
                                     ).Where(x => x.PhotoLocations_CategoryId == catIdDec)
                                     .OrderBy(x => x.ImageBatch_Sort).Skip(parsedrownumberEn).FirstOrDefault();



                if (judgeImageView == null)
                {
                    return View("NoImageUploaded");
                }

                

                judgeImageView.PhotoMetaData_HashPhotoID = localEncryption.ANEncrypt(judgeImageView.ImageBatch_Sort.ToString(), key_);

                

                string imageId = judgeImageView.ImageBatch_ImageHashId;


                var userId = userIdClaim.Value;
                List<ImageScore> judgeScores = new List<ImageScore>();


                //judgeScores = _judgeRepository.GetAllJudgeScore(userId, refCodeParam.RefTypeID, refCodeParam.RefCodeID);






                judgeScores = _judgeRepository.GetImageScoreByJudgeAndRound(userId, refCodeParam.RefTypeID, refCodeParam.RefCodeID, imageId);


                var jsonData = JsonConvert
                                .SerializeObject(
                                    new 
                                        {
                                            judgeImageView = judgeImageView
                                            ,judgeScores = judgeScores
                                    });

                return Ok(jsonData);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                return StatusCode(500, new { Errors = new List<string> { "An error occurred while deleting regional date." } });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PrevPhoto([FromBody] DisplayUpdateRegional displayUpdate)
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            try
            {
                //ProcessParameter --get the Open and IsActive 1
                var processParam = _processParameterRepository.GetProcessParameterByDetailedDescAndIsActive("Open", 1);

                //----What Round?
                var refCodeParam = _processParameterRepository.GetReferenceCodeByIsActiveAndRefTypeId(1, "05");

                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Errors = new List<string> { "Invalid parameter." } });
                }

                string catId = "";
                string xnum_ = "";
                int catIdDec = 0;
                int xnumDec = 1;

                try
                {
                    catId = localEncryption.ANDecrypt(displayUpdate.XCat, key_);
                }
                catch (Exception)
                {
                    return BadRequest(new { Errors = new List<string> { "Data Manipulation Invalid Parameter. Reselect Category." } });
                }

                var catList = _categoryRepository.GetAllCategoryView();

                if (int.TryParse(catId, out catIdDec))
                {
                    if (catIdDec < catList[0].CategoryId || catIdDec > catList[catList.Count - 1].CategoryId)
                    {
                        return BadRequest(new { Errors = new List<string> { "Data Manipulation Invalid Parameter. Reselect Category." } });
                    }
                }

                try
                {
                    xnum_ = localEncryption.ANDecrypt(displayUpdate.XNum, key_);
                    xnumDec = int.Parse(xnum_);
                }
                catch (Exception)
                {
                    return BadRequest(new { Errors = new List<string> { "Data Manipulation Invalid Parameter. Reselect Category." } });
                }

                int parsedrownumberEn;
                try
                {
                    var rownumberEn = localEncryption.ANDecrypt(displayUpdate.ViewPhotoIdx, key_);
                    if (!int.TryParse(rownumberEn, out parsedrownumberEn))
                    {
                        return BadRequest(new { Errors = new List<string> { "Data Manipulation Row Number." } });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                    return BadRequest(new { Errors = new List<string> { "Data Manipulation Row Number. Please refresh the page." } });
                }

                if (parsedrownumberEn != displayUpdate.RowNumber)
                {
                    return BadRequest(new { Errors = new List<string> { "Data Manipulation Row Number. Please refresh the page." } });

                }



               

                var dailyRoundCount = _batchImageRepository.CountActiveImageForTheDayPerCat(processParam.ParameterValue
                                        , processParam.Filler01
                                        , processParam.IsActive, true, catIdDec);

                if (displayUpdate.RowNumber < 0)
                {
                    return BadRequest(new { Errors = new List<string> { "Invalid row number. Row number must be non-negative." } });

                }

                if (displayUpdate.RowNumber > dailyRoundCount)
                {
                    return BadRequest(new { Errors = new List<string> { "Invalid row number. " } });

                }

                if (parsedrownumberEn == 1)
                {
                    parsedrownumberEn = 1;
                }


                //GetListofImage by processParam Dates and Day and ImageBatch = 1
                var judgeImageView = _batchImageRepository
                                    .GetImagesByParamValues(
                                        processParam.ParameterValue
                                        , processParam.Filler01
                                        , processParam.IsActive, true , "10"
                                     ).Where(x => x.PhotoLocations_CategoryId == catIdDec)
                                    .OrderBy(x => x.ImageBatch_Sort).Skip(parsedrownumberEn - 2).FirstOrDefault();

                if (judgeImageView == null)
                {
                    return View("NoImageUploaded");
                }

                judgeImageView.PhotoMetaData_HashPhotoID = localEncryption.ANEncrypt(judgeImageView.ImageBatch_Sort.ToString(), key_);

                string imageId = judgeImageView.ImageBatch_ImageHashId;


                var userId = userIdClaim.Value;
                List<ImageScore> judgeScores = new List<ImageScore>();
                //judgeScores = _judgeRepository.GetAllJudgeScore(userId, refCodeParam.RefTypeID, refCodeParam.RefCodeID);

                judgeScores = _judgeRepository.GetImageScoreByJudgeAndRound(userId, refCodeParam.RefTypeID, refCodeParam.RefCodeID, imageId);



                var jsonData = JsonConvert
                                .SerializeObject(
                                    new
                                    {
                                        judgeImageView = judgeImageView,
                                        judgeScores = judgeScores
                                    });

                return Ok(jsonData);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                return StatusCode(500, new { Errors = new List<string> { "An error occurred while deleting regional date." } });
            }


        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImageScore([FromBody] ImageScoreView imageScoreView)
        {
            var ProcessStatus = 0;
            string ProcessMessage = "";
            string logDescription = "";

            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            //ProcessParameter --get the Open and IsActive 1
            var processParam = _processParameterRepository.GetProcessParameterByDetailedDescAndIsActive("Open", 1);

            //----What Round?
            var refCodeParam = _processParameterRepository.GetReferenceCodeByIsActiveAndRefTypeId(1, "05");

            if (processParam == null || refCodeParam == null)
            {
                return BadRequest(new { Errors = new List<string> { "Invalid Internal Parameter." } });
            }



            if (!ModelState.IsValid)
            {
                return BadRequest(new { Errors = new List<string> { "Invalid parameter." } });
            }

            //checking of Round
            //try to decrypt first 
            try
            {
                string decRound = localEncryption.ANDecrypt(imageScoreView.Round, key_);
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

            int parsedrownumberEn;
            try
            {
                var rownumberEn = localEncryption.ANDecrypt(imageScoreView.VPhotoIdx, key_);
                if (!int.TryParse(rownumberEn, out parsedrownumberEn))
                {
                    return BadRequest(new { Errors = new List<string> { "Data Manipulation Row Number." } });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                return BadRequest(new { Errors = new List<string> { "Data Manipulation Row Number. Please refresh the page." } });
            }

            if (parsedrownumberEn != imageScoreView.RowNumber)
            {
                return BadRequest(new { Errors = new List<string> { "Data Manipulation Row Number. Please refresh the page." } });

            }


            var dailyRoundCount = _batchImageRepository.CountActiveImageForTheDay(processParam.ParameterValue
                            , processParam.Filler01
                            , processParam.IsActive, true);

            if (dailyRoundCount == 0)
            {
                return BadRequest(new { Errors = new List<string> { "Invalid active image number. Please refresh the page." } });
            }

            

            if (imageScoreView.RowNumber < 0)
            {
                return BadRequest(new { Errors = new List<string> { "Invalid row number. Row number must be non-negative." } });

            }

            if (imageScoreView.RowNumber > dailyRoundCount)
            {
                return BadRequest(new { Errors = new List<string> { "Invalid row number. " } });

            }

            var xVal = imageScoreView.Score.ToString();
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

            if (!_judgeRepository.IsImageScoreByUser(userIdClaim.Value, imageScoreView.PhotoId, refCodeParam.RefTypeID.ToString(), refCodeParam.RefCodeID.ToString()))
            {

                var imageScore = new ImageScore
                {
                    UserId = userIdClaim.Value,
                    PhotoId = imageScoreView.PhotoId,
                    Score = imageScoreView.Score,
                    Round = refCodeParam.RefTypeID.ToString() + refCodeParam.RefCodeID.ToString(),
                    LastUpdateDate = DateTime.UtcNow
                };


                //Add Record
                if (_judgeRepository.SaveScoreByJudgeInImage(imageScore))
                {
                    logDescription = "Success | Add Record | Score for photo [" + imageScoreView.PhotoId + "] has been updated to [" + imageScoreView.Score + "] by User [" + userIdClaim.Value + "] | Execution Date: " + DateTime.Now.ToString();
                    _auditLogsRepository.SystemAuditLog("Controller", "JudgeController_ImageScore", 0, logDescription, User.Claims.First().Value);
                    return Json(new { ServerCode = 200, Message = "Score Added." });
                }
                else
                {
                    logDescription = "Error | Add Record | Unable to post score for photo [" + imageScoreView.PhotoId + "] | Execution Date: " + DateTime.Now.ToString();
                    _auditLogsRepository.SystemAuditLog("Controller", "JudgeController_ImageScore", 0, logDescription, User.Claims.First().Value);
                    return Json(new { ServerCode = 400, Message = "Something went wrong. Try to refresh the page." });
                }


            }
            else
            {
                //Update Record
                if (_judgeRepository.UpdateScoreByJudgeInImage
                                            (
                                                userIdClaim.Value, 
                                                imageScoreView.PhotoId, 
                                                imageScoreView.Score,
                                                refCodeParam.RefTypeID.ToString(),
                                                refCodeParam.RefCodeID.ToString(),
                                                DateTime.UtcNow
                                             ))
                {
                    logDescription = "Success | Update Record | Posted Score for photo [" + imageScoreView.PhotoId + "] has been updated to [" + imageScoreView.Score + "] by User [" + userIdClaim.Value + "] | Execution Date: " + DateTime.Now.ToString();
                    _auditLogsRepository.SystemAuditLog("Controller", "JudgeController_ImageScore", 0, logDescription, User.Claims.First().Value);
                    return Json(new { ServerCode = 200, Message = "Score Updated." });
                }
                else
                {
                    logDescription = "Error | Update Record | Unable to update posted score for photo [" + imageScoreView.PhotoId + "] due to false return | Execution Date: " + DateTime.Now.ToString();
                    _auditLogsRepository.SystemAuditLog("Controller", "JudgeController_ImageScore", 0, logDescription, User.Claims.First().Value);
                    return Json(new { ServerCode = 410, Message = "Something went wrong. Try to refresh the page." });
                }
            }
            


        }


        public async Task<IActionResult> SearchPhoto()
        {
            var processParam = _processParameterRepository.GetProcessParameterByDetailedDescAndIsActive("Open", 1);
            var refCodeParam = _processParameterRepository.GetReferenceCodeByIsActiveAndRefTypeId(1, "05");


            if (processParam == null || refCodeParam == null)
            {
                return View("NoImageUploaded");
            }

            var round = refCodeParam.RefTypeID.ToString() + refCodeParam.RefCodeID.ToString();
            ViewBag.Round = localEncryption.ANEncrypt(round, key_);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SearchPhoto_Result([FromBody] SearchImage searchImage)
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var processParam = _processParameterRepository.GetProcessParameterByDetailedDescAndIsActive("Open", 1);
            var refCodeParam = _processParameterRepository.GetReferenceCodeByIsActiveAndRefTypeId(1, "05");
            var round = refCodeParam.RefTypeID.ToString() + refCodeParam.RefCodeID.ToString();

            if (processParam == null || refCodeParam == null)
            {
                return BadRequest(new { Errors = new List<string> { "Invalid Parameters" } });
            }


            if (string.IsNullOrEmpty(searchImage.SearchKey))
            {
                searchImage.SearchKey = "";
            }

            var regionList = _regionRepository.GetAllRegionView();
            var categoryList = _categoryRepository.GetAllCategoryView();

            List<MyRankScore> myRankScores = new List<MyRankScore>();

            foreach (var region in regionList)
            {
                foreach (var category in categoryList)
                {
                    var result_ = _rankRepository.GetMyRankScores(
                                                                    processParam.ParameterValue,
                                                                    int.Parse(processParam.Filler01),
                                                                    true,
                                                                    round,
                                                                    userIdClaim.Value,
                                                                    region.RegionId,
                                                                    category.CategoryId
                                                                    ).ToList();


                    myRankScores.AddRange(result_);
                }
            }









            // Filter photos based on the search key
            var filteredPhotos = myRankScores
                                    .Where(p => p.PhotoTitle.Contains(searchImage.SearchKey, StringComparison.OrdinalIgnoreCase)
                                                && p.PhotoTitle != "Not Available")
                                    .OrderBy(p => p.PhotoTitle)
                                    .ToList();

            return Ok(filteredPhotos);
        }





    }
}
