using System;
using System.Collections.Generic;

namespace photoCon.Models
{
    public partial class TblTallyTransaction
    {
        public int TransId { get; set; }
        public int? EventId { get; set; }
        public int? JudgeId { get; set; }
        public int? PhotoId { get; set; }
        public int? StatusId { get; set; }
        public decimal? Score { get; set; }
        public int? RankNo { get; set; }
        public string? EncodedBy { get; set; }
        public DateTime? DateTimeEncoded { get; set; }
    }
}
