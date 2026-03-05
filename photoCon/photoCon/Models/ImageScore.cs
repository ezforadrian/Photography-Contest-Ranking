using System.ComponentModel.DataAnnotations;

namespace photoCon.Models
{
    public class ImageScore
    {
        public int Index { get; set; }
        [Required]
        public string PhotoId { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public string Round { get; set; }
        [Required]
        public decimal Score { get; set; }
        public DateTime LastUpdateDate { get; set; }
    }
}
