namespace photoCon.Dto
{
    public class JudgeImageView
    {

        // ImageBatch table columns
        public int ImageBatch_Index { get; set; }
        public string ImageBatch_ImageHashId { get; set; }
        public int ImageBatch_Sort { get; set; }
        public string ImageBatch_Date { get; set; }
        public int ImageBatch_DayNumber { get; set; }
        public bool ImageBatch_IsActive { get; set; }

        // PhotoLocations table columns
        public int PhotoLocations_PhotoId { get; set; }
        public string PhotoLocations_HashPhotoID { get; set; }
        public string PhotoLocations_RepositoryLocation { get; set; }
        public int PhotoLocations_RegionId { get; set; }
        public int PhotoLocations_CategoryId { get; set; }
        public string? RegionName { get; set; }
        public string? CategoryName { get; set; }


        // PhotoMetaData table columns
        public int PhotoMetaData_Index { get; set; }
        public string PhotoMetaData_HashPhotoID { get; set; }
        public string PhotoMetaData_FileName { get; set; }
        public string PhotoMetaData_Dimension { get; set; }
        public string PhotoMetaData_Width { get; set; }
        public string PhotoMetaData_Height { get; set; }
        public string PhotoMetaData_HorizontalResolution { get; set; }
        public string PhotoMetaData_VerticalResolution { get; set; }
        public string PhotoMetaData_BitDepth { get; set; }
        public string PhotoMetaData_ResolutionUnit { get; set; }
        public string PhotoMetaData_ImageURL { get; set; }
        public string PhotoMetaData_CameraMaker { get; set; }
        public string PhotoMetaData_CameraModel { get; set; }
        public string PhotoMetaData_FStop { get; set; }
        public string PhotoMetaData_ExposureTime { get; set; }
        public string PhotoMetaData_ISOSpeed { get; set; }
        public string PhotoMetaData_FocalLength { get; set; }
        public string PhotoMetaData_MaxAperture { get; set; }
        public string PhotoMetaData_MeteringMode { get; set; }
        public string PhotoMetaData_FlashMode { get; set; }
        public string PhotoMetaData_MmFocalLength { get; set; }

        public string Location { get; set; }
        public string Description { get; set; }
        public string PhotoTitle { get; set;  }
        public string PhotoTaken { get; set;}
    }
}
