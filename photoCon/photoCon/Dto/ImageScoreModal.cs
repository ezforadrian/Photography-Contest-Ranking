using System.ComponentModel.DataAnnotations;

namespace photoCon.Dto
{
    public class ImageScoreModal
    {
        [Required]
        public string PhotoId { get; set; }
        [Required]
        public decimal Score { get; set; }
        public string? Round { get; set; }
    }
}
