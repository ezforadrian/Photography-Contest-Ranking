namespace photoCon.Dto
{
    public class TransferImage
    {
        public string HashPhotoId { get; set; }
        public string RepositoryLocation { get; set; }
        public int RegionId { get; set; }
        public string RegionName { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string PhotoCode { get; set; }
        public string FileName { get; set; }
        public string Dimension { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        public string HorizontalResolution { get; set; }
        public string VerticalResolution { get; set; }
        public string BitDepth { get; set; }
        public string ResolutionUnit { get; set; }
        public string ImageUrl { get; set; }
        public string CameraMaker { get; set; }
        public string CameraModel { get; set; }
        public string FStop { get; set; }
        public string ExposureTime { get; set; }
        public string ISOSpeed { get; set; }
        public string FocalLength { get; set; }
        public string MaxAperture { get; set; }
        public string MeteringMode { get; set; }
        public string FlashMode { get; set; }
        public string MmFocalLength { get; set; }

    }
}
