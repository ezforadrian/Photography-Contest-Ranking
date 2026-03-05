using System.ComponentModel.DataAnnotations;

namespace photoCon.Models
{
    public class ReferenceType
    {
        [Key]
        public int Index { get; set; }

        [Required]
        [StringLength(10)]
        public string RefTypeID { get; set; }

        [Required]
        [StringLength(100)]
        public string RefTypeName { get; set; }

        [StringLength(250)]
        public string? RefTypeDesc { get; set; } // Nullable

        [Required]
        public int IsActive { get; set; }

        [StringLength(100)]
        public string? Filler01 { get; set; } // Nullable

        [StringLength(100)]
        public string? Filler02 { get; set; } // Nullable

        [StringLength(100)]
        public string? Filler03 { get; set; } // Nullable

        [Required]
        public DateTime EffectivityDate { get; set; }

        [Required]
        [StringLength(50)]
        public string CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDateTime { get; set; }

        [StringLength(50)]
        public string? ModifiedBy { get; set; } // Nullable

        public DateTime? ModifiedDateTime { get; set; } // Nullable
    }
}
