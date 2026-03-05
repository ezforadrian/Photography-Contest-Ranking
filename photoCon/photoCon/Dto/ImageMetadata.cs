namespace photoCon.Dto
{
    public class ImageMetadata
    {
        public int ImageNumber { get; set; }
        public string ImageNumberHash { get; set; }
        public int ImageRegionId { get; set; }
        public byte[]? ImageBytes { get; set; }
        public string ImageURL { get; set; }
        public string ImageDirectory { get; set; }
        public string FileName { get; set; }
        public string Dimension { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        public string HorizontalResolution { get; set; }
        public string VerticalResolution { get; set; }
        public string BitDepth { get; set; }
        public string ResolutionUnit { get; set; }
        //public string ColorRepresentation { get; set; }
        public string CameraMaker { get; set; }
        public string CameraModel { get; set; }
        public string FStop { get; set; }
        public string ExposureTime { get; set; }
        public string ISOSpeed { get; set; }
        //public string ExposureBias { get; set; }
        public string FocalLength { get; set; }
        public string MaxAperture { get; set; }
        public string MeteringMode { get; set; }
        //public string SubjectDistance { get; set; }
        public string FlashMode { get; set; }
        //public string FlashEnergy { get; set; }
        public string MmFocalLength { get; set; }
        // Add more metadata properties as needed


    }
}
