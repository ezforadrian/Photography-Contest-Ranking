using photoCon.Data;
using photoCon.Dto;
using photoCon.Interface;

namespace photoCon.Repository
{
    public class PhotoLocationsRepository : IPhotoLocationsRepository
    {
        private readonly TallyProgramContext _tallyProgramContext;

        public PhotoLocationsRepository(TallyProgramContext tallyProgramContext)
        {
            _tallyProgramContext = tallyProgramContext;
        }

        public List<TransferImage> CopyImageInformation(string photoId)
        {
            List<TransferImage> result = new List<TransferImage>();
            var res = (from a in _tallyProgramContext.PhotoLocations
                       join b in _tallyProgramContext.PhotoMetaData on a.HashPhotoId equals b.HashPhotoId into ab
                       from joinab in ab.DefaultIfEmpty()
                       join c in _tallyProgramContext.TblRegions on a.RegionId equals c.RegionId into ac
                       from joinac in ac.DefaultIfEmpty()
                       join d in _tallyProgramContext.Categories on a.CategoryId equals d.CategoryId into ad
                       from joinad in ad.DefaultIfEmpty()
                       where a.HashPhotoId == photoId
                       select new TransferImage
                       {
                           HashPhotoId = a.HashPhotoId,
                           RepositoryLocation = a.RepositoryLocation,
                           RegionId = a.RegionId,
                           RegionName = joinac.RegionName,
                           CategoryId = a.CategoryId,
                           CategoryName = joinad.CategoryName,
                           PhotoCode = a.PhotoCode,
                           FileName = joinab.FileName,
                           Dimension = joinab.Dimension,
                           Width = joinab.Width,
                           Height = joinab.Height,
                           HorizontalResolution = joinab.HorizontalResolution,
                           VerticalResolution = joinab.VerticalResolution,
                           BitDepth = joinab.BitDepth,
                           ResolutionUnit = joinab.ResolutionUnit,
                           ImageUrl = joinab.ImageUrl,
                           CameraMaker = joinab.CameraMaker,
                           CameraModel = joinab.CameraModel,
                           FStop = joinab.Fstop,
                           ExposureTime = joinab.ExposureTime,
                           ISOSpeed = joinab.Isospeed,
                           FocalLength = joinab.FocalLength,
                           MaxAperture = joinab.MaxAperture,
                           MeteringMode = joinab.MeteringMode,
                           FlashMode = joinab.FlashMode,
                           MmFocalLength = joinab.MmFocalLength
                       }).SingleOrDefault();

            if (res != null)
            {
                result.Add(res); // Add the instance to the result list
            }

            return result;

        }

        public int ImageCategoryCount(int categoryId)
        {
            var Count = (from a in _tallyProgramContext.PhotoLocations
                         where a.CategoryId == categoryId && a.PhotoCode == "10" // regional round
                         select a).Count();

            return Count;
        }

        public int ImageCategoryCountGrandFinals(int categoryId)
        {
            var Count = (from a in _tallyProgramContext.PhotoLocations
                         where a.CategoryId == categoryId && a.PhotoCode == "20" // grandfinal round
                         select a).Count();

            return Count;
        }

        public int ImageRegionCategoryCount(int regionId, int categoryId)
        {
            var Count = (from a in _tallyProgramContext.PhotoLocations
                         where a.RegionId == regionId && a.CategoryId == categoryId && a.PhotoCode == "10" // regional round
                         select a).Count();

            return Count;
        }

        public int ImageRegionCount(int regionId)
        {
            var Count = (from a in _tallyProgramContext.PhotoLocations
                         where a.RegionId == regionId && a.PhotoCode == "10" // reginal round
                         select a).Count();

            return Count;
        }

        public bool UpdatePhotoCodeByImageHashId(string hashphotoId)
        {
            var record = (from a in _tallyProgramContext.PhotoLocations
                          where a.HashPhotoId == hashphotoId 
                          select a).SingleOrDefault();


            if (record == null)
            {
                return false;
            }

            record.Filler02 = "True";

            return Save();
        }

        public bool Save()
        {
            var saved = _tallyProgramContext.SaveChanges();

            return saved > 0 ? true : false;
        }

        public bool IsImageExist(string hashphotoId)
        {
            return _tallyProgramContext.PhotoLocations.Where(x => x.HashPhotoId == hashphotoId).Any();
        }
    }
}
