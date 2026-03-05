using photoCon.Data;
using photoCon.Interface;

namespace photoCon.Repository
{
    public class ImageScoreRepository : IImageScoreRepository
    {
        private readonly TallyProgramContext _tallyProgramContext;

        public ImageScoreRepository(TallyProgramContext tallyProgramContext)
        {
            _tallyProgramContext = tallyProgramContext;
        }

        public bool isRecordExist(string userId, string photoId)
        {
            return _tallyProgramContext.ImageScore.Where(a => a.UserId == userId && a.PhotoId == photoId).Any();
        }

        
    }
}
