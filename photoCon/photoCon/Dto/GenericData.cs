using System.ComponentModel.DataAnnotations;

namespace photoCon.Dto
{
    public class GenericData
    {
        [Required(ErrorMessage = "HashId is required")]
        public string HashId { get; set; }
    }


    public class GenericDataScore
    {
        [Required(ErrorMessage = "HashId is required")]
        public string HashId { get; set; }

        public string? HashRound { get; set; }
    }


}
