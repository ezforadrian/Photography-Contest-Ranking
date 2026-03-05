using System;
using System.Collections.Generic;

namespace photoCon.Models
{
    public partial class TblEvent
    {
        public int EventId { get; set; }
        public string? EventName { get; set; }
        public int? RegionId { get; set; }
        public int? CategoryId { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
        public int? IsActive { get; set; }
        public string? EncodedBy { get; set; }
        public DateTime? DateTimeEncoded { get; set; }
    }
}
