namespace photoCon.Dto
{
    public class MinAveragePerRegionCategory
    {
        public int RegionId { get; set; }
        public int CategoryId { get; set; }
        public decimal MinAverage { get; set; }
        public string? PhtoId { get; set; }
        public string? FileName { get; set; }
    }


    public class MinAverageCategoryGrandFinal
    {
        public int CategoryId { get; set; }
        public decimal MinAverage { get; set; }
        public string? PhtoId { get; set; }
    }
}
