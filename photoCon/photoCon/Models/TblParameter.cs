using System;
using System.Collections.Generic;

namespace photoCon.Models
{
    public partial class TblParameter
    {
        public int ParamId { get; set; }
        public string? ParamDescription { get; set; }
        public string? ParamValue { get; set; }
    }
}
