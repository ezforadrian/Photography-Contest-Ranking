namespace photoCon.Models
{
    public class vm_ReportParameters
    {
        public string? ReportCode { get; set; } = string.Empty;
        public string? Region { get; set; } = string.Empty;
        public string? Category { get; set; } = string.Empty;
        public string? Round { get; set; } = string.Empty;
        public string? EventName { get; set; } = string.Empty;
        public string? EventVenue { get; set; } = string.Empty;
    }

    public class vm_ReportParameters_Selection
    {
        public string? RankScreening {  get; set; } = string.Empty;
        public int? RankCount { get; set; } = 1;
        public string? ReportCode { get; set; } = string.Empty;
        public string? Region { get; set; } = string.Empty;
        public string? Category { get; set; } = string.Empty;
        public string? Round { get; set; } = string.Empty;
        public string? EventName { get; set; } = string.Empty;
        public string? EventVenue { get; set; } = string.Empty;
    }

    public class vm_DisplayReportParameters
    {
        public List<ReferenceCode> ReferenceCodes { get; set; } = new List<ReferenceCode>();
        public List<TblRegion> Regions { get; set; } = new List<TblRegion>();
        public List<Category> Categories { get; set; } = new List<Category>();

        public List<ReferenceCode> GetReportNameList {
            get { return ReferenceCodes.Where(a => a.RefTypeID == "08").ToList(); }
        }

        public List<Category> GetCategoriesList
        {
            get { return Categories.ToList(); }
        }

        public List<TblRegion> GetRegionsList
        {
            get { return Regions.ToList(); }
        }

        public List<ReferenceCode> GetRoundsList
        {
            get { return ReferenceCodes.Where(a => a.RefTypeID == "05").ToList(); }
        }
    }
}
