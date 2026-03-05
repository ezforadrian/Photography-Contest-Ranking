namespace photoCon.Dto
{
    public class SeedInfo
    {
        //-------
        public int RegionId { get; set; }
        public string RegionIdHash { get; set; }
        public string RegionName { get; set; }
        public int CategoryId { get; set; }
        public string CategoryIdHash { get; set; }
        public string CategoryName { get; set; }
        public int DBCountPerRegionandCategory { get; set; }
        public int FolderCountPerRegionandCategory { get; set; }
        public DateTime LastSeededOn { get; set; }
        public int LastDBCount { get; set; }
        public int LastSeededCount { get; set; }
    }
}
