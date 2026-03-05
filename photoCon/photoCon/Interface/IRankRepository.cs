using photoCon.Dto;
using photoCon.Models;

namespace photoCon.Interface
{
    public interface IRankRepository
    {
        List<OverallPhotoRankView> GetOverallRankingView(string date, int dayNumber, string round, int regionId, int categoryId);
        List<OverallPhotoRankView> GetGrandFinalRanking(string date, int dayNumber, string round, int categoryId);
        List<MyRankScore> GetMyRankScores(string date, int dayNumber, bool imIsActive, string round, string userId, int regionId, int categoryId);
        MyRankScore GetImageInfoByHashId(string hashPhotoId, string userId, string round);
        ////List<OverallPhotoRankView> GetOverallRankingViewByRegion(int regionId);
        ////List<OverallPhotoRankView> GetOverallRankingViewByCategory(int categoryId);
        ////List<OverallPhotoRankView> GetOverallRankingViewByRegionAndCategory(int regionId, int categoryId);
        //List<DailyPhotoRankView> GetDailyRankingViewByRegionAndCategory(int regionId, int categoryId);
        //List<DailyPhotoRankView> GetAllDailyRankingViewByRegionAndCategory();
        //List<DailyPhotoRankViewMyScore> MyScore(string userId, int regionId, int categoryId);
    }
}
