using photoCon.Dto;
using photoCon.Models;

namespace photoCon.Interface
{
    public interface IJudgeRepository
    {
        //Task<ICollection<ViewPhotoInfo>> GetViewPhotoInfosAsync();
        //Task<ICollection<ViewPhotoInfo>> GetViewPhotoInfosByHashPhotoIdAsync(string hashPhotoId);
        //List<ImitateUser> GetJudges();
        IQueryable<ViewPhotoInfo> GetViewPhotoInfosAsync();
        IQueryable<JudgeImageView> GetJudgeImageViewAsync();
        Task<ICollection<Judge>> GetAllJudge();
        //IQueryable<JudgeView> GetAllJudgeView();
        Task<Judge> GetJudgeById(int index);
        Decimal GetImageScore(string photoId, string userId);
        List<ImageScore> GetAllJudgeScore(string userId, string refTypeId, string refCodeId);
        List<ImageScore> GetImageScoreByJudgeAndRound(string userId, string refTypeId, string refCodeId, string photoId);
        List<ImageScoreGrandFinal> GetImageScoreGrandFinalByJudgeAndRound(string userId, string refTypeId, string refCodeId, string photoId);
        bool IsImageScoreByUser(string userid, string photoid, string refTypeId, string refCodeId);
        bool IsImageScoreGrandFinalByUser(string userid, string photoid, string refTypeId, string refCodeId);
        bool SaveScoreByJudgeInImage(ImageScore imageScore);
        bool SaveScoreByJudgeInImagePerCriteria(ImageScoreGrandFinal imageScore);
        bool UpdateScoreByJudgeInImage(string userid, string photoid, decimal score, string refTypeId, string refCodeId, DateTime lastupdatedate);
        bool UpdateScoreByJudgeInImagePerCriteria(ImageScoreGrandFinal imageScore);
        bool Save();
    }
}
