using photoCon.Dto;

namespace photoCon.Interface
{
    public interface IPhotoLocationsRepository
    {
        int ImageRegionCount(int regionId);
        int ImageCategoryCount(int categoryId);
        int ImageCategoryCountGrandFinals(int categoryId);
        int ImageRegionCategoryCount(int regionId, int categoryId);
        List<TransferImage> CopyImageInformation(string photoId);
        bool UpdatePhotoCodeByImageHashId(string hashphotoId);
        bool IsImageExist(string hashphotoId);
        bool Save();

    }
}
