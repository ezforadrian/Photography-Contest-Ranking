using System.ComponentModel.DataAnnotations;

namespace photoCon.Dto
{
    public class SubmitBatchGrandFinal
    {
        [Required]
        public string hashRowIdProcessParam { get; set; }
        [Required]
        public string[] selectedCategories { get; set; }
        [Required]
        public bool Inclusion { get; set; }
    }
}
