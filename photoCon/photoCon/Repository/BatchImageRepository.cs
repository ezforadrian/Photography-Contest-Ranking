using Microsoft.EntityFrameworkCore;
using photoCon.Data;
using photoCon.Dto;
using photoCon.Interface;
using photoCon.Models;


namespace photoCon.Repository
{
    public class BatchImageRepository : IBatchImageRepository
    {
        private readonly ILogger<BatchImageRepository> _logger;
        private readonly TallyProgramContext _tallyProgramContext;

        public BatchImageRepository(ILogger<BatchImageRepository> logger, TallyProgramContext tallyProgramContext)
        {
            _logger = logger;
            _tallyProgramContext = tallyProgramContext;
        }

        public bool DeleteBatchRecord(string date_, int DayNumber)
        {
            try
            {
                var recordsToDelete = _tallyProgramContext.ImageBatches
                                      .Where(d => d.Date == date_ && d.DayNumber == DayNumber);

                _tallyProgramContext.ImageBatches.RemoveRange(recordsToDelete);
                return Save(); // Save changes after deleting records
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                Console.WriteLine($"Error deleting records: {ex.HelpLink}");
                return false;
            }
        }

        public List<PhotoLocation> GetAllPhotoLocation()
        {
            return _tallyProgramContext.PhotoLocations.ToList();
        }

        public async Task<List<GetHashPhotoIdsAsync>> GetHashPhotoIdsAsync(List<int> regionIds, List<int> categoryIds)
        {


            var pl = await _tallyProgramContext.PhotoLocations
            .Where(pl => regionIds.Contains(pl.RegionId) && categoryIds.Contains(pl.CategoryId) && (pl.PhotoCode == "10") )
            .ToListAsync();

            var result_ = (from a in pl
                           join b in _tallyProgramContext.PhotoMetaData on a.HashPhotoId equals b.HashPhotoId into joinab
                           from ab in joinab.DefaultIfEmpty()
                           select new GetHashPhotoIdsAsync
                           {
                               HashPhotoId = a.HashPhotoId,
                               FileName = ab.FileName,
                               RegionId = a.RegionId,
                               CategoryId = a.CategoryId

                           }).OrderBy(x => x.RegionId).ThenBy(x => x.CategoryId).ThenBy(x => x.FileName).ToList();

            return result_.ToList();
        }

        //public async Task<List<string>> GetHashPhotoIdsGFAsync(List<int> categoryIds)
        //{
        //    return await _tallyProgramContext.PhotoLocations
        //    .Where(pl => (categoryIds.Contains(pl.CategoryId)) && (pl.PhotoCode == "20") )
        //    .Select(pl => pl.HashPhotoId)
        //    .ToListAsync();
        //}

        public async Task<List<GetHashPhotoIdsAsync>> GetHashPhotoIdsGFAsync(List<int> categoryIds, int inclusion)
        {
            List<PhotoLocationDto> photoLocations = new List<PhotoLocationDto>();



            photoLocations = await _tallyProgramContext.PhotoLocations
                                .Where(pl => categoryIds.Contains(pl.CategoryId) && pl.PhotoCode == "20")
                                .OrderBy(pl => pl.RegionId).ThenBy(pl => pl.CategoryId).ThenBy(pl => pl.Filler01)
                                .Select(pl => new PhotoLocationDto { 
                                    PhotoId = pl.PhotoId,
                                    HashPhotoId = pl.HashPhotoId,
                                    RepositoryLocation = pl.RepositoryLocation,
                                    RegionId = pl.RegionId,
                                    CategoryId = pl.CategoryId,
                                    PhotoCode = pl.PhotoCode,
                                    Filler01 = int.Parse(pl.Filler01),
                                    Filler02 = pl.Filler02,
                                    EncodedBy = pl.EncodedBy,
                                    DateTimeEncoded = pl.DateTimeEncoded,

                                })
                                .ToListAsync();

            var result_ = (from a in photoLocations
                           join b in _tallyProgramContext.PhotoMetaData on a.HashPhotoId equals b.HashPhotoId into joinab
                           from ab in joinab.DefaultIfEmpty()
                           select new GetHashPhotoIdsAsync
                           {
                               HashPhotoId = a.HashPhotoId,
                               FileName = ab.FileName,
                               RegionId = a.RegionId,
                               CategoryId = a.CategoryId

                           }).OrderBy(x => x.RegionId).ThenBy(x => x.CategoryId).ThenBy(x => x.FileName).ToList();





            return result_.ToList();
        }


        public List<ImageBatch> GetImageBatches()
        {
            return _tallyProgramContext.ImageBatches.ToList();
        }

        public int ImageBatchCategoryCount(int categoryId, int dayNumber)
        {
            var Count = (from a in _tallyProgramContext.ImageBatches
                         join b in _tallyProgramContext.PhotoLocations on a.ImageHashId equals b.HashPhotoId into joinAB
                         from joinAB_ in joinAB
                         where joinAB_.CategoryId == categoryId && a.DayNumber == dayNumber
                         select a).Count();

            return Count;
        }

        //count of photo in imagebatch by region
        public int ImageBatchCountPerRegion(int dayNumber, string date, int regionId)
        {
            var countPerRegion = (from a in _tallyProgramContext.ImageBatches
                                  join b in _tallyProgramContext.PhotoLocations on a.ImageHashId equals b.HashPhotoId into ab
                                  from ab_ in ab
                                  where ab_.RegionId == regionId
                                  && a.DayNumber == dayNumber
                                  && a.Date == date
                                  select a).Count();

            return countPerRegion;
        }


        public bool ImageBatchCountPerRegionNotToday(int regionId, int dayNumber, string date)
        {
            var count = (from a in _tallyProgramContext.ImageBatches
                         join b in _tallyProgramContext.PhotoLocations on a.ImageHashId equals b.HashPhotoId into ab
                         from ab_ in ab.DefaultIfEmpty()
                         where ab_.RegionId == regionId
                         && a.DayNumber != dayNumber
                         && a.Date != date
                         select a).Any();

            return count;
        }

        //count of photo in imagebatch by category
        public int ImageBatchCountPerCategory(int dayNumber, string date, int catId)
        {
            var countPerRegion = (from a in _tallyProgramContext.ImageBatches
                                  join b in _tallyProgramContext.PhotoLocations on a.ImageHashId equals b.HashPhotoId into ab
                                  from ab_ in ab
                                  where ab_.CategoryId == catId
                                  && a.DayNumber == dayNumber
                                  && a.Date == date
                                  select a).Count();

            return countPerRegion;
        }

        public bool ImageBatchCountPerCategoryGrandFinalNotToday(int catId, int dayNumber, string date)
        {
            var count = (from a in _tallyProgramContext.ImageBatches
                         join b in _tallyProgramContext.PhotoLocations on a.ImageHashId equals b.HashPhotoId into ab
                         from ab_ in ab.DefaultIfEmpty()
                         where ab_.CategoryId == catId
                         && a.DayNumber != dayNumber
                         && a.Date != date
                         && ab_.PhotoCode == "20"
                         select a).Any();

            return count;
        }





        public int ImageBatchRegionCategoryCount(int regionId, int categoryId)
        {
            var Count = (from a in _tallyProgramContext.ImageBatches
                         join b in _tallyProgramContext.PhotoLocations on a.ImageHashId equals b.HashPhotoId into joinAB
                         from joinAB_ in joinAB
                         where joinAB_.RegionId == regionId && joinAB_.CategoryId == categoryId 
                         select a).Count();

            return Count;
        }

        public int ImageBatchRegionCount(int regionId, int dayNumber)
        {
            var Count = (from a in _tallyProgramContext.ImageBatches
                         join b in _tallyProgramContext.PhotoLocations on a.ImageHashId equals b.HashPhotoId into joinAB
                         from joinAB_ in joinAB
                         where joinAB_.RegionId == regionId &&  a.DayNumber == dayNumber
                         select a).Count();

            return Count;
        }

        public bool IsRecordExist(string imageHashId, string date_, int DayNumber, bool IsActive)
        {
            var isExist = (from a in _tallyProgramContext.ImageBatches
                          where a.ImageHashId == imageHashId
                          && a.Date == date_
                          && a.DayNumber == DayNumber
                          && a.IsActive == IsActive
                          select a).Any();

            return isExist;
        }

        public bool IsThereARecordForThisDay(string date_, int DayNumber)
        {
            var isExist = (from a in _tallyProgramContext.ImageBatches
                           where a.Date == date_
                           && DayNumber == DayNumber
                           select a).Any();

            return isExist;
        }

        public bool Save()
        {
            return _tallyProgramContext.SaveChanges() > 0;
        }

        public bool SaveBatchImage(ImageBatch imageBatch)
        {
            _tallyProgramContext.ImageBatches.Add(imageBatch);
            return Save();
        }

        public async Task SaveBatchImagesAsync(List<ImageBatch> batchImages)
        {
            using var transaction = await _tallyProgramContext.Database.BeginTransactionAsync();

            try
            {
                foreach (var batchImage in batchImages)
                {
                    _tallyProgramContext.ImageBatches.Add(batchImage);
                }

                await _tallyProgramContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred: {Message}, Source: {Source}", ex.Message, ex.Source);
                await transaction.RollbackAsync();
                throw new Exception("Error occurred while saving batch images.");
            }
        }

        public int SortCountPerDateAndDay(string date_, int dayNumber)
        {
            var Count = (from a in _tallyProgramContext.ImageBatches
                         where a.Date == date_
                         && a.DayNumber == dayNumber
                         select a).Count();

            return Count;
        }

        public bool isUpdateButtonEnabled(int daynumber, string date)
        {
            var hasScore = (from a in _tallyProgramContext.ImageScore
                            join b in _tallyProgramContext.ImageBatches on a.PhotoId equals b.ImageHashId into ab
                            from joinab in ab.DefaultIfEmpty() // Ensures the left join
                            where joinab.DayNumber == daynumber && joinab.Date == date
                            select a).Any();

            return hasScore;
        }

        public bool isUpdateButtonEnabledGrandFinal(int daynumber, string date)
        {
            var hasScore = (from a in _tallyProgramContext.ImageScoreGrandFinal
                            join b in _tallyProgramContext.ImageBatches on a.PhotoId equals b.ImageHashId into ab
                            from joinab in ab.DefaultIfEmpty() // Ensures the left join
                            where joinab.DayNumber == daynumber && joinab.Date == date
                            select a).Any();

            return hasScore;
        }

        public List<JudgeImageView> GetImagesByParamValues(string parameterValue, string filler01, int ppIsActive, bool imIsActive, string photoCode)
        {
            var judgeView = (from a in _tallyProgramContext.ImageBatches
                             join b in _tallyProgramContext.ProcessParameters on a.DayNumber.ToString() equals b.Filler01 into ab
                             from joinab in ab.DefaultIfEmpty()
                             join c in _tallyProgramContext.PhotoLocations on a.ImageHashId equals c.HashPhotoId into ac
                             from joinac in ac.DefaultIfEmpty()
                             join d in _tallyProgramContext.PhotoMetaData on a.ImageHashId equals d.HashPhotoId into ad
                             from joinad in ad.DefaultIfEmpty()
                             join e in _tallyProgramContext.TblRegions on joinac.RegionId equals e.RegionId into ce
                             from joince in ce.DefaultIfEmpty()
                             join f in _tallyProgramContext.Categories on joinac.CategoryId equals f.CategoryId into cf
                             from joincf in cf.DefaultIfEmpty()
                             join g in _tallyProgramContext.CSV_PhotoDetails on joinad.FileName equals g.photoname into ag
                             from joinag in ag.DefaultIfEmpty()
                             where
                                joinab.ParameterValue == parameterValue
                                && joinab.Filler01 == filler01
                                && joinab.IsActive == ppIsActive
                                && a.IsActive == imIsActive
                                && joinac.PhotoCode == photoCode
                             select new JudgeImageView
                             {
                                 ImageBatch_Index = a.Index,
                                 ImageBatch_ImageHashId = a.ImageHashId,
                                 ImageBatch_Sort = a.Sort,
                                 ImageBatch_Date = a.Date,
                                 ImageBatch_DayNumber = a.DayNumber,
                                 ImageBatch_IsActive = a.IsActive,
                                 PhotoLocations_PhotoId = joinac.PhotoId,
                                 PhotoLocations_HashPhotoID = joinac.HashPhotoId,
                                 PhotoLocations_RepositoryLocation = joinac.RepositoryLocation,
                                 PhotoLocations_RegionId = joinac.RegionId,
                                 PhotoLocations_CategoryId = joinac.CategoryId,
                                 RegionName = joince.RegionName,
                                 CategoryName = joincf.CategoryName,
                                 PhotoMetaData_Index = joinad.Index,
                                 PhotoMetaData_HashPhotoID = joinad.HashPhotoId,
                                 PhotoMetaData_FileName = joinad.FileName,
                                 PhotoMetaData_Dimension = joinad.Dimension,
                                 PhotoMetaData_Width = joinad.Width,
                                 PhotoMetaData_Height = joinad.Height,
                                 PhotoMetaData_HorizontalResolution = joinad.HorizontalResolution,
                                 PhotoMetaData_VerticalResolution = joinad.VerticalResolution,
                                 PhotoMetaData_BitDepth = joinad.BitDepth,
                                 PhotoMetaData_ResolutionUnit = joinad.ResolutionUnit,
                                 PhotoMetaData_ImageURL = joinad.ImageUrl,
                                 PhotoMetaData_CameraMaker = joinad.CameraMaker,
                                 PhotoMetaData_CameraModel = joinad.CameraModel,
                                 PhotoMetaData_FStop = joinad.Fstop,
                                 PhotoMetaData_ExposureTime = joinad.ExposureTime,
                                 PhotoMetaData_ISOSpeed = joinad.Isospeed,
                                 PhotoMetaData_FocalLength = joinad.FocalLength,
                                 PhotoMetaData_MaxAperture = joinad.MaxAperture,
                                 PhotoMetaData_MeteringMode = joinad.MeteringMode,
                                 PhotoMetaData_FlashMode = joinad.FlashMode,
                                 PhotoMetaData_MmFocalLength = joinad.MmFocalLength,
                                 Location = joinag.location != null ? joinag.location : "Not Available",
                                 Description = joinag.photodesc != null ? joinag.photodesc : "Not Available",
                                 PhotoTitle = joinag.phototitle != null ? joinag.phototitle : "Not Available",
                                 PhotoTaken = joinag.datetaken != null ? joinag.datetaken : "Not Available",

                             }
                             ).ToList().OrderBy(x => x.ImageBatch_Sort);

            return judgeView.ToList();
        }




        public int CountActiveImageForTheDayPerCat(string parameterValue, string filler01, int ppIsActive, bool imIsActive, int catId)
        {
            var judgeViewCount = (from a in _tallyProgramContext.ImageBatches
                                  join b in _tallyProgramContext.ProcessParameters on a.DayNumber.ToString() equals b.Filler01 into ab
                                  from joinab in ab.DefaultIfEmpty()
                                  join c in _tallyProgramContext.PhotoLocations on a.ImageHashId equals c.HashPhotoId into ac
                                  from joinac in ac.DefaultIfEmpty()
                                  join d in _tallyProgramContext.PhotoMetaData on a.ImageHashId equals d.HashPhotoId into ad
                                  from joinad in ad.DefaultIfEmpty()
                                  where
                                     joinab.ParameterValue == parameterValue
                                     && joinab.Filler01 == filler01
                                     && joinab.IsActive == ppIsActive
                                     && a.IsActive == imIsActive
                                     && joinac.CategoryId == catId
                                  select new JudgeImageView
                                  {
                                      ImageBatch_Index = a.Index,
                                      ImageBatch_ImageHashId = a.ImageHashId,
                                      ImageBatch_Sort = a.Sort,
                                      ImageBatch_Date = a.Date,
                                      ImageBatch_DayNumber = a.DayNumber,
                                      ImageBatch_IsActive = a.IsActive,
                                      PhotoLocations_PhotoId = joinac.PhotoId,
                                      PhotoLocations_HashPhotoID = joinac.HashPhotoId,
                                      PhotoLocations_RepositoryLocation = joinac.RepositoryLocation,
                                      PhotoLocations_RegionId = joinac.RegionId,
                                      PhotoLocations_CategoryId = joinac.CategoryId,
                                      PhotoMetaData_Index = joinad.Index,
                                      PhotoMetaData_HashPhotoID = joinad.HashPhotoId,
                                      PhotoMetaData_FileName = joinad.FileName,
                                      PhotoMetaData_Dimension = joinad.Dimension,
                                      PhotoMetaData_Width = joinad.Width,
                                      PhotoMetaData_Height = joinad.Height,
                                      PhotoMetaData_HorizontalResolution = joinad.HorizontalResolution,
                                      PhotoMetaData_VerticalResolution = joinad.VerticalResolution,
                                      PhotoMetaData_BitDepth = joinad.BitDepth,
                                      PhotoMetaData_ResolutionUnit = joinad.ResolutionUnit,
                                      PhotoMetaData_ImageURL = joinad.ImageUrl,
                                      PhotoMetaData_CameraMaker = joinad.CameraMaker,
                                      PhotoMetaData_CameraModel = joinad.CameraModel,
                                      PhotoMetaData_FStop = joinad.Fstop,
                                      PhotoMetaData_ExposureTime = joinad.ExposureTime,
                                      PhotoMetaData_ISOSpeed = joinad.Isospeed,
                                      PhotoMetaData_FocalLength = joinad.FocalLength,
                                      PhotoMetaData_MaxAperture = joinad.MaxAperture,
                                      PhotoMetaData_MeteringMode = joinad.MeteringMode,
                                      PhotoMetaData_FlashMode = joinad.FlashMode,
                                      PhotoMetaData_MmFocalLength = joinad.MmFocalLength,

                                  }
                             ).Count();

            if (judgeViewCount == null || judgeViewCount <= 0)
            {
                return 0;
            }

            return judgeViewCount;
        }

        public int CountActiveImageForTheDay(string parameterValue, string filler01, int ppIsActive, bool imIsActive)
        {
            var judgeViewCount = (from a in _tallyProgramContext.ImageBatches
                                  join b in _tallyProgramContext.ProcessParameters on a.DayNumber.ToString() equals b.Filler01 into ab
                                  from joinab in ab.DefaultIfEmpty()
                                  join c in _tallyProgramContext.PhotoLocations on a.ImageHashId equals c.HashPhotoId into ac
                                  from joinac in ac.DefaultIfEmpty()
                                  join d in _tallyProgramContext.PhotoMetaData on a.ImageHashId equals d.HashPhotoId into ad
                                  from joinad in ad.DefaultIfEmpty()
                                  where
                                     joinab.ParameterValue == parameterValue
                                     && joinab.Filler01 == filler01
                                     && joinab.IsActive == ppIsActive
                                     && a.IsActive == imIsActive
                                  select new JudgeImageView
                                  {
                                      ImageBatch_Index = a.Index,
                                      ImageBatch_ImageHashId = a.ImageHashId,
                                      ImageBatch_Sort = a.Sort,
                                      ImageBatch_Date = a.Date,
                                      ImageBatch_DayNumber = a.DayNumber,
                                      ImageBatch_IsActive = a.IsActive,
                                      PhotoLocations_PhotoId = joinac.PhotoId,
                                      PhotoLocations_HashPhotoID = joinac.HashPhotoId,
                                      PhotoLocations_RepositoryLocation = joinac.RepositoryLocation,
                                      PhotoLocations_RegionId = joinac.RegionId,
                                      PhotoLocations_CategoryId = joinac.CategoryId,
                                      PhotoMetaData_Index = joinad.Index,
                                      PhotoMetaData_HashPhotoID = joinad.HashPhotoId,
                                      PhotoMetaData_FileName = joinad.FileName,
                                      PhotoMetaData_Dimension = joinad.Dimension,
                                      PhotoMetaData_Width = joinad.Width,
                                      PhotoMetaData_Height = joinad.Height,
                                      PhotoMetaData_HorizontalResolution = joinad.HorizontalResolution,
                                      PhotoMetaData_VerticalResolution = joinad.VerticalResolution,
                                      PhotoMetaData_BitDepth = joinad.BitDepth,
                                      PhotoMetaData_ResolutionUnit = joinad.ResolutionUnit,
                                      PhotoMetaData_ImageURL = joinad.ImageUrl,
                                      PhotoMetaData_CameraMaker = joinad.CameraMaker,
                                      PhotoMetaData_CameraModel = joinad.CameraModel,
                                      PhotoMetaData_FStop = joinad.Fstop,
                                      PhotoMetaData_ExposureTime = joinad.ExposureTime,
                                      PhotoMetaData_ISOSpeed = joinad.Isospeed,
                                      PhotoMetaData_FocalLength = joinad.FocalLength,
                                      PhotoMetaData_MaxAperture = joinad.MaxAperture,
                                      PhotoMetaData_MeteringMode = joinad.MeteringMode,
                                      PhotoMetaData_FlashMode = joinad.FlashMode,
                                      PhotoMetaData_MmFocalLength = joinad.MmFocalLength,

                                  }
                             ).Count();



            return judgeViewCount;
        }

        public List<MinAveragePerRegionCategory> GetMinimumAveragePerRegAndCat(int regionId, int categoryId, string dateParamValue, int dayNumber, bool isActive, string dailyRound)
        {
            var result = (from a in _tallyProgramContext.ImageBatches
                          join b in _tallyProgramContext.ImageScore on a.ImageHashId equals b.PhotoId into ab
                          from joinab in ab.DefaultIfEmpty()
                          join c in _tallyProgramContext.PhotoLocations on a.ImageHashId equals c.HashPhotoId into ac
                          from joinac in ac.DefaultIfEmpty()
                          where joinac.RegionId == regionId && joinac.CategoryId == categoryId
                                && a.Date == dateParamValue && a.DayNumber == dayNumber
                                && a.IsActive == isActive && (joinab.Round == dailyRound || joinab.Round == null) /////05292024 7:53pm
                          group new { a, joinab, joinac } by new { a.ImageHashId, joinac.RegionId, joinac.CategoryId } into g
                          select new MinAveragePerRegionCategory
                          {
                              RegionId = g.Key.RegionId,
                              CategoryId = g.Key.CategoryId,
                              MinAverage = g.Average(x => (decimal?)x.joinab.Score ?? 0.00m)
                          })
                            .OrderByDescending(x => x.MinAverage)
                            .ToList();

            result = result.ToList();

            return result;
        }

        public List<MinAverageCategoryGrandFinal> GetMinimumAverageCatGrandFinal(int categoryId, string dateParamValue, int dayNumber, bool isActive, string dailyRound)
        {
            var result = (from a in _tallyProgramContext.ImageBatches
                          join b in _tallyProgramContext.ImageScoreGrandFinal on a.ImageHashId equals b.PhotoId into ab
                          from joinab in ab.DefaultIfEmpty()
                          join c in _tallyProgramContext.PhotoLocations on a.ImageHashId equals c.HashPhotoId into ac
                          from joinac in ac.DefaultIfEmpty()
                          where joinac.CategoryId == categoryId
                                && a.Date == dateParamValue && a.DayNumber == dayNumber
                                && a.IsActive == isActive && (joinab.Round == dailyRound || joinab.Round == null) /////05292024 7:53pm
                          group new { a, joinab, joinac } by new { a.ImageHashId, joinac.CategoryId } into g
                          select new MinAverageCategoryGrandFinal
                          {
                              CategoryId = g.Key.CategoryId,
                              MinAverage = g.Average(x => (decimal?)x.joinab.Score ?? 0.00m)
                          })
                            .OrderByDescending(x => x.MinAverage)
                            .ToList();

            return result;
        }

        public List<MinAveragePerRegionCategory> GetImageBatchesAveragePerRegAndCat(int regionId, int categoryId, string dateParamValue, int dayNumber, bool isActive, string dailyRound)
        {
            var result = (from a in _tallyProgramContext.ImageBatches
                          join b in _tallyProgramContext.ImageScore on a.ImageHashId equals b.PhotoId into ab
                          from joinab in ab.DefaultIfEmpty()
                          join c in _tallyProgramContext.PhotoLocations on a.ImageHashId equals c.HashPhotoId into ac
                          from joinac in ac.DefaultIfEmpty()
                          join d in _tallyProgramContext.PhotoMetaData on joinac.HashPhotoId equals d.HashPhotoId into cd
                          from joincd in cd.DefaultIfEmpty()
                          where joinac.RegionId == regionId && joinac.CategoryId == categoryId
                                && a.Date == dateParamValue && a.DayNumber == dayNumber
                                && a.IsActive == isActive && (joinab.Round == dailyRound || joinab.Round == null) /////05292024 7:53pm
                          group new { a, joinab, joinac, joincd } by new { a.ImageHashId, joinac.RegionId, joinac.CategoryId, joincd.FileName } into g
                          select new MinAveragePerRegionCategory
                          {
                              RegionId = g.Key.RegionId,
                              CategoryId = g.Key.CategoryId,
                              MinAverage = g.Average(x => (decimal?)x.joinab.Score ?? 0.00m),
                              PhtoId = g.Key.ImageHashId,
                              FileName = g.Key.FileName,
                          })
                            .OrderBy(x => x.RegionId).ThenBy(x => x.CategoryId).ThenBy(x => x.FileName).ThenByDescending(x => x.MinAverage)
                            .ToList();

            return result;
        }

        public List<MinAverageCategoryGrandFinal> GetImageBatchesAveragePerCatGrandFinal(int categoryId, string dateParamValue, int dayNumber, bool isActive, string dailyRound)
        {
            var result = (from a in _tallyProgramContext.ImageBatches
                          join b in _tallyProgramContext.ImageScoreGrandFinal on a.ImageHashId equals b.PhotoId into ab
                          from joinab in ab.DefaultIfEmpty()
                          join c in _tallyProgramContext.PhotoLocations on a.ImageHashId equals c.HashPhotoId into ac
                          from joinac in ac.DefaultIfEmpty()
                          where joinac.CategoryId == categoryId
                                && a.Date == dateParamValue && a.DayNumber == dayNumber
                                && a.IsActive == isActive && (joinab.Round == dailyRound || joinab.Round == null) /////05292024 7:53pm
                          group new { a, joinab, joinac } by new { a.ImageHashId, joinac.CategoryId } into g
                          select new MinAverageCategoryGrandFinal
                          {
                              CategoryId = g.Key.CategoryId,
                              MinAverage = g.Average(x => (decimal?)x.joinab.Score ?? 0.00m),
                              PhtoId = g.Key.ImageHashId
                          })
                            .OrderBy(x => x.CategoryId).ThenByDescending(x => x.MinAverage)
                            .ToList();

            return result;
        }

        public List<MinAveragePerRegionCategory> GetImageBatches(int regionId, int categoryId, string dateParamValue, int dayNumber, bool isActive)
        {
            var result = (from a in _tallyProgramContext.ImageBatches
                          join c in _tallyProgramContext.PhotoLocations on a.ImageHashId equals c.HashPhotoId into ac
                          from joinac in ac.DefaultIfEmpty()
                          join d in _tallyProgramContext.PhotoMetaData on joinac.HashPhotoId equals d.HashPhotoId into cd
                          from joincd in cd.DefaultIfEmpty()
                          where joinac.RegionId == regionId && joinac.CategoryId == categoryId
                                && a.Date == dateParamValue && a.DayNumber == dayNumber
                                && a.IsActive == isActive
                          group new { a, joinac, joincd } by new { a.ImageHashId, joinac.RegionId, joinac.CategoryId , joincd.FileName } into g
                          select new MinAveragePerRegionCategory
                          {
                              RegionId = g.Key.RegionId,
                              CategoryId = g.Key.CategoryId,
                              MinAverage = 0.00m,
                              PhtoId = g.Key.ImageHashId,
                              FileName = g.Key.FileName
                          })
                            .OrderBy(x => x.RegionId).ThenBy(x => x.CategoryId).ThenBy(x => x.FileName).ThenByDescending(x => x.MinAverage)
                            .ToList();

            return result;
        }

        public bool CanCloseRound(int daynumber, string date_, string round, int qNumber)
        {
            var record = (from a in _tallyProgramContext.ImageBatches
                          join b in _tallyProgramContext.ImageScore on a.ImageHashId equals b.PhotoId into ab
                          from joinab in ab.DefaultIfEmpty()
                          where
                            a.DayNumber == daynumber
                            && a.Date == date_
                            && joinab.Round == round
                            && a.IsActive == true
                          group new { a, joinab } by new { a.ImageHashId, a.Date, a.DayNumber, joinab.Round } into k
                          select new
                          {
                              ImageHashId = k.Key.ImageHashId,
                              Date = k.Key.Date,
                              Round = k.Key.Round,
                              DayNumber = k.Key.DayNumber,
                              Average = k.Average(x => (decimal?)x.joinab.Score ?? 0.00m),
                          }).ToList();

            if (record.Count >= qNumber)
            {
                return true;
            }

            return false;
            
        }

        public List<ImageBatch> GetImageBatchByDateDayNumberCategory(string date, int daynumber, int categoryId, string round, string userId)
        {
            var recordListB = (from a in _tallyProgramContext.ImageBatches
                               join c in _tallyProgramContext.PhotoLocations on a.ImageHashId equals c.HashPhotoId into ac
                               from joinac in ac.DefaultIfEmpty()
                               where
                                    joinac.PhotoCode == "10"
                                 && joinac.CategoryId == categoryId
                                 && a.Date == date
                                 && a.DayNumber == daynumber
                                 && a.IsActive == true
                               select new
                               {
                                   ImageHashId = a.ImageHashId,
                                   Sort = a.Sort,
                                   Date = a.Date,
                                   DayNumber = a.DayNumber,
                                   IsActive = a.IsActive,
                                   Round = round,
                                   Score = 0.00m
                               }).ToList();


            var recordListS = (from a in _tallyProgramContext.ImageBatches
                               join b in _tallyProgramContext.ImageScore on a.ImageHashId equals b.PhotoId into ab
                               from joinab in ab.DefaultIfEmpty()
                               join c in _tallyProgramContext.PhotoLocations on a.ImageHashId equals c.HashPhotoId into ac
                               from joinac in ac.DefaultIfEmpty()
                               where
                                 joinac.PhotoCode == "10"
                                 && joinac.CategoryId == categoryId
                                 && a.Date == date
                                 && a.DayNumber == daynumber
                                 && joinab.Round == round
                                 && joinab.UserId == userId
                                 && a.IsActive == true
                               select new
                               {
                                   ImageHashId = a.ImageHashId,
                                   Sort = a.Sort,
                                   Date = a.Date,
                                   DayNumber = a.DayNumber,
                                   IsActive = a.IsActive,
                                   Round = joinab.Round,
                                   Score = joinab.Score,

                               }).ToList();

            List<ImageBatchCatScoreList> batchList = new List<ImageBatchCatScoreList>();

            foreach (var main in recordListB)
            {
                var correspondingAve = recordListS.FirstOrDefault(a => a.ImageHashId == main.ImageHashId);

                if (correspondingAve != null)
                {
                    // Use MinAverage from imageAveList
                    batchList.Add(new ImageBatchCatScoreList
                    {
                        ImageHashId = correspondingAve.ImageHashId,
                        Sort = correspondingAve.Sort,
                        Date = correspondingAve.Date,
                        DayNumber = correspondingAve.DayNumber,
                        IsActive = correspondingAve.IsActive,
                        UserId = userId,
                        Round = correspondingAve.Round,
                        Score = correspondingAve.Score,
                    });
                }
                else
                {
                    // Use MinAverage from masterimgList
                    batchList.Add(new ImageBatchCatScoreList
                    {
                        ImageHashId = main.ImageHashId,
                        Sort = main.Sort,
                        Date = main.Date,
                        DayNumber = main.DayNumber,
                        IsActive = main.IsActive,
                        UserId = userId,
                        Round = main.Round,
                        Score = main.Score,
                    });
                }

            }

            var recordList = (from a in batchList
                              where
                                a.Score == 0.00m
                              select new ImageBatch
                              {
                                  ImageHashId = a.ImageHashId,
                                  Sort = a.Sort,
                                  Date = a.Date,
                                  DayNumber = a.DayNumber,
                                  IsActive = a.IsActive,

                              }).ToList();

            return recordList;
        }
    }
}
