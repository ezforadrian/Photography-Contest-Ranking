using photoCon.Models;

namespace photoCon.Interface
{
    public interface ISeedDatabaseRepository
    {
        bool IsPhotoLocationExist(string hashPhotoID, string photoLocation);
        bool IsPhotoMetaDataExist(string hashPhotoID, string filename, string imageURL);
        bool CreatePhotoLocation(PhotoLocation PhotoLocation);
        bool CreatePhotoMetadata(PhotoMetaDatum photoMetaDatum);
        bool Save();
    }

}
