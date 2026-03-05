using photoCon.Data;
using photoCon.Interface;
using photoCon.Models;

namespace photoCon.Repository
{
    public class SeedDatabaseRepository : ISeedDatabaseRepository
    {
        private readonly TallyProgramContext _tallyProgramContext;

        public SeedDatabaseRepository(TallyProgramContext tallyProgramContext)
        {
            _tallyProgramContext = tallyProgramContext;
        }

        public bool CreatePhotoLocation(PhotoLocation tblPhotoLocation)
        {
            _tallyProgramContext.PhotoLocations.Add(tblPhotoLocation);
            return Save();
        }

        public bool CreatePhotoMetadata(PhotoMetaDatum photoMetaDatum)
        {
            _tallyProgramContext.PhotoMetaData.Add(photoMetaDatum);
            return Save();
        }

        public bool IsPhotoLocationExist(string hashPhotoID, string photoLocation)
        {
            return _tallyProgramContext.PhotoLocations.Any(
                    l => l.HashPhotoId == hashPhotoID
                    && l.RepositoryLocation == photoLocation
                );
        }

        public bool IsPhotoMetaDataExist(
                                            string hashPhotoID, 
                                            string filename, 
                                            string imageURL
                                        )
        {
            return _tallyProgramContext.PhotoMetaData.Any(
                    l => l.HashPhotoId == hashPhotoID
                    && l.FileName == filename
                    && l.ImageUrl == imageURL
                );
        }

        public bool Save()
        {
            return _tallyProgramContext.SaveChanges() > 0;
        }
    }
}
