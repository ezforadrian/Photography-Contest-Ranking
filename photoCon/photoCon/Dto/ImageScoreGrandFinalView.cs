using System.ComponentModel.DataAnnotations;

namespace photoCon.Dto
{
    public class ImageScoreGrandFinalView
    {

        [Required(ErrorMessage = "Row number is Invalid")]
        public int RowNumber { get; set; }

        [Required(ErrorMessage = "VPhotoIdx is Invalid")]
        public string VPhotoIdx { get; set; }

        [Required(ErrorMessage = "PhotoId is Invalid")]
        public string PhotoId { get; set; }
        public List<ScoreData> ScoreData { get; set; }

        public string? Round { get; set; }
    }

    public class ScoreData
    {
        [Required(ErrorMessage = "Criteria is required")]
        public string Criteria { get; set; }

        [Required(ErrorMessage = "Score is required")]
        [Range(1, 10, ErrorMessage = "Score must be between 1 and 10")]
        public decimal Score { get; set; }
    }
}
