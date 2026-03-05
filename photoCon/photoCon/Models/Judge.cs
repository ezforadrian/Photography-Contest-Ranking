namespace photoCon.Models
{
    public class Judge
    {
        public int Index { get; set; }
        public string HashIndex { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? EmailAddress { get; set; }
        public string? MobileNumber { get; set; }
        public string? Description { get; set; }
        public int? Filler01 { get; set; }
        public int? Filler02 { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDateTime { get; set; }
    }
}
