namespace photoCon.Dto
{
    public class OverallPhotoRankView
    {
        public string ImageUrl { get; set; }
        public string HashPhotoId { get; set; }
        public decimal AverageScore { get; set; }
        public int RegionId { get; set; }
        public string RegionName { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string? Location { get; set; }
        public string? Description { get; set; }
        public string? PhotoTitle { get; set; }
        public string? PhotoTaken { get; set; }
        public bool Unranked { get; set; }
        public long Rank { get; set; }
        

    }
}
