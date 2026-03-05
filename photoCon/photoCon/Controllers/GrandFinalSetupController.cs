using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using photoCon.Data;
using photoCon.Dto;
using photoCon.Helper;
using photoCon.Interface;
using photoCon.Models;
using System.Security.Claims;

namespace photoCon.Controllers
{
    [Authorize(Roles = "SYSADMIN,ECDUSER,APPADMIN")]
    public class GrandFinalSetupController : Controller
    {
        private readonly ILogger<GrandFinalSetupController> _logger;
        private readonly TallyProgramContext _tallyProgramContext;
        private readonly IProcessParameterRepository _processParameterRepository;
        private readonly IBatchImageRepository _batchImageRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IPhotoLocationsRepository _photoLocationsRepository;
        private readonly ISeedDatabaseRepository _seedDatabaseRepository;
        private readonly ConfigurationAppInfo _appInfo;
        LocalEncryption _localEncryption = new LocalEncryption();

        const string key = "thequickbrownfox";

        public GrandFinalSetupController(
                                            ILogger<GrandFinalSetupController> logger,
                                            IProcessParameterRepository processParameterRepository,
                                            IBatchImageRepository batchImageRepository,
                                            ICategoryRepository categoryRepository,
                                            IPhotoLocationsRepository photoLocationsRepository,
                                            TallyProgramContext tallyProgramContext,
                                            ISeedDatabaseRepository seedDatabaseRepository,
                                            IOptions<ConfigurationAppInfo> appInfo
                                        )
        {
            _logger = logger;
            _processParameterRepository = processParameterRepository;
            _batchImageRepository = batchImageRepository;
            _categoryRepository = categoryRepository;
            _photoLocationsRepository = photoLocationsRepository;
            _tallyProgramContext = tallyProgramContext;
            _seedDatabaseRepository = seedDatabaseRepository;
            _appInfo = appInfo.Value;
        }





        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetAllGrandFinalDates(int start, int length)
        {
            var query = _processParameterRepository.GetAllActiveDates("GrandFinalDate");
            var totalRecords = query.LongCount();
            var result = query.Skip(start).Take(length).ToList();
            return Ok(new { data = result, recordsTotal = totalRecords, recordsFiltered = totalRecords });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddGrandFinalDates([FromBody] PrelimFinalsDate dates_)
        {
            // Retrieve the user ID claim
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            string userId = null;
            if (userIdClaim != null)
            {
                userId = userIdClaim.Value;
                // Check if the model state is valid
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(new { Errors = errors });
                }

                if (dates_ == null)
                {
                    return BadRequest(new { Errors = new List<string> { "Date list cannot be empty." } });
                }

                //check if the date exist
                if (_processParameterRepository.IsExistProcessParameterByDate(dates_.prelimStartDate))
                {
                    return BadRequest(new { Errors = new List<string> { $"This Date already exist: {string.Join(", ", dates_.prelimStartDate)}" } });
                }

                try
                {


                    var existingPrelimActiveDates = _processParameterRepository.GetAllPrelimActiveDates();
                    var existingFinalActiveDates = _processParameterRepository.GetAllFinalActiveDates();
                    var newDate = DateTime.Parse(dates_.prelimStartDate);


                    if (existingPrelimActiveDates.Any()) //PrelimDates
                    {

                        var maxActiveDate = existingPrelimActiveDates.Max(e => DateTime.Parse(e.ParameterValue));

                        if (newDate < maxActiveDate)
                        {

                            return BadRequest(new { Errors = new List<string> { "You cannot add a date earlier than any regional dates. (OR) Please complete the regional round before adding grand final dates. " } });

                        }
                    }


                    if (existingFinalActiveDates.Any()) //FinalDates
                    {

                        var earliestActiveDate = existingFinalActiveDates.Min(e => DateTime.Parse(e.ParameterValue));

                        if (newDate < earliestActiveDate)
                        {
                            var earliestFinalDate = existingFinalActiveDates.First(e => DateTime.Parse(e.ParameterValue) == earliestActiveDate);

                            if (earliestFinalDate.DetailedDescription == "Open")
                            {
                                return BadRequest(new { Errors = new List<string> { "You cannot add a date earlier than any open dates." } });
                            }
                        }
                    }

                    //check if all prelimactive dates are completed if not do not proceed on adding the grandfinal date
                    if (!_processParameterRepository.IsAllActiveRegionalDateCompleted())
                    {
                        return BadRequest(new { Errors = new List<string> { "You cannot add grand final date. Please complete the regional round before adding grand final dates. " } });


                    }




                    // Check if the date is between the existing active dates
                    var newDateCode = (existingFinalActiveDates.Count + 1).ToString("00");
                    var newDateFiller01 = existingFinalActiveDates.Count + 1;
                    var point = false;
                    foreach (var existingDate in existingFinalActiveDates)
                    {
                        var existingDateTime = DateTime.Parse(existingDate.ParameterValue);

                        if (existingDateTime > newDate)
                        {
                            if (!point)
                            {
                                //get the value
                                newDateCode = existingDate.Code;
                                newDateFiller01 = int.Parse(existingDate.Filler01);

                                point = true;
                            }

                            // Adjust existing dates
                            existingDate.Code = (int.Parse(existingDate.Code) + 1).ToString("00");
                            existingDate.Filler01 = (int.Parse(existingDate.Filler01) + 1).ToString();

                            ////update the nextdates
                            _processParameterRepository.UpdateProcessParameterUsingIndex(existingDate.Index, existingDate.Code, existingDate.Filler01, userId);




                        }
                    }

                    // Save the new date
                    var newProcessParameter = new ProcessParameter
                    {
                        Process = "2",
                        Code = newDateCode,
                        Description = "GrandFinalDate",
                        DetailedDescription = "Close",
                        ParameterValue = newDate.Date.ToString("yyyy-MM-dd"),
                        IsActive = 1,
                        Filler01 = newDateFiller01.ToString(),
                        Filler02 = "False",
                        Filler03 = "0",
                        Filler04 = null,
                        Filler05 = null,
                        EffectivityDate = DateTime.Today,
                        CreatedBy = userIdClaim.Value,
                        CreatedDateTime = DateTime.Now,
                        ModifiedBy = null,
                        ModifiedDateTime = null
                    };

                    _processParameterRepository.CreateProcessParameter(newProcessParameter);




                    return Ok(new { Message = "GrandFinal Date added successfully." });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                    return StatusCode(500, new { Errors = new List<string> { "An error occurred while adding Grand Final date." } });
                }

            }
            else
            {
                // User ID claim not found, handle accordingly
                return View("Login", "Home");
            }


        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFinalDate([FromBody] GenericData data)
        {
            var id = data.HashId;

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

                var index = _localEncryption.ANDecrypt(id, key);
                if (!int.TryParse(index, out int parsedIndex))
                {
                    return BadRequest(new { Errors = new List<string> { "Data Manipulation" } });
                }

                if (!_processParameterRepository.CanDeleteFinalProcessParameter(parsedIndex))
                {
                    return BadRequest(new { Errors = new List<string> { "This record cannot be deleted based on its current status and parameters." } });
                }

                var dateInfo = _processParameterRepository.GetProcessParameterByIndex(parsedIndex);

                if (_batchImageRepository.IsThereARecordForThisDay(dateInfo.ParameterValue, int.Parse(dateInfo.Filler01)))
                {
                    return BadRequest(new { Errors = new List<string> { "This record cannot be deleted because there are images already uploaded." } });
                }

                if (_processParameterRepository.DeleteRecordByIndex(parsedIndex, userIdClaim.Value))
                {

                    //i need to update the records of the non deleted date, i need to update the code and filler01
                    //updating it 

                    var getAllActiveDates = _processParameterRepository.GetAllFinalActiveDates().OrderBy(a => a.Index);
                    var codeFiller = 1;
                    foreach (var item in getAllActiveDates)
                    {
                        //updatecode and filler01
                        _processParameterRepository.UpdateProcessParameterUsingIndex(item.Index, codeFiller.ToString("00"), codeFiller.ToString(), userIdClaim.Value);

                        codeFiller++;
                    }
                    return Ok(new { Message = "Record Deleted" });
                }
                else
                {
                    return StatusCode(500, new { Errors = new List<string> { "An error occurred while deleting grandfinal date." } });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                return StatusCode(500, new { Errors = new List<string> { "An error occurred while deleting Grand Final date." } });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDetailedDescription([FromBody] GenericData data)
        {
            var id = data.HashId;

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

                var index = _localEncryption.ANDecrypt(id, key);
                if (!int.TryParse(index, out int parsedIndex))
                {
                    return BadRequest(new { Errors = new List<string> { "Data Manipulation" } });
                }

                //check if all regional dates are completed
                if (!_processParameterRepository.CanOpenFinalDate())
                {
                    return BadRequest(new { Errors = new List<string> { "Cannot open the date. (1) Regional dates are not yet completed (2) There are other open dates." } });
                }

                if (_processParameterRepository.UpdateDetailedDescriptionByIndex(parsedIndex, userIdClaim.Value)) // 
                {
                    //update the reference code for the round reference -- opening the date will update 0501 to IsActive = True

                    //check if 0501 is already true
                    if (!_processParameterRepository.IsRoundReferenceTrue("06", "01"))
                    {
                        _processParameterRepository.UpdateRoundReferenceCode("06", "01");
                    }

                    return Ok(new { Message = "GrandFinal Date Open." });
                }
                else
                {
                    return StatusCode(500, new { Errors = new List<string> { "An error occurred while opening final date." } });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                return StatusCode(500, new { Errors = new List<string> { "An error occurred while updating Grand Final date." } });
            }

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OpenInfoEditModal([FromBody] GenericData data)
        {
            var id = data.HashId;

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

                var index = _localEncryption.ANDecrypt(id, key);
                if (!int.TryParse(index, out int parsedIndex))
                {
                    return BadRequest(new { Errors = new List<string> { "Data Manipulation" } });
                }

                var paramInfo = _processParameterRepository.GetProcessParameterByIndex(parsedIndex);
                var categoryList = _categoryRepository.GetAllCategoryView();
                var imageBatchCount = _batchImageRepository.SortCountPerDateAndDay(paramInfo.ParameterValue, int.Parse(paramInfo.Filler01));



                List<RegCatGenericView> categoryL = new List<RegCatGenericView>();

                foreach (var item in categoryList)
                {
                    categoryL.Add(new RegCatGenericView
                    {
                        HashId = _localEncryption.ANEncrypt(item.CategoryId.ToString(), key), // Assign appropriate values as needed
                        Id = item.CategoryId, // Assuming 'item' has properties Id, Name, etc.
                        Name = item.CategoryName,
                        BatchImageCount = _batchImageRepository.ImageBatchCountPerCategory(int.Parse(paramInfo.Filler01), paramInfo.ParameterValue, item.CategoryId),
                        PhotoLocationCount = _photoLocationsRepository.ImageCategoryCountGrandFinals(item.CategoryId),
                        Readonly = _batchImageRepository.ImageBatchCountPerCategoryGrandFinalNotToday(item.CategoryId, int.Parse(paramInfo.Filler01), paramInfo.ParameterValue)
                    });
                }

                var isUpdateEnabled = !_batchImageRepository.isUpdateButtonEnabledGrandFinal(int.Parse(paramInfo.Filler01), paramInfo.ParameterValue);
                var IsUpdateCloseEnabledGrandFinal = _processParameterRepository.IsUpdateCloseEnabledGrandFinal(paramInfo.Index);

                if (isUpdateEnabled && IsUpdateCloseEnabledGrandFinal)
                {
                    isUpdateEnabled = true;
                }
                else
                {
                    isUpdateEnabled = false;
                }




                //complete / close daily record status and value 
                return Ok(new
                {
                    paramInfo = paramInfo,
                    categoryList = categoryL,
                    imageBatchCount = imageBatchCount,
                    hashId = _localEncryption.ANEncrypt(paramInfo.Index.ToString(), key),
                    isUpdateEnabled,
                    IsUpdateCloseEnabledGrandFinal,




                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                return StatusCode(500, new { Errors = new List<string> { "An error occurred while editing date." } });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitBatch([FromBody] SubmitBatchGrandFinal submitBatch)
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



                var processParamIndex = _localEncryption.ANDecrypt(submitBatch.hashRowIdProcessParam, key);
                if (!int.TryParse(processParamIndex, out int parsedprocessParamIndex))
                {
                    return BadRequest(new { Errors = new List<string> { "Data Manipulation" } });
                }

                var paramInfo = _processParameterRepository.GetProcessParameterByIndex(parsedprocessParamIndex);
                int inclusion = 4;
                if (submitBatch.Inclusion)
                {
                    inclusion = 10;
                    //update process parameter filler 06 to true
                    _processParameterRepository.UpdateFiller08(parsedprocessParamIndex, "True", userIdClaim.Value.ToString());

                }
                else
                {
                    //update process parameter filler 06 to false
                    _processParameterRepository.UpdateFiller08(parsedprocessParamIndex, "False", userIdClaim.Value.ToString());


                }





                int decId;
                string decId_ = "";

                List<int> decCategoryIdList = new List<int>();
                foreach (var item in submitBatch.selectedCategories)
                {
                    decId_ = _localEncryption.ANDecrypt(item, key);
                    if (!int.TryParse(decId_, out decId))
                    {
                        return BadRequest(new { Errors = new List<string> { "Data Manipulation" } });
                    }
                    else
                    {
                        if (!_batchImageRepository.ImageBatchCountPerCategoryGrandFinalNotToday(decId, int.Parse(paramInfo.Filler01), paramInfo.ParameterValue))
                        {
                            decCategoryIdList.Add(decId);
                        }
                        
                    }
                }

                List<ImageBatch> batchImages = new List<ImageBatch>();
                var hashPhotoIds = await _batchImageRepository.GetHashPhotoIdsGFAsync(decCategoryIdList, inclusion);
                //hashPhotoIds = hashPhotoIds.Take(inclusion).ToList();

                var isUpdateEnabled = !_batchImageRepository.isUpdateButtonEnabledGrandFinal(int.Parse(paramInfo.Filler01), paramInfo.ParameterValue);
                var IsUpdateCloseEnabledGrandFinal = _processParameterRepository.IsUpdateCloseEnabledGrandFinal(paramInfo.Index);

                if (!(isUpdateEnabled))
                {
                    return BadRequest(new { Errors = new List<string> { "Error: Cannot  Update this Date Batch." } });
                }



                if (_batchImageRepository.IsThereARecordForThisDay(paramInfo.ParameterValue, int.Parse(paramInfo.Filler01)))
                {
                    //delete existing record
                    _batchImageRepository.DeleteBatchRecord(paramInfo.ParameterValue, int.Parse(paramInfo.Filler01));

                }

                int sort = 1;
                hashPhotoIds = hashPhotoIds.OrderBy(x => x.CategoryId).ThenBy(x => x.FileName).ToList();
                foreach (var hashPhotoId in hashPhotoIds)
                {
                    if (!_batchImageRepository.IsRecordExist(hashPhotoId.HashPhotoId, paramInfo.ParameterValue, int.Parse(paramInfo.Filler01), true))
                    {
                        batchImages.Add(new ImageBatch
                        {
                            ImageHashId = hashPhotoId.HashPhotoId,
                            Sort = sort,
                            Date = paramInfo.ParameterValue,
                            DayNumber = int.Parse(paramInfo.Filler01),
                            IsActive = true
                        });

                        sort++;
                    }

                }

                try
                {
                    await _batchImageRepository.SaveBatchImagesAsync(batchImages);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                    return BadRequest(new { Errors = new List<string> { "Cannot  Update this Date Batch. " + ex } });
                }

                return Ok(new
                {
                    ResponseCode = "200",
                    Message = "Added " + batchImages.Count() + " image for Day Number: " + paramInfo.Filler01 + " - Date: " + paramInfo.ParameterValue,



                });





            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                return StatusCode(500, new { Errors = new List<string> { "An error occurred while submitting batch image." } });

            }


        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CompleteDailyRound([FromBody] FinNumber model)
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

                string userId = userIdClaim.Value;
                var processParam = _processParameterRepository.GetProcessParameterByDetailedDescAndIsActive("Open", 1);
                var refCodeParam = _processParameterRepository.GetReferenceCodeByIsActiveAndRefTypeId(1, "06");
                var round = refCodeParam.RefTypeID.ToString() + refCodeParam.RefCodeID.ToString();


                var IsCompleteDailyRoundEnable = _processParameterRepository.IsCompleteDailyRoundGrandFinalEnable(processParam.Index);

                if (!IsCompleteDailyRoundEnable)
                {
                    return StatusCode(400, new { Errors = new List<string> { "Error: Daily round closed." } });
                }



                if (processParam == null || refCodeParam == null)
                {
                    return StatusCode(500, new { Errors = new List<string> { "Error in retrieving process parameters." } });

                }

                if (model == null || model.QualifyingNumber <= 0)
                {
                    return StatusCode(400, new { Errors = new List<string> { "Error: Invalid Parameter." } });
                }

                using (var transaction = _tallyProgramContext.Database.BeginTransaction())
                {
                    try
                    {
                        var categoryList = _categoryRepository.GetAllCategoryView();

                        List<MinAverageCategoryGrandFinal> top5Averages = new List<MinAverageCategoryGrandFinal>();
                        List<MinAverageCategoryGrandFinal> imageBatch = new List<MinAverageCategoryGrandFinal>();


                        
                        foreach (var category in categoryList)
                        {
                            int sort = 1; // Reset sort for each region and category
                            //getting the minAverage Per RegAndCat
                            var averageScores = _batchImageRepository
                                                .GetMinimumAverageCatGrandFinal(
                                                    category.CategoryId,
                                                    processParam.ParameterValue,
                                                    int.Parse(processParam.Filler01),
                                                    true,
                                                    round

                                                );

                            if (averageScores.Count >= model.QualifyingNumber)
                            {
                                var MinimumAverage = averageScores[model.QualifyingNumber - 1]; 
                                var minimumAverageNeeded = MinimumAverage.MinAverage;

                                top5Averages.Add(new MinAverageCategoryGrandFinal
                                {
                                    CategoryId = MinimumAverage.CategoryId,
                                    MinAverage = minimumAverageNeeded
                                });
                            }

                            //getting the imageBatchesPerRegionCatwithAverage
                            var imageAveList = _batchImageRepository
                                                .GetImageBatchesAveragePerCatGrandFinal(
                                                    category.CategoryId,
                                                    processParam.ParameterValue,
                                                    int.Parse(processParam.Filler01),
                                                    true,
                                                    round

                                                );

                            foreach (var imageAve in imageAveList)
                            {
                                imageBatch.Add(new MinAverageCategoryGrandFinal
                                {
                                    CategoryId = imageAve.CategoryId,
                                    MinAverage = imageAve.MinAverage,
                                    PhtoId = imageAve.PhtoId
                                });
                            }

                            var imagesInGroup = imageBatch
                            .Where(image => image.CategoryId == category.CategoryId)
                            .OrderByDescending(image => image.MinAverage)
                            .ToList();

                            int sort_ = 1; // Reset sort for each region and category
                            decimal preAve = 0.00m;
                            int lenGroup = imagesInGroup.Count;

                            foreach (var image in imagesInGroup)
                            {
                                var minAvg = top5Averages.FirstOrDefault(x =>  x.CategoryId == image.CategoryId);
                                var imageToUpdate = _tallyProgramContext.ImageBatches.FirstOrDefault(i => i.ImageHashId == image.PhtoId);
                                if (imageToUpdate != null)
                                {
                                   
                                    if (image.MinAverage >= minAvg.MinAverage) // --- 05292024
                                    {
                                        if (!CopyImageToWinnerFolder(imageToUpdate.ImageHashId, sort_))
                                        {
                                            transaction.Rollback();
                                            return StatusCode(400, new { Errors = new List<string> { "Error: Update cannot be completed." } });
                                        }

                                        if (sort_ == 1)
                                        {

                                            try
                                            {
                                                var x = imagesInGroup[sort_ + 1].MinAverage;
                                                if (x < image.MinAverage)
                                                {
                                                    sort_++;
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                                                sort_++;
                                            }

                                        }
                                        else
                                        {
                                            if (preAve > image.MinAverage)
                                            {
                                                sort_++;
                                            }
                                        }


                                        
                                    }

                                    imageToUpdate.Sort = 0;
                                    imageToUpdate.IsActive = false;

                                    _tallyProgramContext.Update(imageToUpdate);
                                }

                                preAve = image.MinAverage;
                            }


                        }
                        

                        _tallyProgramContext.SaveChanges();

                        //update the paramValues and refCode
                        //closing initial round

                        if (_processParameterRepository.CloseDailyGrandFinalRoundByIndex(
                                                    processParam.Index,
                                                    refCodeParam.Index,
                                                    model.QualifyingNumber.ToString(),
                                                    userId
                                            ))
                        {
                            transaction.Commit();

                            //copy the images to the grandfinals folder
                            //Top4 and Top6


                            return Ok(new
                            {
                                ResponseCode = "200",
                                Message = "Update Completed. Closing Initial Round. Opening Daily Final Round."
                            });
                        }
                        else
                        {

                            transaction.Rollback();
                            return StatusCode(400, new { Errors = new List<string> { "Error: Update cannot be completed." } });

                        }

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                        transaction.Rollback();
                        //return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
                        return StatusCode(400, new { Errors = new List<string> { "Error: Update cannot be completed." } });

                    }
                }



            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                return StatusCode(500, new { Errors = new List<string> { "An error occurred while submitting batch image." } });

            }

        }

        private bool CopyImageToWinnerFolder(string photoId, int sort)
        {

            var imageInfo = _photoLocationsRepository.CopyImageInformation(photoId);
            string RootURL = _appInfo.AppURLRepository + @"4_Winners/";  
            var imagePartialPath = _appInfo.AppRepository + "4_Winners\\";
            var imgFullFilePath = imagePartialPath + imageInfo[0].CategoryName + "\\" + imageInfo[0].FileName;
            var fullUrl = RootURL + imageInfo[0].CategoryName + @"/" + imageInfo[0].FileName;


            if (imageInfo != null)
            {
                string imageCategoryName = imageInfo[0].CategoryName;
                var imageFullPath = imagePartialPath + imageCategoryName + @"\";
                string sourceFilePath = imageInfo[0].RepositoryLocation;



                try
                {
                    if (!System.IO.File.Exists(sourceFilePath))
                    {
                        return false;
                    }

                    try
                    {
                        string fileName = imageInfo[0].FileName;
                        //System.IO.File.Copy(sourceFilePath, imageFullPath);

                        if (!Directory.Exists(Path.GetDirectoryName(imgFullFilePath)))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(imgFullFilePath));
                        }

                        if (!System.IO.File.Exists(imgFullFilePath))
                        {
                            System.IO.File.Copy(sourceFilePath, imgFullFilePath);
                        }



                        var photoLocation_ = new PhotoLocation
                        {
                            HashPhotoId = _localEncryption.ANEncrypt(imgFullFilePath.ToString(), key),
                            RepositoryLocation = imgFullFilePath,
                            RegionId = imageInfo[0].RegionId,
                            CategoryId = imageInfo[0].CategoryId,
                            PhotoCode = "30",
                            Filler01 = sort.ToString(),
                            EncodedBy = null,
                            DateTimeEncoded = DateTime.Now

                        };

                        var photoMetadata_ = new PhotoMetaDatum
                        {
                            HashPhotoId = photoLocation_.HashPhotoId,
                            FileName = imageInfo[0].FileName,
                            Dimension = imageInfo[0].Dimension,
                            Width = imageInfo[0].Width,
                            Height = imageInfo[0].Height,
                            HorizontalResolution = imageInfo[0].HorizontalResolution,
                            VerticalResolution = imageInfo[0].VerticalResolution,
                            BitDepth = imageInfo[0].BitDepth,
                            ResolutionUnit = imageInfo[0].ResolutionUnit,
                            ImageUrl = fullUrl,
                            CameraMaker = imageInfo[0].CameraMaker,
                            CameraModel = imageInfo[0].CameraModel,
                            Fstop = imageInfo[0].FStop,
                            ExposureTime = imageInfo[0].ExposureTime,
                            Isospeed = imageInfo[0].ISOSpeed,
                            FocalLength = imageInfo[0].FocalLength,
                            MaxAperture = imageInfo[0].MaxAperture,
                            MeteringMode = imageInfo[0].MeteringMode,
                            FlashMode = imageInfo[0].FlashMode,
                            MmFocalLength = imageInfo[0].MmFocalLength
                        };

                        _seedDatabaseRepository.CreatePhotoLocation(photoLocation_);
                        _seedDatabaseRepository.CreatePhotoMetadata(photoMetadata_);

                        return true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                        return false;
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                    return false;
                }
            }

            return false;
        }


    }
}
