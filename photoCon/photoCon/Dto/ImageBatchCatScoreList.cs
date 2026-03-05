using System.ComponentModel.DataAnnotations;

namespace photoCon.Dto
{
    public class ImageBatchCatScoreList
    {
        public string ImageHashId { get; set; }
        public int Sort { get; set; }
        public string Date { get; set; }
        public int DayNumber { get; set; } // No attributes needed for DayNumber
        public bool IsActive { get; set; }
        public string UserId { get; set; }
        public string Round { get; set; }
        public decimal Score { get; set; }
    }
}
