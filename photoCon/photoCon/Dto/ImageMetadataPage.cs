namespace photoCon.Dto
{
    public class ImageMetadataPage
    {
        public List<ImageMetadata> ImageMetadatas { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
    }

}
