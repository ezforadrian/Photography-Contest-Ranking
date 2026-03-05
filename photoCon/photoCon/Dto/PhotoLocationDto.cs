namespace photoCon.Dto
{
    public class PhotoLocationDto
    {

        public int PhotoId { get; set; }
        public string HashPhotoId { get; set; } = null!;
        public string RepositoryLocation { get; set; } = null!;
        public int RegionId { get; set; }
        public int CategoryId { get; set; }
        public string? PhotoCode { get; set; }
        public int? Filler01 { get; set; }
        public string? Filler02 { get; set; }
        public string? EncodedBy { get; set; }
        public DateTime? DateTimeEncoded { get; set; }
    }
}
