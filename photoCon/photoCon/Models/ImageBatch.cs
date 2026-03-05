using System.ComponentModel.DataAnnotations;

namespace photoCon.Models
{
    public class ImageBatch
    {
        [Key]
        public int Index { get; set; }

        [Required]
        [StringLength(100)]
        public string ImageHashId { get; set; }

        [Required]
        public int Sort { get; set; }

        [Required]
        [StringLength(50)]
        public string Date { get; set; }

        public int DayNumber { get; set; } // No attributes needed for DayNumber

        [Required]
        public bool IsActive { get; set; }
    }
}
