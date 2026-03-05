namespace photoCon.Dto
{
    public class JudgeDDay
    {
        public string judgeId { get; set; }
        public string photoId { get; set; }
        public string? Round { get; set; }
    }

    public class DriverRankTable
    {
        public string photoId { get; set; }
        public int regionId { get; set; }
        public int categoryId { get; set; }
        public string? round { get; set; }
        public decimal AveScore { get; set; }

    }
}
