using FoodAppAPI.Models;

public class Item
{
    public int ItemId { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int CategoryId { get; set; }       // FK matches PK type
    public Category Category { get; set; } = null!;

    public string ItemName { get; set; } = null!;

    public string? ImageUrl { get; set; }   // save img url from cloud URL
    public string? PublicId { get; set; }   // (for delete link and img from cloud)

    public int Quantity { get; set; }
    public bool IsShoppingList { get; set; }
    public DateTime? AddedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }

    public bool IsExpiredProcessed { get; set; }

    public ICollection<WasteLog> WasteLogs { get; set; } = new List<WasteLog>();
}
