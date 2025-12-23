namespace FoodAppAPI.Models
{
    public class WasteLog
    {
        public int WasteLogId { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int ItemId { get; set; }
        public Item Item { get; set; } = null!;
        public int QuantityWasted { get; set; }
        public string Reason { get; set; } = null!;
        public DateTime? DateWasted { get; set; }
    }
}
