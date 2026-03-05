using System.ComponentModel.DataAnnotations;

namespace photoCon.Dto
{
    public class SubmitBatch
    {
        [Required]  
        public string hashRowIdProcessParam { get; set; }
        [Required]
        public string[] selectedRegions { get; set; }
        [Required]
        public string[] selectedCategories { get; set; }
      

    }
}
