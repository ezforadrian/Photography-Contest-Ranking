using System.ComponentModel.DataAnnotations;

namespace photoCon.Dto
{
    public class ProcessParameterView
    {
        public int Id_ { get; set; }
        public int Index { get; set; }
        public string HashIndex { get; set; }
        public string Process { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string? DetailedDescription { get; set; }
        public string ParameterValue { get; set; }
        public int IsActive { get; set; }
        public string? Filler01 { get; set; }
        public string? Filler02 { get; set; }
        public string? Filler03 { get; set; }
        public string? Filler04 { get; set; }
        public string? Filler05 { get; set; }
        public string? Filler06 { get; set; }
        public string? Filler07 { get; set; }
        public string? Filler08 { get; set; }
        public DateTime EffectivityDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDateTime { get; set; }
    }
}
