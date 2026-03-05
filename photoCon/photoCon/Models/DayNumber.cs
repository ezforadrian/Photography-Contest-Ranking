using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace photoCon.Models
{
    public class DayNumber
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string Date_ { get; set; }

        [Required]
        [Column(TypeName = "nchar(10)")]
        public string Category { get; set; }

        [Required]
        public int DayNumber_ { get; set; }

        [Required]
        [Column(TypeName = "nchar(10)")]
        public string Status { get; set; }

        [Required]
        public bool IsActive { get; set; }

        public void ToggleStatus_()
        {
            Status = (Status.Trim() == "Open".Trim()) ? "Close".Trim() : "Open".Trim();
        }
    }
}
