namespace photoCon.Dto
{
    public class MyRankScore
    {
        public string ImageUrl { get; set; }
        public int ImageSort { get; set; }
        public string HashPhotoId { get; set; }
        public decimal? MyScore { get; set; }
        public int RegionId { get; set; }
        public string RegionName { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string? Location { get; set; }
        public string? Description { get; set; }
        public string? PhotoTitle { get; set; }
        public string? PhotoTaken { get; set; }
        public string Round { get; set; }
        public bool IsUnranked { get; set; }
    }
}
