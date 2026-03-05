

using System.ComponentModel.DataAnnotations;

namespace photoCon.Dto
{
    public class ImageScoreView
    {
        [Required]
        public int RowNumber { get; set; }
        [Required]
        public string VPhotoIdx { get; set; }
        [Required]
        public string PhotoId { get; set; }
        [Required]
        public decimal Score { get; set; }
        public string? Round { get; set; }


    }
}
