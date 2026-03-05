using System.ComponentModel.DataAnnotations;

namespace photoCon.Dto
{
    public class DisplayUpdate
    {
        [Required]
        public int RowNumber { get; set; }
        [Required]
        public string PhotoIdx { get; set; }
        [Required]
        public string ViewPhotoIdx { get; set; }
    }

    public class DisplayUpdateRegional
    {
        [Required]
        public int RowNumber { get; set; }
        [Required]
        public string PhotoIdx { get; set; }
        [Required]
        public string ViewPhotoIdx { get; set; }
        [Required]
        public string XCat { get; set; }
        [Required]
        public string XNum { get; set; }

    }
}
