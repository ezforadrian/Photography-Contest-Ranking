using System.ComponentModel.DataAnnotations;

namespace photoCon.Models
{
    public class CSV_Details
    {
        public string? id { get; set; }
        [Key]
        public string? photoname { get; set; }
        public string? photohash { get; set; }
        public string? phototitle { get; set; }
        public string? photodesc { get; set; }
        public string? location { get; set; }
        public string? datetaken { get; set; }
        public string? posted { get; set; }
        public string? size { get; set; }
        public string? mime { get; set; }
        public string? datetimeupdate { get; set; }
        public string? userid { get; set; }
    }
}
