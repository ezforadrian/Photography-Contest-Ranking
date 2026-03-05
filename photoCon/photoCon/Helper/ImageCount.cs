using photoCon.Interface;

namespace photoCon.Helper
{
    public class ImageCount 
    {
        private readonly IPhotoLocationsRepository photoLocationsRepository;

        public ImageCount(IPhotoLocationsRepository photoLocationsRepository)
        {
            this.photoLocationsRepository = photoLocationsRepository;
        }
    }
}
