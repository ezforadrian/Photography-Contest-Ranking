namespace photoCon.Dto
{
    public class ServerResponse
    {
        public int ServerResponseCode { get; set; }
        public string ServerResponseMessage { get; set; }
        public int FolderCount { get; set; }
        public int DBCountTo { get; set; }
        public List<ImageMetadata> imageMetadata { get; set; } // Change to List<ImageMetadata>
    }
}
