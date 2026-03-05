using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // Add this namespace

namespace photoCon.Models
{
    public partial class SeedControlTotal
    {
        [Key]
        public int Index { get; set; }
        public int RegionId { get; set; }
        public int CategoryId { get; set; }
        public DateTime? LastUpdate { get; set; }
        public int? DbCountFrom { get; set; }
        public int? SeededData { get; set; }
        public DateTime? ExecutedOn { get; set; }
    }
}
