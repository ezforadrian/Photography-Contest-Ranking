using System.ComponentModel.DataAnnotations;

namespace photoCon.Dto
{
    public class FinNumber
    {
        [Required]
        public int QualifyingNumber { get; set; }
        [Required]
        public string RoundInfo { get; set; }
    }
}
