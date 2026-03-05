using System;
using System.Collections.Generic;

namespace photoCon.Models
{
    public partial class TblStatus
    {
        public int StatusId { get; set; }
        public string? StatusDescription { get; set; }
        public string? EncodedBy { get; set; }
        public DateTime? DateTimeEncoded { get; set; }
    }
}
