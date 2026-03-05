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
    public class InitialSetupController : Controller
    {

        
        private readonly TallyProgramContext _tallyProgramContext;
        private readonly ILogger<InitialSetupController> _logger;
        private readonly IProcessParameterRepository _processParameterRepository;
        private readonly IBatchImageRepository _batchImageRepository;
        private readonly IRegionRepository _regionRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IPhotoLocationsRepository _photoLocationsRepository;
        private readonly ISeedDatabaseRepository _seedDatabaseRepository;
        private readonly ConfigurationAppInfo _appInfo;

        LocalEncryption _localEncryption = new LocalEncryption();


        const string key = "thequickbrownfox";

        public InitialSetupController(
                                        ILogger<InitialSetupController> logger,
                                        IProcessParameterRepository processParameterRepository,
                                        IBatchImageRepository batchImageRepository,
                                        IRegionRepository regionRepository,
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
            _regionRepository = regionRepository;
            _categoryRepository = categoryRepository;
            _photoLocationsRepository = photoLocationsRepository;
            _tallyProgramContext = tallyProgramContext;
            _seedDatabaseRepository = seedDatabaseRepository;
            _appInfo = appInfo.Value;
        }

        public async Task<IActionResult> Index()
        {
            


            return View();
        }

        [HttpGet]
        public IActionResult GetAllPrelimDates(int start, int length)
        {
           
            var query = _processParameterRepository.GetAllActiveDates("PrelimDate");

            var totalRecords = query.LongCount();

            var result = query
                        .Skip(start)
                        .Take(length)
                        .ToList();


            return Ok(new { data = result, recordsTotal = totalRecords, recordsFiltered = totalRecords });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPrelimDates([FromBody] PrelimFinalsDate dates_)
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
                    
                    
                    var existingActiveDates = _processParameterRepository.GetAllPrelimActiveDates();
                    var newDate = DateTime.Parse(dates_.prelimStartDate);

                    // Additional checking: check if the date is less than the earliest active date
                    if (existingActiveDates.Any())
                    {
                        //----Additional checking needed, check if the date is less than the earliest active date, now if it is true, check if the current earliest date is open,
                        //------if it is open, throw an error that you cannot add a date earlier than the current open date. 
                        var earliestActiveDate = existingActiveDates.Min(e => DateTime.Parse(e.ParameterValue));

                        if (newDate < earliestActiveDate)
                        {
                            var earliestActiveEntry = existingActiveDates.First(e => DateTime.Parse(e.ParameterValue) == earliestActiveDate);

                            if (earliestActiveEntry.DetailedDescription == "Open" || earliestActiveEntry.Filler06 == "True" || earliestActiveEntry.Filler04 == "True" || earliestActiveEntry.Filler02 == "True")
                            {
                                return BadRequest(new { Errors = new List<string> { "You cannot add a date earlier than the current open date or completed regional date." } });
                            }
                        }
                    }


                    //is there an existing grand final date????
                    if (_processParameterRepository.IsThereAActiveGrandFinalDate())
                    {
                        return BadRequest(new { Errors = new List<string> { "You cannot add a regional date when there is an existing grand final date." } });
                    }


                    // Check if the date is between the existing active dates
                    var newDateCode = (existingActiveDates.Count + 1).ToString("00");
                    var newDateFiller01 = existingActiveDates.Count + 1;
                    var point = false;
                    foreach (var existingDate in existingActiveDates)
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
                            _processParameterRepository.UpdateProcessParameterUsingIndex(existingDate.Index, existingDate.Code, existingDate.Filler01,userId);




                        }
                    }

                    // Save the new date
                    var newProcessParameter = new ProcessParameter
                    {
                        Process = "1",
                        Code = newDateCode,
                        Description = "PrelimDate",
                        DetailedDescription = "Close",
                        ParameterValue = newDate.Date.ToString("yyyy-MM-dd"),
                        IsActive = 1,
                        Filler01 = newDateFiller01.ToString(),
                        Filler02 = "False",
                        Filler03 = "0",
                        Filler04 = "False",
                        Filler05 = "0",
                        Filler06 = "False",
                        Filler07 = "0",
                        EffectivityDate = DateTime.Today,
                        CreatedBy = userIdClaim.Value,
                        CreatedDateTime = DateTime.Now,
                        ModifiedBy = null,
                        ModifiedDateTime = null
                    };

                    _processParameterRepository.CreateProcessParameter(newProcessParameter);

                 
          

                    return Ok(new { Message = "Regional Date added successfully." });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                    return StatusCode(500, new { Errors = new List<string> { "An error occurred while adding regional date." } });
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
        public async Task<IActionResult> DeletePrelimDate([FromBody] GenericData data)
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

                if (!_processParameterRepository.CanDeleteProcessParameter(parsedIndex))
                {
                    return BadRequest(new { Errors = new List<string> { "This record cannot be deleted based on its current status and parameters." } });
                }

                var dateInfo = _processParameterRepository.GetProcessParameterByIndex(parsedIndex);

                if (_batchImageRepository.IsThereARecordForThisDay(dateInfo.ParameterValue, int.Parse(dateInfo.Filler01)))
                {
                    return BadRequest(new { Errors = new List<string> { "This record cannot be deleted based on its current status and parameters." } });
                }

                if (_processParameterRepository.DeleteRecordByIndex(parsedIndex, userIdClaim.Value))
                {

                    //i need to update the records of the non deleted date, i need to update the code and filler01
                    //updating it 

                    var getAllActiveDates = _processParameterRepository.GetAllPrelimActiveDates().OrderBy(a => a.Index);
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
                    return StatusCode(500, new { Errors = new List<string> { "An error occurred while deleting regional date." } });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                return StatusCode(500, new { Errors = new List<string> { "An error occurred while deleting regional date." } });
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


                if (_processParameterRepository.UpdateDetailedDescriptionByIndex(parsedIndex, userIdClaim.Value)) // 
                {
                    //update the reference code for the round reference -- opening the date will update 0501 to IsActive = True

                    //check if 0501 is already true
                    if (!_processParameterRepository.IsRoundReferenceTrue("05", "01"))
                    {
                        _processParameterRepository.UpdateRoundReferenceCode("05", "01");
                    }

                    return Ok(new { Message = "Regional Date Open." });
                }
                else
                {
                    return StatusCode(500, new { Errors = new List<string> { "An error occurred while opening regional date." } });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                return StatusCode(500, new { Errors = new List<string> { "An error occurred while opening regional date." } });
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
                var regionList = _regionRepository.GetAllRegionView();
                var categoryList = _categoryRepository.GetAllCategoryView();
                var imageBatchCount = _batchImageRepository.SortCountPerDateAndDay(paramInfo.ParameterValue, int.Parse(paramInfo.Filler01));

                List<RegCatGenericView> regionL = new List<RegCatGenericView>();

                foreach (var item in regionList)
                {
                    regionL.Add(new RegCatGenericView
                    {
                        HashId = _localEncryption.ANEncrypt(item.RegionId.ToString(), key), // Assign appropriate values as needed
                        Id = item.RegionId, // Assuming 'item' has properties Id, Name, etc.
                        Name = item.RegionName,
                        BatchImageCount = _batchImageRepository.ImageBatchCountPerRegion(int.Parse(paramInfo.Filler01), paramInfo.ParameterValue, item.RegionId),
                        PhotoLocationCount = _photoLocationsRepository.ImageRegionCount(item.RegionId),
                        Readonly = _batchImageRepository.ImageBatchCountPerRegionNotToday(item.RegionId, int.Parse(paramInfo.Filler01), paramInfo.ParameterValue)

                    });
                }

                List<RegCatGenericView> categoryL = new List<RegCatGenericView>();

                foreach (var item in categoryList)
                {
                    categoryL.Add(new RegCatGenericView
                    {
                        HashId = _localEncryption.ANEncrypt(item.CategoryId.ToString(), key), // Assign appropriate values as needed
                        Id = item.CategoryId, // Assuming 'item' has properties Id, Name, etc.
                        Name = item.CategoryName,
                        BatchImageCount = _batchImageRepository.ImageBatchCountPerCategory(int.Parse(paramInfo.Filler01), paramInfo.ParameterValue, item.CategoryId),
                        PhotoLocationCount = _photoLocationsRepository.ImageCategoryCount(item.CategoryId)
                    });
                }

                var isUpdateEnabled = !_batchImageRepository.isUpdateButtonEnabled(int.Parse(paramInfo.Filler01), paramInfo.ParameterValue);
                var isUpdateEnableByParam = _processParameterRepository.IsUpdateEnabledByFiller02(paramInfo.Index);

                if (isUpdateEnabled && isUpdateEnableByParam)
                {
                    isUpdateEnabled = true;
                }
                else
                {
                    isUpdateEnabled = false;
                }

                var IsCloseInitialRoundEnable = _processParameterRepository.IsCloseInitialRoundEnable(paramInfo.Index, 1); //RoundOne
                var IsCloseInitialRoundTwoEnable = _processParameterRepository.IsCloseInitialRoundEnable(paramInfo.Index, 2); //RoundTwo
                var IsCompleteDailyRoundEnable = _processParameterRepository.IsCloseInitialRoundEnable(paramInfo.Index, 3); //RoundThree

                var CloseInitialRoundValue = _processParameterRepository.CloseInitialRoundValue(paramInfo.Index, 1); //RoundOne
                var CloseInitialRoundTwoValue = _processParameterRepository.CloseInitialRoundValue(paramInfo.Index, 2); //RoundTwo
                var CompleteDailyRoundValue = _processParameterRepository.CloseInitialRoundValue(paramInfo.Index, 3); //RoundThree

                //closeInitial status and value

                //complete / close daily record status and value 
                return Ok(new
                {
                    paramInfo = paramInfo,
                    regionList = regionL,
                    categoryList = categoryL,
                    imageBatchCount = imageBatchCount,
                    hashId = _localEncryption.ANEncrypt(paramInfo.Index.ToString(), key),
                    isUpdateEnabled,
                    IsCloseInitialRoundEnable,
                    CloseInitialRoundValue,
                    IsCloseInitialRoundTwoEnable,
                    CloseInitialRoundTwoValue,
                    IsCompleteDailyRoundEnable,
                    CompleteDailyRoundValue




                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                return StatusCode(500, new { Errors = new List<string> { "An error occurred while opening regional date." } });
            }
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitBatch([FromBody] SubmitBatch submitBatch) 
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

                
                


                int decId;
                string decId_ = "";

                List<int> decRegionIdList = new List<int>();
                List<int> decCategoryIdList = new List<int>();

                foreach (var item in submitBatch.selectedRegions)
                {
                    decId_ = _localEncryption.ANDecrypt(item, key);
                    if (!int.TryParse(decId_, out decId))
                    {
                        return BadRequest(new { Errors = new List<string> { "Data Manipulation" } });
                    }
                    else
                    {
                        //check first id the decId exist already in the imageBatch or has already a record
                        if (!_batchImageRepository.ImageBatchCountPerRegionNotToday(decId, int.Parse(paramInfo.Filler01), paramInfo.ParameterValue))
                        {
                            decRegionIdList.Add(decId);
                        }
                        
                    }
                }

                foreach (var item in submitBatch.selectedCategories)
                {
                    decId_ = _localEncryption.ANDecrypt(item, key);
                    if (!int.TryParse(decId_, out decId))
                    {
                        return BadRequest(new { Errors = new List<string> { "Data Manipulation" } });
                    }
                    else
                    {
                        decCategoryIdList.Add(decId);
                    }
                }

                List<ImageBatch> batchImages = new List<ImageBatch>();
                var hashPhotoIds = await _batchImageRepository.GetHashPhotoIdsAsync(decRegionIdList, decCategoryIdList);

                var isUpdateEnabled = !_batchImageRepository.isUpdateButtonEnabled(int.Parse(paramInfo.Filler01), paramInfo.ParameterValue);
                var isUpdateEnableByParam = _processParameterRepository.IsUpdateEnabledByFiller02(paramInfo.Index);

                if (!(isUpdateEnabled && isUpdateEnableByParam))
                {
                    return BadRequest(new { Errors = new List<string> { "Error: Cannot  Update this Date Batch." } });
                }
                //var isUpdateEnabled = !_batchImageRepository.isUpdateButtonEnabled(int.Parse(paramInfo.Filler01), paramInfo.ParameterValue);
                //if (!isUpdateEnabled) //means hindi na pwede mag update kasi meron ng na score
                //{
                //    return BadRequest(new { Errors = new List<string> { "Cannot  Update this Date Batch." } });
                //}


                if (_batchImageRepository.IsThereARecordForThisDay(paramInfo.ParameterValue, int.Parse(paramInfo.Filler01)))
                {
                    //delete existing record
                    _batchImageRepository.DeleteBatchRecord(paramInfo.ParameterValue, int.Parse(paramInfo.Filler01));

                }

                int sort = 1;
                hashPhotoIds = hashPhotoIds.OrderBy(x => x.RegionId).ThenBy(x => x.CategoryId).ThenBy(x => x.FileName).ToList();
                int prevCat = hashPhotoIds[0].CategoryId;
                foreach (var hashPhotoId in hashPhotoIds)
                {
                    if (prevCat != hashPhotoId.CategoryId)
                    {
                        sort = 1;
                    }
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

                    
                    prevCat = hashPhotoId.CategoryId;
                }

         
                try
                {
                    await _batchImageRepository.SaveBatchImagesAsync(batchImages);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                    return BadRequest(new { Errors = new List<string> { "Cannot  Update this Date Batch. " } });
                }

                return Ok(new
                {
                    ResponseCode = "200",
                    Message = "Added " + batchImages.Count() + " image for Day Number: " + paramInfo.Filler01 + " - Date: " + paramInfo.ParameterValue,
                    //submitBatch,
                    //decRegionIdList,
                    //decCategoryIdList,
                    //hashPhotoIds


                });





            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                return StatusCode(500, new { Errors = new List<string> { "An error occurred while submitting batch image." } });

            }

            
        }




        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult CloseInitialRound([FromBody] FinNumber model)
        //{
        //    var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        //    if (userIdClaim == null)
        //    {
        //        return Unauthorized();
        //    }

        //    try
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            return BadRequest(new { Errors = new List<string> { "Invalid parameter." } });
        //        }

        //        string userId = userIdClaim.Value;
        //        var processParam = _processParameterRepository.GetProcessParameterByDetailedDescAndIsActive("Open", 1);
        //        var refCodeParam = _processParameterRepository.GetReferenceCodeByIsActiveAndRefTypeId(1, "05");
        //        var round = refCodeParam.RefTypeID.ToString() + refCodeParam.RefCodeID.ToString();

        //        var IsCloseInitialRoundEnable = _processParameterRepository.IsCloseInitialRoundEnable(processParam.Index, 1);

        //        if (!IsCloseInitialRoundEnable)
        //        {
        //            return StatusCode(400, new { Errors = new List<string> { "Error Closing Round One" } });
        //        }

        //        if (processParam == null || refCodeParam == null)
        //        {
        //            return StatusCode(500, new { Errors = new List<string> { "Error in retrieving process parameters." } });

        //        }

        //        if (model == null || model.QualifyingNumber <= 0)
        //        {
        //            return StatusCode(400, new { Errors = new List<string> { "Error: Invalid Parameter." } });
        //        }

        //        using (var transaction = _tallyProgramContext.Database.BeginTransaction())
        //        {
        //            try
        //            {
        //                var regionList = _regionRepository.GetAllRegionView();
        //                var categoryList = _categoryRepository.GetAllCategoryView();

        //                List<MinAveragePerRegionCategory> top5Averages = new List<MinAveragePerRegionCategory>();
        //                List<MinAveragePerRegionCategory> imageBatch = new List<MinAveragePerRegionCategory>();

        //                int sort = 1; // Reset sort for each region and category
        //                foreach (var region in regionList)
        //                {
        //                    foreach (var category in categoryList)
        //                    {
        //                        //getting the minAverage Per RegAndCat
        //                        var averageScores = _batchImageRepository
        //                                            .GetMinimumAveragePerRegAndCat(
        //                                                region.RegionId,
        //                                                category.CategoryId,
        //                                                processParam.ParameterValue,
        //                                                int.Parse(processParam.Filler01),
        //                                                true,
        //                                                round

        //                                            );

        //                        if (averageScores.Count >= model.QualifyingNumber)
        //                        {
        //                            var MinimumAverage = averageScores[model.QualifyingNumber - 1]; // Get the 5th item
        //                            var minimumAverageNeeded = MinimumAverage.MinAverage;

        //                            top5Averages.Add(new MinAveragePerRegionCategory
        //                            {
        //                                RegionId = MinimumAverage.RegionId,
        //                                CategoryId = MinimumAverage.CategoryId,
        //                                MinAverage = minimumAverageNeeded
        //                            });
        //                        }

        //                        //getting the imageBatchesPerRegionCatwithAverage
        //                        var imageAveList = _batchImageRepository
        //                                            .GetImageBatchesAveragePerRegAndCat(
        //                                                region.RegionId,
        //                                                category.CategoryId,
        //                                                processParam.ParameterValue,
        //                                                int.Parse(processParam.Filler01),
        //                                                true,
        //                                                round

        //                                            );

        //                        foreach (var imageAve in imageAveList)
        //                        {
        //                            imageBatch.Add(new MinAveragePerRegionCategory
        //                            {
        //                                RegionId = imageAve.RegionId,
        //                                CategoryId = imageAve.CategoryId,
        //                                MinAverage = imageAve.MinAverage,
        //                                PhtoId = imageAve.PhtoId
        //                            });
        //                        }

        //                        var imagesInGroup = imageBatch
        //                        .Where(image => image.RegionId == region.RegionId && image.CategoryId == category.CategoryId)
        //                        //.OrderByDescending(image => image.MinAverage)
        //                        .ToList();

        //                        foreach (var image in imagesInGroup)
        //                        {
        //                            var minAvg = top5Averages.FirstOrDefault(x => x.RegionId == image.RegionId && x.CategoryId == image.CategoryId);
        //                            var imageToUpdate = _tallyProgramContext.ImageBatches.FirstOrDefault(i => i.ImageHashId == image.PhtoId);
        //                            if (imageToUpdate != null)
        //                            {
        //                                if (minAvg != default && ((image.MinAverage < minAvg.MinAverage) || (image.MinAverage == 0.00m))) //-- 05292024
        //                                {
        //                                    imageToUpdate.IsActive = false;
        //                                    imageToUpdate.Sort = 0;
        //                                }
        //                                else
        //                                {
        //                                    imageToUpdate.Sort = sort++;
        //                                }
        //                                _tallyProgramContext.Update(imageToUpdate);
        //                            }
        //                        }


        //                    }
        //                }

        //                _tallyProgramContext.SaveChanges();

        //                //update the paramValues and refCode
        //                //closing initial round

        //                if (_processParameterRepository.CloseDailyInitialRoundByIndex(
        //                                            processParam.Index,
        //                                            refCodeParam.Index,
        //                                            model.QualifyingNumber.ToString(),
        //                                            userId
        //                                    ))
        //                {
        //                    transaction.Commit();

        //                    return Ok(new
        //                    {
        //                        ResponseCode = "200",
        //                        Message = "Update Completed. Closing Initial Round. Opening Daily Final Round."
        //                    });
        //                }
        //                else
        //                {

        //                    transaction.Rollback();
        //                    return StatusCode(400, new { Errors = new List<string> { "Error: Update cannot be completed." } });

        //                }

        //            }
        //            catch (Exception ex)
        //            {
        //                _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
        //                transaction.Rollback();
        //                //return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        //                return StatusCode(400, new { Errors = new List<string> { "Error: Update cannot be completed." } });

        //            }
        //        }



        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
        //        return StatusCode(500, new { Errors = new List<string> { "An error occurred while submitting batch image." } });

        //    }

        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CloseRegionalRound([FromBody] FinNumber model)
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
                var refCodeParam = _processParameterRepository.GetReferenceCodeByIsActiveAndRefTypeId(1, "05");
                var round = refCodeParam.RefTypeID.ToString() + refCodeParam.RefCodeID.ToString();

                string roundInfoE = "";
                int roundInfoD = 0;

                try
                {
                    roundInfoE = _localEncryption.ANDecrypt(model.RoundInfo.ToString(), key);
                    if (int.TryParse(roundInfoE, out roundInfoD))
                    {
                        var temp_ = "05" + roundInfoD.ToString("00");
                        if (temp_ != round)
                        {
                            return BadRequest(new { Errors = new List<string> { "Cannot Close Current Round." } });
                        }
                    }
                    else
                    {
                        return BadRequest(new { Errors = new List<string> { "Invalid Round Information." } });
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                    return BadRequest(new { Errors = new List<string> { "Invalid Round Information." } });
                }


            
                var IsCloseInitialRoundEnable = _processParameterRepository.IsCloseInitialRoundEnable(processParam.Index, roundInfoD);

                if (!IsCloseInitialRoundEnable)
                {
                    return StatusCode(400, new { Errors = new List<string> { "Error Closing Round." } });
                }

                if (processParam == null || refCodeParam == null)
                {
                    return StatusCode(500, new { Errors = new List<string> { "Error in retrieving process parameters." } });

                }

                if (model == null || model.QualifyingNumber <= 0)
                {
                    return StatusCode(400, new { Errors = new List<string> { "Error: Invalid Parameter." } });
                }

                //check if round has score , or if the qualifying number is valid
                if (!_batchImageRepository.CanCloseRound(int.Parse(processParam.Filler01), processParam.ParameterValue, round, model.QualifyingNumber))
                {
                    return StatusCode(400, new { Errors = new List<string> { "Error: The number of images with score is less than the qualifying round." } });
                }




                using (var transaction = _tallyProgramContext.Database.BeginTransaction())
                {
                    try
                    {
                        var regionList = _regionRepository.GetAllRegionView();
                        var categoryList = _categoryRepository.GetAllCategoryView();

                        List<MinAveragePerRegionCategory> top5Averages = new List<MinAveragePerRegionCategory>();
                        List<MinAveragePerRegionCategory> imageBatch = new List<MinAveragePerRegionCategory>();

                         // Reset sort for each region and category
                        foreach (var region in regionList)
                        {
                            
                            foreach (var category in categoryList)
                            {
                                int sort = 1;
                                //getting the minAverage Per RegAndCat
                                var averageScores = _batchImageRepository
                                                    .GetMinimumAveragePerRegAndCat(
                                                        region.RegionId,
                                                        category.CategoryId,
                                                        processParam.ParameterValue,
                                                        int.Parse(processParam.Filler01),
                                                        true,
                                                        round

                                                    );

                                if (averageScores.Count >= model.QualifyingNumber)
                                {
                                    var MinimumAverage = averageScores[model.QualifyingNumber - 1]; // Get the 5th item
                                    var minimumAverageNeeded = MinimumAverage.MinAverage;

                                    top5Averages.Add(new MinAveragePerRegionCategory
                                    {
                                        RegionId = MinimumAverage.RegionId,
                                        CategoryId = MinimumAverage.CategoryId,
                                        MinAverage = minimumAverageNeeded
                                    });
                                }
                                else 
                                {
                                    top5Averages.Add(new MinAveragePerRegionCategory
                                    {
                                        RegionId = region.RegionId,
                                        CategoryId = category.CategoryId,
                                        MinAverage = 0.00m
                                    });
                                }

                               
                       

                                //getting the imageBatchesPerRegionCatwithAverage
                                var imageAveList = _batchImageRepository
                                                    .GetImageBatchesAveragePerRegAndCat(
                                                        region.RegionId,
                                                        category.CategoryId,
                                                        processParam.ParameterValue,
                                                        int.Parse(processParam.Filler01),
                                                        true,
                                                        round

                                                    );


                                var masterimgList = _batchImageRepository.GetImageBatches(region.RegionId,
                                                        category.CategoryId,
                                                        processParam.ParameterValue,
                                                        int.Parse(processParam.Filler01),
                                                        true);



                                // Loop through masterimgList to populate imageBatch
                                foreach (var masterImage in masterimgList)
                                {
                                    // Check if there is a corresponding item in imageAveList
                                    var correspondingAve = imageAveList.FirstOrDefault(a => a.PhtoId == masterImage.PhtoId);

                                    if (correspondingAve != null)
                                    {
                                        // Use MinAverage from imageAveList
                                        imageBatch.Add(new MinAveragePerRegionCategory
                                        {
                                            RegionId = correspondingAve.RegionId,
                                            CategoryId = correspondingAve.CategoryId,
                                            MinAverage = correspondingAve.MinAverage,
                                            PhtoId = correspondingAve.PhtoId
                                        });
                                    }
                                    else
                                    {
                                        // Use MinAverage from masterimgList
                                        imageBatch.Add(new MinAveragePerRegionCategory
                                        {
                                            RegionId = masterImage.RegionId,
                                            CategoryId = masterImage.CategoryId,
                                            MinAverage = masterImage.MinAverage,
                                            PhtoId = masterImage.PhtoId
                                        });
                                    }
                                }


                                //foreach (var imageAve in imageAveList)
                                //{
                                //    imageBatch.Add(new MinAveragePerRegionCategory
                                //    {
                                //        RegionId = imageAve.RegionId,
                                //        CategoryId = imageAve.CategoryId,
                                //        MinAverage = imageAve.MinAverage,
                                //        PhtoId = imageAve.PhtoId
                                //    });
                                //}

                                var imagesInGroup = imageBatch
                                .Where(image => image.RegionId == region.RegionId && image.CategoryId == category.CategoryId)
                                //.OrderByDescending(image => image.MinAverage)
                                .ToList();

                                if (roundInfoD == 3) //reorder using minaverage if round 3
                                {
                                    imagesInGroup = imagesInGroup.OrderByDescending(image => image.MinAverage).ToList();
                                }

                                int sort_ = 1; // Reset sort for each region and category
                                decimal preAve = 0.00m;
                                int lenGroup = imagesInGroup.Count;

                                foreach (var image in imagesInGroup)
                                {
                                    var minAvg = top5Averages.FirstOrDefault(x => x.RegionId == image.RegionId && x.CategoryId == image.CategoryId);
                                    
                                    var imageToUpdate = _tallyProgramContext.ImageBatches.FirstOrDefault(i => i.ImageHashId == image.PhtoId);
                                    if (imageToUpdate != null)
                                    {
                                        if (minAvg == null)
                                        {
                                            imageToUpdate.IsActive = false;
                                            imageToUpdate.Sort = 0;
                                        }
                                        else if ((image.MinAverage < minAvg.MinAverage) || (image.MinAverage == 0.00m))
                                        {
                                            imageToUpdate.IsActive = false;
                                            imageToUpdate.Sort = 0;
                                        }
                                        else
                                        {
                                            if (roundInfoD == 1 || roundInfoD == 2)
                                            {
                                                if (!CopyImageToRegionalRound(imageToUpdate.ImageHashId, roundInfoD))
                                                {
                                                    transaction.Rollback();
                                                    return StatusCode(400, new { Errors = new List<string> { "Error: Update cannot be completed." } });
                                                }
                                                
                                                imageToUpdate.Sort = sort++;
                                            }


                                            if (roundInfoD == 3)
                                            {
                                                if (!CopyImageToFinalFolder(imageToUpdate.ImageHashId, sort_))
                                                {
                                                    transaction.Rollback();
                                                    return StatusCode(400, new { Errors = new List<string> { "Error: Update cannot be completed." } });
                                                }


                                                //imageToUpdate.Sort = sort++;
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

                                                imageToUpdate.IsActive = false;
                                                imageToUpdate.Sort = 0;
                                            }

                                        }
                                        _tallyProgramContext.Update(imageToUpdate);
                                    }

                                    preAve = image.MinAverage;
                                }


                            }
                        }

                        _tallyProgramContext.SaveChanges();

                        //update the paramValues and refCode
                        //closing initial round

                        if (_processParameterRepository.CloseDailyInitialRoundByIndex(
                                                    processParam.Index,
                                                    refCodeParam.Index,
                                                    model.QualifyingNumber.ToString(),
                                                    roundInfoD,
                                                    userId
                                            ))
                        {
                            transaction.Commit();

                            return Ok(new
                            {
                                ResponseCode = "200",
                                Message = "Update Completed. Round Closed."
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




        private bool CopyImageToFinalFolder(string photoId, int sort) {

            var imageInfo = _photoLocationsRepository.CopyImageInformation(photoId);
            string RootURL = _appInfo.AppURLRepository + @"3_GrandFinal/";
            var imagePartialPath = _appInfo.AppRepository + "3_GrandFinal\\"; 
            var imgFullFilePath = imagePartialPath + imageInfo[0].CategoryName + @"\" + imageInfo[0].FileName; 
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
                            PhotoCode = "20",
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

        private bool CopyImageToRegionalRound(string photoId, int Round)
        {

            var imageInfo = _photoLocationsRepository.CopyImageInformation(photoId);

            var imagePartialPath = "";
            if (Round == 1)
            {
                imagePartialPath = _appInfo.AppRepository + "2_RegionalRound\\Top40\\";
            }

            if (Round == 2)
            {
                imagePartialPath = _appInfo.AppRepository + "2_RegionalRound\\Top10\\";
            }
            

            var imgFullFilePath = imagePartialPath + imageInfo[0].RegionName + @"\" + imageInfo[0].CategoryName + @"\" + imageInfo[0].FileName;
            


            if (imageInfo != null)
            {

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
