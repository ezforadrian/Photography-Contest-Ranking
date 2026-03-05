using photoCon.Dto;
using photoCon.Models;
using System.Security.Cryptography;

namespace photoCon.Interface
{
    public interface IBatchImageRepository
    {
        Task<List<GetHashPhotoIdsAsync>> GetHashPhotoIdsAsync(List<int> regionIds, List<int> categoryIds);
        Task<List<GetHashPhotoIdsAsync>> GetHashPhotoIdsGFAsync(List<int> categoryIds, int inclusion);
        Task SaveBatchImagesAsync(List<ImageBatch> batchImages);
        List<PhotoLocation> GetAllPhotoLocation();
        List<ImageBatch> GetImageBatches();
        List<MinAveragePerRegionCategory> GetImageBatches(int regionId, int categoryId, string dateParamValue, int dayNumber, bool isActive);
        List<JudgeImageView> GetImagesByParamValues(string parameterValue, string filler01, int ppIsActive, bool imIsActive, string photoCode);
        List<MinAveragePerRegionCategory> GetMinimumAveragePerRegAndCat(int regionId, int categoryId, string dateParamValue, int dayNumber, bool isActive, string dailyRound);
        List<MinAveragePerRegionCategory> GetImageBatchesAveragePerRegAndCat(int regionId, int categoryId, string dateParamValue, int dayNumber, bool isActive, string dailyRound);
        List<MinAverageCategoryGrandFinal> GetMinimumAverageCatGrandFinal(int categoryId, string dateParamValue, int dayNumber, bool isActive, string dailyRound);
        List<MinAverageCategoryGrandFinal> GetImageBatchesAveragePerCatGrandFinal(int categoryId, string dateParamValue, int dayNumber, bool isActive, string dailyRound);
        List<ImageBatch> GetImageBatchByDateDayNumberCategory(string date, int daynumber, int categoryId, string round, string userId);
        int ImageBatchCountPerRegion(int dayNumber, string date, int regionId);
        bool ImageBatchCountPerRegionNotToday(int regionId, int dayNumber, string date);
        int ImageBatchCountPerCategory(int dayNumber, string date, int catId);
        bool ImageBatchCountPerCategoryGrandFinalNotToday(int catId, int dayNumber, string date);
        int ImageBatchRegionCount(int regionId, int dayNumber);
        int ImageBatchCategoryCount(int categoryId , int dayNumber);
        int ImageBatchRegionCategoryCount(int regionId, int categoryId);
        int SortCountPerDateAndDay(string date_, int dayNumber);
        int CountActiveImageForTheDay(string parameterValue, string filler01, int ppIsActive, bool imIsActive);
        int CountActiveImageForTheDayPerCat(string parameterValue, string filler01, int ppIsActive, bool imIsActive, int catId);
        bool isUpdateButtonEnabled(int daynumber, string date);
        bool isUpdateButtonEnabledGrandFinal(int daynumber, string date);
        bool IsRecordExist(string imageHashId, string date_, int DayNumber, bool IsActive);
        bool SaveBatchImage(ImageBatch imageBatch);
        bool DeleteBatchRecord(string date_, int DayNumber);
        bool IsThereARecordForThisDay(string date_, int DayNumber);
        bool CanCloseRound(int daynumber, string date_, string round, int qNumber);
        bool Save();


    }
}
