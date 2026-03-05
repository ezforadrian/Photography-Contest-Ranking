using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using photoCon.Data;
using photoCon.Dto;
using photoCon.Helper;
using photoCon.Interface;
using photoCon.Models;
using System.Dynamic;
using System.Security.Claims;

namespace photoCon.Controllers
{
    [Authorize(Roles = "SYSADMIN,ECDUSER,APPADMIN")]
    public class JudgeGrandFinalController : Controller
    {

        private readonly ILogger<JudgeGrandFinalController> _logger;
        private readonly IJudgeRepository _judgeRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProcessParameterRepository _processParameterRepository;
        private readonly IBatchImageRepository _batchImageRepository;
        private readonly TallyProgramContext _tallyProgramContext;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAuditLogsRepository _auditLogsRepository;
        private ApplicationDbContext _application;

        LocalEncryption localEncryption = new LocalEncryption();

        const string key_ = "thequickbrownfox";

        private const string SessionId = "_xId";
        private const string SessionName = "_xName";


        public JudgeGrandFinalController(ILogger<JudgeGrandFinalController> logger,
                                IJudgeRepository judgeRepository,
                                UserManager<ApplicationUser> userManager,
                                IProcessParameterRepository processParameterRepository,
                                IBatchImageRepository batchImageRepository,
                                TallyProgramContext tallyProgramContext,
                                ApplicationDbContext application, 
                                RoleManager<IdentityRole> roleManager,
                                IAuditLogsRepository auditLogsRepository)
                                
        {
            _logger = logger;
            _judgeRepository = judgeRepository;
            _userManager = userManager;
            _processParameterRepository = processParameterRepository;
            _batchImageRepository = batchImageRepository;
            _tallyProgramContext = tallyProgramContext;
            _application = application;
            _roleManager = roleManager;
            _auditLogsRepository = auditLogsRepository;
        }

        public ActionResult ImitateJudge()
        {

            // Get the role "JUDGE" or "Judge"
            var judgeRole = _roleManager.Roles.FirstOrDefault(r => r.Name.ToUpper() == "JUDGE");

            if (judgeRole == null)
            {
                // Handle the case where the role does not exist
                return NotFound("Judge role not found.");
            }

            // Get the users with the role "JUDGE" or "Judge"
            var judgeUsers = _application.Users
                .Where(u => _application.UserRoles.Any(ur => ur.RoleId == judgeRole.Id && ur.UserId == u.Id))
                .Select(u => new JudgeView { FirstName = u.FirstName, LastName = u.LastName, MiddleName = u.MiddleName, Id = u.Id })
                .OrderBy(u => u.LastName).ThenBy(u => u.FirstName).ThenBy(u => u.MiddleName)
                .ToList();

            return View(judgeUsers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ValidateJudgeId(string judgeId)
        {

            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }


            // Verify the submitted judgeId
            var judgeUser = _application.Users
                .FirstOrDefault(u => u.Id == judgeId);

            if (judgeUser == null)
            {
                // Handle the case where the user does not exist
                return NotFound("Judge not found.");
            }

            var judgeRole = _roleManager.Roles.FirstOrDefault(r => r.Name.ToUpper() == "JUDGE");

            if (judgeRole == null)
            {
                // Handle the case where the role does not exist
                return NotFound("Judge role not found.");
            }

            var isJudge = _application.UserRoles.Any(ur => ur.RoleId == judgeRole.Id && ur.UserId == judgeId);

            if (!isJudge)
            {
                // Handle the case where the user does not have the "JUDGE" role
                return Unauthorized("User does not have the judge role.");
            }

            var xFullname = judgeUser.FirstName.ToString() + " " + judgeUser.LastName.ToString();
            HttpContext.Session.SetString(SessionId, judgeId);
            HttpContext.Session.SetString(SessionName, xFullname);


            return Redirect("Index");
        }



        public IActionResult Index()
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                // User ID claim not found, handle accordingly
                return RedirectToAction("Login", "Home");
            }

            string userId = userIdClaim.Value;

            // Validate session
            var sessionJudgeId = HttpContext.Session.GetString(SessionId);
            var sessionJudgeName = HttpContext.Session.GetString(SessionName);

            if (string.IsNullOrEmpty(sessionJudgeId) || string.IsNullOrEmpty(sessionJudgeName))
            {
                // Session values are not set, handle accordingly
                return RedirectToAction("Login", "Home");
                //return Ok(JsonConvert.SerializeObject(sessionJudgeId));
            }

            var processParam = _processParameterRepository.GetProcessParameterByDetailedDescAndIsActive("Open", 1);
            var refCodeParam = _processParameterRepository.GetReferenceCodeByIsActiveAndRefTypeId(1, "06");

            if (processParam == null || refCodeParam == null)
            {
                return View("NoImageUploaded");
            }

            var round = localEncryption.ANEncrypt((refCodeParam.RefTypeID.ToString() + refCodeParam.RefCodeID.ToString()), key_);


            var judgeImageViews = _batchImageRepository.GetImagesByParamValues(
                processParam.ParameterValue,
                processParam.Filler01,
                processParam.IsActive,
                true, "20"
            );

            var totalCount = judgeImageViews.Count;
            ViewBag.TotalCount = totalCount;
            ViewBag.Round = round;

            judgeImageViews = judgeImageViews.Take(1).ToList();

            if (judgeImageViews.Count < 1)
            {
                return View("NoImageUploaded");
            }

            //get the criteria
            var refCodeCriteria = _processParameterRepository.GetCriteria();

            string imageId = judgeImageViews[0].ImageBatch_ImageHashId;
            var judgeScores = _judgeRepository.GetImageScoreGrandFinalByJudgeAndRound(sessionJudgeId, refCodeParam.RefTypeID, refCodeParam.RefCodeID, imageId);

            dynamic multipleModel = new ExpandoObject();
            multipleModel.judgeImageViews = judgeImageViews;
            multipleModel.judgeScores = judgeScores;
            multipleModel.refCodeCriteria = refCodeCriteria;


            return View(multipleModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NextPhoto([FromBody] DisplayUpdate displayUpdate)
        {

            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            // Validate session
            var sessionJudgeId = HttpContext.Session.GetString(SessionId);
            var sessionJudgeName = HttpContext.Session.GetString(SessionName);

            if (string.IsNullOrEmpty(sessionJudgeId) || string.IsNullOrEmpty(sessionJudgeName))
            {
                // Session values are not set, handle accordingly
                return RedirectToAction("Login", "Home");
            }

            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Errors = new List<string> { "Invalid parameter." } });
                }

                //ProcessParameter --get the Open and IsActive 1
                var processParam = _processParameterRepository.GetProcessParameterByDetailedDescAndIsActive("Open", 1);

                //----What Round?
                var refCodeParam = _processParameterRepository.GetReferenceCodeByIsActiveAndRefTypeId(1, "06");


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


                var dailyRoundCount = _batchImageRepository.CountActiveImageForTheDay(processParam.ParameterValue
                                        , processParam.Filler01
                                        , processParam.IsActive, true);

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
                                        , processParam.IsActive, true, "20"
                                     ).OrderBy(x => x.ImageBatch_Sort).Skip(parsedrownumberEn).FirstOrDefault();
                if (judgeImageView == null)
                {
                    return View("NoImageUploaded");
                }
                judgeImageView.PhotoMetaData_HashPhotoID = localEncryption.ANEncrypt(judgeImageView.ImageBatch_Sort.ToString(), key_);



                string imageId = judgeImageView.ImageBatch_ImageHashId;


                var userId = userIdClaim.Value;
                List<ImageScoreGrandFinal> judgeScores = new List<ImageScoreGrandFinal>();


                //judgeScores = _judgeRepository.GetAllJudgeScore(userId, refCodeParam.RefTypeID, refCodeParam.RefCodeID);




                //get the criteria
                var refCodeCriteria = _processParameterRepository.GetCriteria();

                judgeScores = _judgeRepository.GetImageScoreGrandFinalByJudgeAndRound(sessionJudgeId, refCodeParam.RefTypeID, refCodeParam.RefCodeID, imageId);


                var jsonData = JsonConvert
                                .SerializeObject(
                                    new
                                    {
                                        judgeImageView = judgeImageView
                                        ,
                                        judgeScores = judgeScores
                                        ,
                                        refCodeCriteria = refCodeCriteria

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
        public async Task<IActionResult> PrevPhoto([FromBody] DisplayUpdate displayUpdate)
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            // Validate session
            var sessionJudgeId = HttpContext.Session.GetString(SessionId);
            var sessionJudgeName = HttpContext.Session.GetString(SessionName);

            if (string.IsNullOrEmpty(sessionJudgeId) || string.IsNullOrEmpty(sessionJudgeName))
            {
                // Session values are not set, handle accordingly
                return RedirectToAction("Login", "Home");
            }

            try
            {
                //ProcessParameter --get the Open and IsActive 1
                var processParam = _processParameterRepository.GetProcessParameterByDetailedDescAndIsActive("Open", 1);

                //----What Round?
                var refCodeParam = _processParameterRepository.GetReferenceCodeByIsActiveAndRefTypeId(1, "06");

                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Errors = new List<string> { "Invalid parameter." } });
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





                var dailyRoundCount = _batchImageRepository.CountActiveImageForTheDay(processParam.ParameterValue
                                        , processParam.Filler01
                                        , processParam.IsActive, true);

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
                                        , processParam.IsActive, true, "20"
                                     ).OrderBy(x => x.ImageBatch_Sort).Skip(parsedrownumberEn - 2).FirstOrDefault();

                if (judgeImageView == null)
                {
                    return View("NoImageUploaded");
                }

                judgeImageView.PhotoMetaData_HashPhotoID = localEncryption.ANEncrypt(judgeImageView.ImageBatch_Sort.ToString(), key_);

                string imageId = judgeImageView.ImageBatch_ImageHashId;

                //get the criteria
                var refCodeCriteria = _processParameterRepository.GetCriteria();

                var userId = userIdClaim.Value;
                List<ImageScoreGrandFinal> judgeScores = new List<ImageScoreGrandFinal>();
                //judgeScores = _judgeRepository.GetAllJudgeScore(userId, refCodeParam.RefTypeID, refCodeParam.RefCodeID);

                judgeScores = _judgeRepository.GetImageScoreGrandFinalByJudgeAndRound(sessionJudgeId, refCodeParam.RefTypeID, refCodeParam.RefCodeID, imageId);



                var jsonData = JsonConvert
                                .SerializeObject(
                                    new
                                    {
                                        judgeImageView = judgeImageView
                                        ,judgeScores = judgeScores
                                        ,refCodeCriteria = refCodeCriteria

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
        public async Task<IActionResult> ImageScore([FromBody] ImageScoreGrandFinalView imageScoreView)
        {
            var ProcessStatus = 0;
            string ProcessMessage = "";
            string logDescription = "";

            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            // Validate session
            var sessionJudgeId = HttpContext.Session.GetString(SessionId);
            var sessionJudgeName = HttpContext.Session.GetString(SessionName);

            if (string.IsNullOrEmpty(sessionJudgeId) || string.IsNullOrEmpty(sessionJudgeName))
            {
                // Session values are not set, handle accordingly
                return RedirectToAction("Login", "Home");
            }

            //ProcessParameter --get the Open and IsActive 1
            var processParam = _processParameterRepository.GetProcessParameterByDetailedDescAndIsActive("Open", 1);

            //----What Round?
            var refCodeParam = _processParameterRepository.GetReferenceCodeByIsActiveAndRefTypeId(1, "06");


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



            if (imageScoreView.RowNumber < 0)
            {
                return BadRequest(new { Errors = new List<string> { "Invalid row number. Row number must be non-negative." } });

            }

            if (imageScoreView.RowNumber > dailyRoundCount)
            {
                return BadRequest(new { Errors = new List<string> { "Invalid row number. " } });

            }

            // Calculate the total score and count of scores
            decimal totalScore = 0;
            int scoreCount = 0;

            foreach (var scoreData in imageScoreView.ScoreData)
            {
                // Add the score to the total
                totalScore += scoreData.Score;

                // Increment the score count
                scoreCount++;
            }

            // Calculate the average score
            decimal averageScore = scoreCount > 0 ? totalScore : 0;

            // Begin transaction
            using (var transaction = _tallyProgramContext.Database.BeginTransaction())
            {


                try
                {
                    if (!_judgeRepository.IsImageScoreGrandFinalByUser(sessionJudgeId, imageScoreView.PhotoId, refCodeParam.RefTypeID.ToString(), refCodeParam.RefCodeID.ToString()))
                    {
                        // Image score does not exist, so add new records for each criteria
                        foreach (var scoreData in imageScoreView.ScoreData)
                        {
                            var imageScoreGrandFinal = new ImageScoreGrandFinal
                            {
                                PhotoId = imageScoreView.PhotoId,
                                UserId = sessionJudgeId,
                                Round = refCodeParam.RefTypeID.ToString() + refCodeParam.RefCodeID.ToString(),
                                Criteria = scoreData.Criteria,
                                Score = scoreData.Score,
                                LastUpdateDate = DateTime.Now
                            };

                            // Add the new record to the database
                            _judgeRepository.SaveScoreByJudgeInImagePerCriteria(imageScoreGrandFinal);

                            logDescription = "Success | Add Criteria | Criteria Score [" + scoreData.Criteria + "] for photo [" + imageScoreView.PhotoId + "] has been updated to [" + scoreData.Score + "] by User [" + sessionJudgeId + "] | Execution Date: " + DateTime.Now.ToString();
                            _auditLogsRepository.SystemAuditLog("Controller", "JudgeGrandFinalController_ImageScore", 0, logDescription, User.Claims.First().Value);
                        }
                    }
                    else
                    {
                        // Image score exists, so update existing records for each criteria
                        foreach (var scoreData in imageScoreView.ScoreData)
                        {
                            var imageScoreGrandFinal = new ImageScoreGrandFinal
                            {
                                PhotoId = imageScoreView.PhotoId,
                                UserId = sessionJudgeId,
                                Round = refCodeParam.RefTypeID.ToString() + refCodeParam.RefCodeID.ToString(),
                                Criteria = scoreData.Criteria,
                                Score = scoreData.Score,
                                LastUpdateDate = DateTime.Now
                            };

                            // Update the existing record in the database
                            _judgeRepository.UpdateScoreByJudgeInImagePerCriteria(imageScoreGrandFinal);

                            logDescription = "Success | Update Criteria | Posted Score Criteria [" + scoreData.Criteria + "] for photo [" + imageScoreView.PhotoId + "] has been updated to [" + scoreData.Score + "] by User [" + sessionJudgeId + "] | Execution Date: " + DateTime.Now.ToString();
                            _auditLogsRepository.SystemAuditLog("Controller", "JudgeGrandFinalController_ImageScore", 0, logDescription, User.Claims.First().Value);
                        }
                    }

                    // Commit transaction if all operations succeed
                    await _tallyProgramContext.SaveChangesAsync();
                    transaction.Commit();

                    // Return JSON upon successful completion
                    return Json(new { ServerCode = 200,
                        Message = "Image scores updated successfully."                                 ,
                        AverageScore = averageScore
                    });

                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                    // Roll back transaction if a database update error occurs
                    transaction.Rollback();

                    // Log the exception for debugging purposes
                    Console.WriteLine($"DbUpdateException occurred: {ex}");

                    // Determine the specific causes of the error
                    foreach (var entry in ex.Entries)
                    {
                        Console.WriteLine($"Entity of type {entry.Entity.GetType().Name} in state {entry.State} could not be updated");
                    }

                    // Handle the error accordingly, e.g., return an error response
                    return StatusCode(500, new { Errors = new List<string> { "An error occurred while updating image scores." } });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                    // Roll back transaction if an error occurs
                    transaction.Rollback();

                    // Log the exception for debugging purposes
                    Console.WriteLine($"Exception occurred: {ex}");

                    // Handle the error accordingly, e.g., return an error response
                    return StatusCode(500, new { Errors = new List<string> { "An error occurred while updating image scores." } });
                }
            }



        }
    }
}
