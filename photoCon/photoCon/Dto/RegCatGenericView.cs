namespace photoCon.Dto
{
    public class RegCatGenericView
    {
        public string HashId { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public int BatchImageCount { get; set; }
        public int PhotoLocationCount { get; set; }
        public bool? Readonly { get; set; }

    }

    
}
