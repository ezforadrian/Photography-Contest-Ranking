using System.ComponentModel.DataAnnotations;

namespace photoCon.Models
{
    public class ReferenceCode
    {
        [Key]
        public int Index { get; set; }

        [Required]
        [StringLength(10)]
        public string RefTypeID { get; set; }

        [Required]
        [StringLength(10)]
        public string RefCodeID { get; set; }

        [Required]
        [StringLength(100)]
        public string RefCodeName { get; set; }

        [StringLength(250)]
        public string? RefCodeDesc { get; set; }

        [Required]
        public int IsActive { get; set; }

        public int? SortNumber { get; set; }

        [StringLength(100)]
        public string? Filler01 { get; set; }

        [StringLength(100)]
        public string? Filler02 { get; set; }

        [StringLength(100)]
        public string? Filler03 { get; set; }

        [Required]
        public DateTime EffectivityDate { get; set; }

        [Required]
        [StringLength(50)]
        public string CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDateTime { get; set; }

        [StringLength(50)]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDateTime { get; set; }
    }
}
